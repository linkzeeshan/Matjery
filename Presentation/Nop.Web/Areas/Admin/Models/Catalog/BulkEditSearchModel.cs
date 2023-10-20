using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using System.ComponentModel;

namespace Nop.Web.Areas.Admin.Models.Catalog
{
    /// <summary>
    /// Represents a product search model
    /// </summary>
    public partial class BulkEditSearchModel : BaseSearchModel
    {
        #region Ctor

        public BulkEditSearchModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableProductTypes = new List<SelectListItem>();
        }

        #endregion

        #region Properties

       // [NopResourceDisplayName("Admin.Catalog.BulkEdit.List.SearchProductName")]
        //[AllowHtml]
     //   [DisplayName("Product Name")]
        public string SearchProductName { get; set; }

       // [NopResourceDisplayName("Admin.Catalog.BulkEdit.List.SearchCategory")]
        //[DisplayName("Category Name")]
        public int SearchCategoryId { get; set; }

       // [NopResourceDisplayName("Admin.Catalog.BulkEdit.List.SearchManufacturer")]
        public int SearchManufacturerId { get; set; }
       // [NopResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
        public int SearchProductTypeId { get; set; }
        public IList<SelectListItem> AvailableProductTypes { get; set; }


        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableManufacturers { get; set; }
        public List<BulkEditProductModel> BulkEditListModel { get; set; }
        #endregion
    }
}