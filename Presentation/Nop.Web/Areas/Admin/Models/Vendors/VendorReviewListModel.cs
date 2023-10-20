using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Vendors
{
    public class VendorReviewListModel : BaseSearchModel
    {
        public IList<SelectListItem> AvailableStores { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.List.CreatedOnFrom")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnFrom { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.List.CreatedOnTo")]
        [UIHint("DateNullable")]
        public DateTime? CreatedOnTo { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.List.SearchStore")]
        public int SearchStoreId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.List.SearchText")]
        public string SearchText { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.List.SearchVendor")]
        public int SearchVendorId { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }

        public VendorReviewListModel()
        {
            this.AvailableStores = new List<SelectListItem>();
            this.AvailableVendors = new List<SelectListItem>();
        }
    }
}
