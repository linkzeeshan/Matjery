using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Web.Areas.Admin.Models.Vendors
{
    public class AdminVendorReviewModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Customer")]
        public int CustomerId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Customer")]
        public string CustomerInfo { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.IsApproved")]
        public bool IsApproved { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Rating")]
        public int Rating { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.ReviewText")]
        public string ReviewText { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Store")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Vendor")]
        public int VendorId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Vendor")]
        public string VendorName
        {
            get; set;
        }

        public string ReturnUrl { get; set; }
    }
}
