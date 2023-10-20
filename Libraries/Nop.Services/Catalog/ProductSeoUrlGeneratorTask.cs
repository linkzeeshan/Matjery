using Nop.Services.Seo;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Catalog
{
    public class ProductSeoUrlGeneratorTask : IScheduleTask
    {
        private readonly IProductService _productService;
        private readonly IUrlRecordService _urlRecordService;

        public ProductSeoUrlGeneratorTask(IProductService productService, IUrlRecordService urlRecordService)
        {
            _productService = productService;
            _urlRecordService = urlRecordService;
        }

        public virtual void Execute()
        {
            var products = _productService.SearchProducts(showHidden: false);
            foreach (var product in products)
            {
                var existingSeName = _urlRecordService.GetSeName(product);
                if (string.IsNullOrEmpty(existingSeName))
                {
                    //search engine name
                    var seName = _urlRecordService.ValidateSeName(product, existingSeName, product.Name, true);    //product.ValidateSeName("", product.Name, true);
                    _urlRecordService.SaveSlug(product, seName, 0);
                }
            }
        }
    }
}
