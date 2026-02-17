using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VirtualStore.Models;

namespace VirtualStore.Binders
{
    public class CartModelBinder : IModelBinder
    {
        private const string SessionKey = "Cart";

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var session = controllerContext.HttpContext.Session;
            var cart = session[SessionKey] as Cart;

            if (cart == null)
            {
                cart = new Cart();
                session[SessionKey] = cart;
            }

            return cart;
        }
    }
}