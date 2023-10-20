using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Web.Components
{
    public class HomepageProductsViewComponent : NopViewComponent
    {
        private readonly IAclService _aclService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IVendorService _vendorService;

        public HomepageProductsViewComponent(IAclService aclService,
            IProductModelFactory productModelFactory,
            IProductService productService,
            CatalogSettings catalogSettings,
            IVendorService vendorService,
            IStoreMappingService storeMappingService)
        {
            _aclService = aclService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _storeMappingService = storeMappingService;
            _catalogSettings = catalogSettings;
            _vendorService = vendorService;
        }

        public IViewComponentResult Invoke(int? productThumbPictureSize)
        {
            var NumberOfFeaturedProductsOnHomepage = _catalogSettings.NumberOfFeaturedProductsOnHomepage;
            if (NumberOfFeaturedProductsOnHomepage <= 0)
                NumberOfFeaturedProductsOnHomepage = 15;
            var products = _productService.GetAllProductsDisplayedOnHomePage(pageSize: NumberOfFeaturedProductsOnHomepage).ToList();
            for (int i = 0; i < products.Count; i++)
            {
                var vendor = _vendorService.GetVendorById(products[i].VendorId);
                if (!vendor.Active)
                {
                    products.RemoveAt(i);
                    i--;
                }
            }
            //ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();
            //availability dates
            products = products.Where(p => _productService.ProductIsAvailable(p)).ToList();

            products = products.Where(p => p.VisibleIndividually).ToList();

            if (!products.Any())
                return Content("");

            var model = _productModelFactory.PrepareProductOverviewModels(products, true, true, productThumbPictureSize).ToList();

            return View(model);
        }
    }
}