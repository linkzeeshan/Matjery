using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Vendors
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.Configuration.AllowAnonymousUsersToReviewVendor")]
        public bool AllowAnonymousUsersToReviewVendor { get; set; }

        public bool AllowAnonymousUsersToReviewVendor_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableWidgetZone_VendorReviews { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.Configuration.DefaultVendorRatingValue")]
        public int DefaultVendorRatingValue { get; set; }

        public bool DefaultVendorRatingValue_OverrideForStore { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.Configuration.VendorReviewsMustBeApproved")]
        public bool VendorReviewsMustBeApproved { get; set; }

        public bool VendorReviewsMustBeApproved_OverrideForStore { get; set; }

        [NopResourceDisplayName("Nop.Plugin.NopWarehouse.VendorReview.Admin.Configuration.VendorReviewsWidgetZone")]
        public string VendorReviewsWidgetZone { get; set; }

        public bool WidgetZone_VendorReviews_OverrideForStore { get; set; }

        public ConfigurationModel()
        {
            this.AvailableWidgetZone_VendorReviews = new List<SelectListItem>();
        }
    }
}
