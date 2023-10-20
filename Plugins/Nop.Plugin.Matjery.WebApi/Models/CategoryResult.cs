using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public class CategoryResult : BaseNopModel
    {
        public CategoryResult()
        {
            Products = new List<ProductResult>();
        }

        public List<ProductResult> Products { get; set; }

        public string Description { get; set; }

        public string FullSizeImageUrl { get; set; }

        public int Id { get; set; }

        public string ImageUrl { get; set; }

        public string Name { get; set; }

        public int ParentCategoryId { get; set; }

        public bool Published { get; set; }

        public string SeName { get; internal set; }
        public int NumberOfProducts { get; internal set; }
        public string ImageBase64 { get; set; }
        public bool? TradeLicenseRequired { get; set; }
    }
}
