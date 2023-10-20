using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Plugin.NopWarehouse.VendorReview.ViewEngines;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.NopWarehouse.VendorReview.Infrastrcture
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(VendorReviewDefaults.ConfigurationRouteName, "Plugins/VendorReview/Configure",
              new { controller = "VendorReview", action = "Configure", area = AreaNames.Admin });

            //System.Web.Mvc.ViewEngines.Engines.Add(new CustomViewEngine());
            //endpointRouteBuilder.MapControllerRoute("SetVendorReviewHelpfulness",
            //     "setvendorreviewhelpfulness",
            //     new { controller = VendorReviewDefaults.VENDOR_REVIEW_CONTROLLER, action = "SetVendorReviewHelpfulness" },
            //     new string[] { VendorReviewDefaults.VENDOR_REVIEW_NAMESPACES }
            // );
            // endpointRouteBuilder.MapControllerRoute(VendorReviewDefaults.VendorReview, "Plugins/VendorReview/List",
            // new { controller = "VendorReview", action = "List" , area = AreaNames.Admin });


            // endpointRouteBuilder.MapControllerRoute("SetVendorReviewHelpfulness", "Plugins/VendorReview/SetVendorReviewHelpfulness",
            //new { controller = "VendorReview", action = "SetVendorReviewHelpfulness" });

            //endpointRouteBuilder.MapControllerRoute(VendorReviewDefaults.VendorReview, "Plugins/VendorReview/List",
            //new { controller = "VendorReview", action = "List", area = AreaNames.Admin });

            endpointRouteBuilder.MapControllerRoute("Nop.Plugin.NopWarehouse.VendorReview.List",
                "Admin/Plugins/VendorReview/VendorReview/List",
                   new { controller = "VendorReview", action = "List", area = AreaNames.Admin });



            //           endpointRouteBuilder.MapControllerRoute("Nop.Plugin.NopWarehouse.VendorFollower.List", "Plugins/VendorFollower/List",
            //        new { controller = "VendorFollower", action = "List", area = AreaNames.Admin });
            //           //endpointRouteBuilder.MapControllerRoute("Nop.Plugin.NopWarehouse.VendorFollower.List",
            //           //    "Admin/Plugins/VendorFollower/VendorFollower/List",
            //           //    new { controller = VendorReviewDefaults.VENDOR_FOLLOWER_CONTROLLER, action = "List", area = AreaNames.Admin },
            //           //    new string[] { VendorReviewDefaults.VENDOR_REVIEW_NAMESPACES });
            //           endpointRouteBuilder.MapControllerRoute("Nop.Plugin.NopWarehouse.VendorReview.Edit", "Plugins/VendorReview/Edit/{id}",
            //new { controller = "VendorReview", action = "Edit", area = AreaNames.Admin });

            //endpointRouteBuilder.MapControllerRoute("Nop.Plugin.NopWarehouse.VendorReview.Edit",
            //    "Admin/Plugins/VendorReview/VendorReview/Edit/{id}",
            //    new { controller = VendorReviewDefaults.VENDOR_REVIEW_CONTROLLER, action = "Edit", area = AreaNames.Admin },
            //    new string[] { VendorReviewDefaults.VENDOR_REVIEW_NAMESPACES });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => 0;
    }
}
