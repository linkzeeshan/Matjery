using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Components
{
    public class HomepageDiscountedProductsViewComponent: NopViewComponent
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly IProductModelFactory _productModelFactory;
        public HomepageDiscountedProductsViewComponent(CatalogSettings catalogSettings,
            IProductService productService,
            IVendorService vendorService,
            IProductModelFactory productModelFactory)
        {
            _catalogSettings = catalogSettings;
            _productService = productService;
            _vendorService = vendorService;
            _productModelFactory = productModelFactory;
        }

        public IViewComponentResult Invoke(int? productThumbPictureSize)
        {
            var NumberOfDiscountedProductsOnHomepage =  _catalogSettings.NumberOfDiscountedProductsOnHomepage;
            if (NumberOfDiscountedProductsOnHomepage <= 0)
                NumberOfDiscountedProductsOnHomepage = 6;
            IPagedList<Product> products = _productService.GetAllProductsDiscounted(1,pageSize: NumberOfDiscountedProductsOnHomepage);
            for (int i = 0; i < products.Count; i++)
            {
                var vendor = _vendorService.GetVendorById(products[i].VendorId);
                if (vendor != null)
                {
                    if (!vendor.Active)
                    {
                        products.RemoveAt(i);
                        i--;
                    }

                }
             
            }
            if (!products.Any())
                return Content("");

            var model = _productModelFactory.PrepareProductOverviewModels(products, true, true, productThumbPictureSize).ToList();
            return View(model);
        }



    }
}
