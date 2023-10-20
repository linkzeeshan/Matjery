using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Interface
{
    public interface IProductPluginService
    {
        List<ProductResult> GetHomePageProducts();
        List<ProductResult> GetHomePageBestSeller();
        List<ProductResult> GetHomePageDiscountedPage(int pageIndex, int pageSize);
        ProductDetailsModel GetProductDetails(int productId);
    }
}
