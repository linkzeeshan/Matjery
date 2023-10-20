using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Vendors;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Nop.Plugin.NopWarehouse.VendorReview
{
    public class VendorReviewPlugin : BasePlugin, IWidgetPlugin, IPlugin, IAdminMenuPlugin
    {

        #region Constants & Fields

        private readonly IWorkContext _workContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly WidgetSettings _widgetSettings;
        private readonly VendorReviewSettings _vendorRatingSettings;
        private readonly ICustomerService _customerService;
        private readonly IPluginService _pluginService;
        private readonly IWebHelper _webHelper;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        #endregion

        #region Constructor 

        public VendorReviewPlugin(IWorkContext workContext, ISettingService settingService, ILocalizationService localizationService,
            WidgetSettings widgetSettings,
             IPluginService pluginService,
             IUrlHelperFactory urlHelperFactory,
        ICustomerService customerService,
        IWebHelper webHelper,
        IActionContextAccessor actionContextAccessor,

             VendorReviewSettings vendorRatingSettings)
        {
            _webHelper = webHelper;
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _customerService = customerService;
            this._workContext = workContext;
            this._settingService = settingService;
            this._localizationService = localizationService;
            this._widgetSettings = widgetSettings;
            this._vendorRatingSettings = vendorRatingSettings;
            _pluginService = pluginService;
        }
        #endregion

        public bool HideInWidgetList => false;

        #region methods
        public string GetWidgetViewComponentName(string widgetZone)
        {
            return VendorReviewDefaults.TRACKING_VIEW_COMPONENT_NAME;
        }
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "VendorReview";
            routeValues = new RouteValueDictionary()
            {
                { "Namespaces", "Nop.Plugin.NopWarehouse.VendorReview.Controllers" },
                { "area", null }
            };
        }
        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(VendorReviewDefaults.ConfigurationRouteName);
            //return $"{_webHelper.GetStoreLocation()}Admin/VendorReview/Configure";
        }

        public IList<string> GetWidgetZones()
        {
            return new List<string>()
            {
                this._vendorRatingSettings.VendorReviewsWidgetZone
            };
        }
        public virtual IWidgetPlugin LoadWidgetBySystemName(string systemName)
        {
            var descriptor = _pluginService.GetPluginDescriptorBySystemName<IWidgetPlugin>(systemName);
            if (descriptor != null)
                return descriptor.Instance<IWidgetPlugin>();

            return null;
        }
        public void ManageSiteMap(SiteMapNode rootNode)
        {
            if ( _customerService.IsAdmin(_workContext.CurrentCustomer))
            {
                SiteMapNode pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "NopWarehouse");
                if (pluginNode == null)
                {
                    IList<SiteMapNode> childNodes = rootNode.ChildNodes;
                    SiteMapNode siteMapNode = new SiteMapNode
                    {
                        SystemName = "NopWarehouse",
                        Title = "NopWarehouse",
                        Visible = true
                    };
                    childNodes.Add(siteMapNode);
                    pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "NopWarehouse");
                }
                IWidgetPlugin vendorRatingPlugin =LoadWidgetBySystemName("NopWarehouse.VendorReview");
                SiteMapNode pluginMenu = new SiteMapNode
                {
                    Title = _localizationService.GetLocalizedFriendlyName(vendorRatingPlugin, this._workContext.WorkingLanguage.Id),
                    Visible = true
                };
                SiteMapNode siteMapNode1 = new SiteMapNode
                {
                    Title = this._localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Configuration"),
                    Url = "~/Admin/VendorReview/Configure",
                    Visible = true,
                    RouteValues = new RouteValueDictionary()
                {
                    {"Namespaces", "Nop.Plugin.NopWarehouse.VendorReview"},
                    {"area", "admin"}
                }
                };
                pluginMenu.ChildNodes.Add(siteMapNode1);

                SiteMapNode siteMapNode2 = new SiteMapNode();
                siteMapNode2.Title = this._localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.ManageVendorReviews");
                siteMapNode2.Url = "~/Admin/Plugins/VendorReview/VendorReview/List";
                siteMapNode2.Visible = true;
                siteMapNode2.RouteValues = new RouteValueDictionary()
            {
                { "Namespaces", "Nop.Plugin.NopWarehouse.VendorReview" },
                { "area", "admin" }
            };
                pluginMenu.ChildNodes.Add(siteMapNode2);
                pluginNode.ChildNodes.Add(pluginMenu);
            }
        }
        #endregion

        #region Install / Uninstall
       
        public override void Uninstall()
        {
            //settings
            this._settingService.DeleteSetting<VendorReviewSettings>();

            //locales
            _localizationService.DeletePluginLocaleResources("Nop.Plugin.NopWarehouse.VendorReview");

            base.Uninstall();
        }

        public override void Install()
        {
            try
            {

                //locales
                _localizationService.AddPluginLocaleResource(new Dictionary<string, string>
                {
                    ["Nop.Plugin.NopWarehouse.VendorReview.Configuration"] = "[Configuration]",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.Configuration.VendorReviewsWidgetZone"] = "WidgetZone - vendor reviews",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.Configuration.VendorReviewsWidgetZone.hint"] = "WidgetZone - vendor reviews",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.Configuration.DefaultVendorRatingValue"] = "Default vendor rating value",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.Configuration.DefaultVendorRatingValue.hint"] = "Default vendor rating value",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.Configuration.AllowAnonymousUsersToReviewVendor"] = "Allow anonymous users to write vendor reviews",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.Configuration.AllowAnonymousUsersToReviewVendor.hint"] = "Check to allow anonymous users to write vendor reviews.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.Configuration.VendorReviewsMustBeApproved"] = "Vendor reviews must be approved",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.Configuration.VendorReviewsMustBeApproved.hint"] = "Check if vendor reviews must be approved by administrator.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.DefaultVendorRatingValue.Range"] = "Default vendor rating must be from 1 to 5.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.Fields.Title.Required"] = "Title is required.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.Fields.Title.MaxLengthValidation"] = "Max length of vendor review title is {0} chars",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.Fields.ReviewText.Required"] = "Review text is required.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.Fields.Title"] = "Review title",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.Fields.ReviewText"] = "Review text",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.Fields.Rating"] = "Rating",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.OnlyRegisteredUsersCanWriteReviews"] = "Only registered users can write reviews",
                    ["Nop.Plugin.NopWarehouse.VendorReview.ActivityLog.PublicStore.AddVendorReview"] = "Added a vendor review ['{0}']",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.SeeAfterApproving"] = "You will see the vendor review after approving by a store administrator.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.SuccessfullyAdded"] = "Vendor review is successfully added.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.Helpfulness.OnlyRegistered"] = "Only registered customers can set review helpfulness",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.Helpfulness.YourOwnReview"] = "You cannot vote for your own review",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.Helpfulness.SuccessfullyVoted"] = "Successfully voted",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.Fields.Rating.Bad"] = "Bad",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.Fields.Rating.Excellent"] = "Excellent",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.SubmitButton"] = "Submit review",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.From"] = "From",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.Date"] = "Date",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.Helpfulness.WasHelpful?"] = "Was this review helpful?",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews"] = "Reviews",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.List.CreatedOnFrom"] = "Created from",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.List.CreatedOnFrom.hint"] = "The creation from date for the search.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.List.CreatedOnTo"] = "Created to",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.List.CreatedOnTo.hint"] = "The creation to date for the search.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.List.SearchText"] = "Message",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.List.SearchText.hint"] = "Search in title and review text.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.List.SearchVendor"] = "Vendor",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.List.SearchVendor.hint"] = "Search by a specific vendor.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.List.SearchStore"] = "Store",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.List.SearchStore.hint"] = "Search by a specific store.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Store"] = "Store",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Store.hint"] = "A store name in which this review was written.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Vendor"] = "Vendor",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Vendor.hint"] = "The name of the vendor that the review is for.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Customer"] = "Customer",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Customer.hint"] = "The customer who created the review.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Title"] = "Title",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Title.hint"] = "The title of the vendor review.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.ReviewText"] = "Review text",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.ReviewText.hint"] = "The review text",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Rating"] = "Rating",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Rating.hint"] = "The customer's vendor rating.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.IsApproved"] = "Is approved",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.IsApproved.hint"] = "Is the review approved? Marking it as approved means that it is visible to all your site's visitors.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.CreatedOn"] = "Created on",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.CreatedOn.hint"] = "The date/time that the review was created.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.Title.Required"] = "Title is required.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Fields.ReviewText.Required"] = "Review text is required.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Updated"] = "The vendor review has been updated successfully.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Deleted"] = "The vendor review has been deleted successfully.",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews"] = "Vendor reviews",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.ApproveSelected"] = "Approve selected",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.DisapproveSelected"] = "Disapprove selected",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.DeleteSelected"] = "Delete selected",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.EditVendorReviewDetails"] = "Edit vendor review details",
                    ["Nop.Plugin.NopWarehouse.VendorReview.ManageVendorReviews"] = "Manage Vendor Reviews",
                    ["Nop.Plugin.NopWarehouse.VendorReview.BackToList"] = "Back to list",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.ViewReviews"] = "View reviews",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Reviews.HideReviews"] = "Hide reviews",
                    //followers
                    ["Nop.Plugin.NopWarehouse.VendorReview.Followers.Follow"] = "Follow",
                    ["Nop.Plugin.NopWarehouse.VendorReview.Followers.Admin.VendorFollowers"] = "Followers",
                    ["nop.plugin.nopwarehouse.vendorreview.admin.vendorfollowers.fields.customer"] = "Customer",
                    ["nop.plugin.nopwarehouse.vendorreview.admin.vendorfollowers.fields.followonutc"] = "Follow On",
                    ["nop.plugin.nopwarehouse.vendorreview.admin.vendorfollowers.fields.unfollowonutc"] = "UnFollow On",
                    ["nop.plugin.nopwarehouse.vendorreview.admin.vendorfollowers.fields.Following"] = "Following"

                });
                VendorReviewSettings vendorRatingSetting = new VendorReviewSettings()
                {
                    VendorReviewsWidgetZone = "vendordetails_top",
                    DefaultVendorRatingValue = 5,
                    AllowAnonymousUsersToReviewVendor = false,
                    VendorReviewsMustBeApproved = false
                };
                this._settingService.SaveSetting(vendorRatingSetting);

                base.Install();
            }
            catch (Exception exception)
            {
                throw new NopException(exception.Message);
            }
        }

      

        #endregion

    }
}
