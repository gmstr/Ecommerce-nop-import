using System;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Settings;
using MrCMS.Tasks;
using MrCMS.Web.Apps.Core.MessageTemplates;
using MrCMS.Web.Apps.Core.Models;
using MrCMS.Website;
using NHibernate;

namespace MrCMS.Web.Apps.Core.Services
{
    public interface IResetPasswordService
    {
        void SetResetPassword(User user);
        void ResetPassword(ResetPasswordViewModel model);
    }

    public class ResetPasswordService : IResetPasswordService
    {
        private readonly ISession _session;
        private readonly SiteSettings _siteSettings;
        private readonly IUserService _userService;
        private readonly IPasswordManagementService _passwordManagementService;
        private readonly IMessageParser<ResetPasswordMessageTemplate, User> _messageParser;
        private readonly MailSettings _mailSettings;

        public ResetPasswordService(ISession session, SiteSettings siteSettings, MailSettings mailSettings, IUserService userService, IPasswordManagementService passwordManagementService, IMessageParser<ResetPasswordMessageTemplate, User> messageParser)
        {
            _session = session;
            _siteSettings = siteSettings;
            _mailSettings = mailSettings;
            _userService = userService;
            _passwordManagementService = passwordManagementService;
            _messageParser = messageParser;
        }

        public void SetResetPassword(User user)
        {
            user.ResetPasswordExpiry = CurrentRequestData.Now.AddDays(1);
            user.ResetPasswordGuid = Guid.NewGuid();
            _userService.SaveUser(user);

            var queuedMessage = _messageParser.GetMessage(user);

            _session.Transact(session => session.SaveOrUpdate(queuedMessage));

            //to do - is this needed with new task system?

            TaskExecutor.ExecuteLater(new SendQueuedMessagesTask(_mailSettings, _siteSettings));
        }

        public void ResetPassword(ResetPasswordViewModel model)
        {
            var user = _userService.GetUserByEmail(model.Email);

            if (user.ResetPasswordGuid == model.Id && user.ResetPasswordExpiry > CurrentRequestData.Now &&
                _passwordManagementService.ValidatePassword(model.Password, model.ConfirmPassword))
            {
                _passwordManagementService.SetPassword(user, model.Password, model.ConfirmPassword);

                user.ResetPasswordExpiry = null;
                user.ResetPasswordGuid = null;

                _userService.SaveUser(user);
            }
            else
                throw new InvalidOperationException("Unable to reset password, resend forgotten password email");
        }
    }
}