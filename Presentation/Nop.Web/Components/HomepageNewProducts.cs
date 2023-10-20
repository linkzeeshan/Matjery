using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Components
{
    public class HomepageNewProductsViewComponent: NopViewComponent
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;
        private readonly IProductModelFactory _productModelFactory;

        public HomepageNewProductsViewComponent(
            CatalogSettings catalogSettings,
            IProductService productService,
            IVendorService vendorService,
            IProductModelFactory productModelFactory)
        {
            _catalogSettings = catalogSettings;
            _productService = productService;
            _vendorService = vendorService;
            _productModelFactory = productModelFactory;
        }

        public IViewComponentResult Invoke()
        {
            if (!_catalogSettings.NewProductsEnabled)
                return Content("");
            var pageSize = _catalogSettings.NumberOfNewProductsOnHomepage;
            if (pageSize < 1)
            {
                pageSize = 9;
            }

            var products = _productService.GetRecentlyAddedProducts(pageSize: pageSize,
                      pageIndex: 0);

            for (int i = 0; i < products.Count; i++)
            {
                var vendor = _vendorService.GetVendorById(products[i].VendorId);
                if (!vendor.Active)
                {
                    products.RemoveAt(i);
                    i--;
                }
            }
            var model = new List<ProductOverviewModel>();
            model.AddRange(_productModelFactory.PrepareProductOverviewModels(products));
            return View(model);
        }
    }
}
