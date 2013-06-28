﻿using System.Web.Mvc;
using MrCMS.Web.Apps.Ecommerce.Services.Cart;
using MrCMS.Website.Controllers;
using MrCMS.Web.Apps.Ecommerce.Pages;

namespace MrCMS.Web.Apps.Ecommerce.Controllers
{
    public class OrderPlacedController : MrCMSAppUIController<EcommerceApp>
    {
        private readonly IGetCart _getCart;

        public OrderPlacedController(IGetCart getCart)
        {
            _getCart = getCart;
        }

        public ViewResult Show(OrderPlaced page,int orderID=0)
        {
            ViewBag.orderID = orderID;
            return View(page);
        }
    }
}