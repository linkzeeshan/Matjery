using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.NopWarehouse.VendorReview
{
    public  class VendorReviewDefaults
    {
        public static string VendorReview => "Plugin.Misc.NopWarehouse.VendorReview.List";

        public const string TRACKING_VIEW_COMPONENT_NAME = "WidgetsVendorReview";

        public static string ConfigurationRouteName => "Plugin.Widgets.VendorReview.Configure";
        public static string SystemName => "NopWarehouse.VendorReview";

  
        public static string PartnerName => "MAQTA";
        public const string VENDOR_REVIEW_CONTROLLER = "VendorReview";
        public const string VENDOR_FOLLOWER_CONTROLLER = "VendorFollower";
        public const string VENDOR_REVIEW_NAMESPACES = "Nop.Plugin.NopWarehouse.VendorReview.Controllers";

    }
}
