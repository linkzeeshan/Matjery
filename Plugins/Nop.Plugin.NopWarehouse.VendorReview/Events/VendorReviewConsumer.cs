using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Nop.Plugin.NopWarehouse.VendorReview.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.NopWarehouse.VendorReview.Events
{
    /// <summary>
    /// This class is used to detect when a vendor is being edited in order to add a new tab for our vendor review.
    /// </summary>
    public class VendorReviewConsumer : IConsumer<AdminTabStripCreated>
    {
        private readonly IProductService _productService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelperFactory factory;

        public VendorReviewConsumer(IProductService productService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService, IActionContextAccessor actionContextAccessor,
            IGenericAttributeService genericAttributeService, IPermissionService permissionService, IUrlHelperFactory factory)
        {
            this.factory = factory;
            _httpContextAccessor = httpContextAccessor;
            this._productService = productService;
            this._localizationService = localizationService;
            this._genericAttributeService = genericAttributeService;
            this._permissionService = permissionService;
            _actionContextAccessor = actionContextAccessor;
        }

        public void HandleEvent(AdminTabStripCreated eventMessage)
        {
            if (eventMessage.TabStripName == "vendor-edit")
            {
                int vendorId = Convert.ToInt32(_httpContextAccessor.HttpContext.Request.RouteValues["id"]);
         
                var routeValues = new RouteValueDictionary()
                                  {
                                      {"Namespaces", "Nop.Plugin.NopWarehouse.VendorReview.Controllers"},
                                      {"area", null},
                                      {"id", vendorId},
                                  };
                string tabReviewsText = _localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews");
                string tabFollowersText = _localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Followers.Admin.VendorFollowers");
                var urlHelper = factory.GetUrlHelper(_actionContextAccessor.ActionContext).Action("ListPartial", "VendorReview", routeValues);
                eventMessage.BlocksToRender.Add(TabStripHelper.RenderAdminTab("vendor-edit", tabReviewsText, urlHelper, "tab-vendor-reviews"));
                var urlFollowerHelper = factory.GetUrlHelper(_actionContextAccessor.ActionContext).Action("ListPartial", "VendorFollower", routeValues);
                eventMessage.BlocksToRender.Add(TabStripHelper.RenderAdminTab("vendor-edit", tabFollowersText, urlFollowerHelper, "tab-vendor-followers"));
            }
        }
    }
}
