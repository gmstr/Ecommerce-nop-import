using System.Linq;
using MrCMS.Services.Resources;
using MrCMS.Web.Apps.Ecommerce.Entities.Cart;
using MrCMS.Web.Apps.Ecommerce.Entities.DiscountLimitations;
using MrCMS.Web.Apps.Ecommerce.Models;

namespace MrCMS.Web.Apps.Ecommerce.Services.Cart
{
    public class CartHasAtLeastXItemsChecker : DiscountLimitationChecker<CartHasAtLeastXItems>
    {
        private readonly IStringResourceProvider _stringResourceProvider;
        public CartHasAtLeastXItemsChecker(IStringResourceProvider stringResourceProvider)
        {
            _stringResourceProvider = stringResourceProvider;
        }

        public override CheckLimitationsResult CheckLimitations(CartHasAtLeastXItems limitation, CartModel cart)
        {
            return cart.ItemQuantity >= limitation.NumberOfItems
                ? CheckLimitationsResult.Successful(Enumerable.Empty<CartItem>())
                : CheckLimitationsResult.CurrentlyInvalid(
                    string.Format(
                        _stringResourceProvider.GetValue("You need at least {0} items to apply this discount"),
                        limitation.NumberOfItems));
        }
    }
}