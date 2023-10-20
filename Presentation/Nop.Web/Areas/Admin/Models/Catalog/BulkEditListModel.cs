using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    public partial class BulkEditListModel : BasePagedListModel<BulkEditProductModel>
    {
        public BulkEditListModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableProductTypes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.Catalog.BulkEdit.List.SearchProductName")]
        //[AllowHtml]
        [DisplayName("Product Name")]
        public string SearchProductName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.BulkEdit.List.SearchCategory")]
        [DisplayName("Category Name")]
        public int SearchCategoryId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.BulkEdit.List.SearchManufacturer")]
        public int SearchManufacturerId { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
        public int SearchProductTypeId { get; set; }
        public IList<SelectListItem> AvailableProductTypes { get; set; }


        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableManufacturers { get; set; }
    }
}
