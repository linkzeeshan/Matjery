using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Shipping;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Checkout;

namespace Nop.Web.Components
{
    public class CheckoutProgressViewComponent : NopViewComponent
    {
        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly ShippingSettings _shippingSettings;

        public CheckoutProgressViewComponent(ICheckoutModelFactory checkoutModelFactory,ShippingSettings 
            shippingSettings)
        {
            _checkoutModelFactory = checkoutModelFactory;
            _shippingSettings = shippingSettings;
        }

        public IViewComponentResult Invoke(CheckoutProgressStep step)
        {
            var model = _checkoutModelFactory.PrepareCheckoutProgressModel(step);
          
            return View(model);
        }
    }
}
