using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Components
{
    public class ProductReviewPopupViewComponent : NopViewComponent
    {
        private readonly ICustomerModelFactory _customerModelFactory;
        public ProductReviewPopupViewComponent(ICustomerModelFactory customerModelFactory)
        {
            _customerModelFactory = customerModelFactory;
        }
        public IViewComponentResult Invoke()
        {
            IEnumerable<CustomerProductReviewModel> customerProductReviewModels = new List<CustomerProductReviewModel>();
            customerProductReviewModels = _customerModelFactory.GetProductsForReviews();
            return View(customerProductReviewModels);
        }
    }
}
