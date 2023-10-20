using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Vendors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Controllers
{
    public class VendorReviewPublicController : BasePublicController
    {
        #region Constants & Fields 

        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IVendorService _vendorService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IStoreContext _storeContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly CustomerSettings _customerSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;

        private readonly OrderSettings _orderSettings;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerService _customerService;
        private readonly IVendorRatingService _vendorRatingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly INotificationService _notificationService;
      


        #endregion
        #region Constructor 

        public VendorReviewPublicController(IPermissionService permissionService, IPictureService pictureService, IVendorService vendorService,
            ICountryService countryService, IStateProvinceService stateProvinceService, IStaticCacheManager cacheManager,
            ILocalizationService localizationService, IWorkContext workContext, IOrderService orderService, IProductService productService,
            CatalogSettings catalogSettings, IWorkflowMessageService workflowMessageService, ICustomerActivityService customerActivityService,
            LocalizationSettings localizationSettings, ISettingService settingService, IStoreService storeService, IStoreContext storeContext,
            IDateTimeHelper dateTimeHelper, CustomerSettings customerSettings, RewardPointsSettings rewardPointsSettings,
            OrderSettings orderSettings, IWebHelper webHelper, ICustomerService customerService,
            INotificationService notificationService,
  
        IVendorRatingService vendorRatingService, IGenericAttributeService genericAttributeService)
        {
     
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _vendorService = vendorService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _cacheManager = cacheManager;
            _localizationService = localizationService;
            _workContext = workContext;
            _orderService = orderService;
            _productService = productService;
            _catalogSettings = catalogSettings;
            _workflowMessageService = workflowMessageService;
            _customerActivityService = customerActivityService;
            _localizationSettings = localizationSettings;
            _settingService = settingService;
            _storeService = storeService;
            _storeContext = storeContext;
            _dateTimeHelper = dateTimeHelper;
            _customerSettings = customerSettings;
            _rewardPointsSettings = rewardPointsSettings;
      
            _orderSettings = orderSettings;
            _webHelper = webHelper;
            _customerService = customerService;
            _vendorRatingService = vendorRatingService;
            _genericAttributeService = genericAttributeService;
        }

        #endregion
        
        public IActionResult Index()
        {
            return View();
        }
        //[ActionName("VendorReviews")]
        //[FormValueRequired("add-vendor-review")]
        [HttpPost]
        public virtual IActionResult VendorReviewsAdd(VendorReviewsModel model)
        {
            TempData["Message"] = string.Empty;
            ActionResult route=RedirectToRoute("HomePage");
            int vendorId = model.VendorId;
            Vendor vendor = _vendorService.GetVendorById(model.VendorId);
            if (vendor != null && vendor.Active && !vendor.Deleted)
            {
                int storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext, _genericAttributeService);
                VendorReviewSettings vendorReviewSettings = _settingService.LoadSetting<VendorReviewSettings>(storeScope);
                if (_customerService.IsGuest(_workContext.CurrentCustomer) && !vendorReviewSettings.AllowAnonymousUsersToReviewVendor)
                    ModelState.AddModelError("", _localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Reviews.OnlyRegisteredUsersCanWriteReviews"));

                model.IsDisplayed = true;
                string validationCode = _genericAttributeService.GetAttribute<string>(_workContext.CurrentCustomer, string.Concat("VendorRatingForm-", vendorId.ToString()),
                    _storeContext.CurrentStore.Id);
                string vendorCode = Request.Form["validation-code"];
                if (!ModelState.IsValid)    
                {
                    PrepareVendorReviewsModel(model, vendor, vendorReviewSettings);
                    //int vendorId,  bool isEmptyModel = true)
                    return RedirectToAction("vendor","catalog",new { vendorId = vendor.Id});
                    //return ViewComponent("vendorreview", new { vendorId = vendor.Id, isEmptyModel = true });
                    //route = RedirectToPage("vendor"); ;// VendorReviews(vendor.Id, model, false);
                }
                else
                {
                    int rating = model.AddVendorReview.Rating;
                    if (rating < 1 || rating > 5)
                    {
                        rating = vendorReviewSettings.DefaultVendorRatingValue;
                    }
                    bool isApproved = !vendorReviewSettings.VendorReviewsMustBeApproved;
                    Nop.Core.Domain.Vendors.VendorReview vendorReview = new Nop.Core.Domain.Vendors.VendorReview
                    {
                        VendorId = vendor.Id,
                        CustomerId = _workContext.CurrentCustomer.Id,
                        Title = model.AddVendorReview.Title,
                        ReviewText = model.AddVendorReview.ReviewText,
                        Rating = rating,
                        HelpfulYesTotal = 0,
                        HelpfulNoTotal = 0,
                        IsApproved = isApproved,
                        CreatedOnUtc = DateTime.UtcNow,
                        StoreId = _storeContext.CurrentStore.Id
                    };
                    _vendorRatingService.InsertVendorReview(vendorReview);
                    // _customerActivityService.InsertActivity("PublicStore.AddVendorReview", _localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.ActivityLog.PublicStore.AddVendorReview"), new object[] { vendor.Name });
                    model.AddVendorReview.Title = null;
                    model.AddVendorReview.ReviewText = null;
                    model.AddVendorReview.SuccessfullyAdded = true;
                    if (isApproved)
                    {
                        model.AddVendorReview.Result = _localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Reviews.SuccessfullyAdded");
                    }
                    else
                    {
                        model.AddVendorReview.Result = _localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Reviews.SeeAfterApproving");
                    }
                    TempData["Message"] = model.AddVendorReview.Result;
                    return RedirectToAction("vendor", "catalog", new { vendorId = vendor.Id });

                }
            }
            else
            {
                route = RedirectToRoute("HomePage");
            }
            return route;
        }


        [NonAction]
        protected void PrepareVendorReviewsModel(VendorReviewsModel model, Vendor vendor, VendorReviewSettings vendorReviewSettings)
        {
            if (vendor == null)
                throw new ArgumentNullException(nameof(vendor));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            string submitButtonText = _localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Reviews.SubmitButton");
            Guid code = Guid.NewGuid();
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, string.Concat("VendorRatingForm-", vendor.Id.ToString()), code.ToString(), _storeContext.CurrentStore.Id);
            model.Button = string.Format("<input type=\"submit\" name=\"add-vendor-review\" class=\"button-1 write-product-review-button\" value=\"{0}\" /><input type=\"hidden\" name=\"validation-code\" value=\"{1}\" />", submitButtonText, code);
            int storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext, _genericAttributeService);
            _settingService.LoadSetting<VendorReviewSettings>(storeScope);
            model.VendorId = vendor.Id;
            model.VendorName = _localizationService.GetLocalized(vendor, x => x.Name);
            DateTime? nullable = null;
            DateTime? nullable1 = null;
            nullable = null;
            IPagedList<Nop.Core.Domain.Vendors.VendorReview> vendorReviews = _vendorRatingService.GetAllVendorReviews(0, vendor.Id, _storeContext.CurrentStore.Id, true);
            model.RatingSum = 0;
            model.TotalReviews = 0;
            foreach (Nop.Core.Domain.Vendors.VendorReview vr in vendorReviews)
            {
                Customer customer = vr.Customer;
                IList<VendorReviewModel> items = model.Items;
                VendorReviewModel vendorReviewModel = new VendorReviewModel();
                ((BaseNopEntityModel)vendorReviewModel).Id = vr.Id;
                vendorReviewModel.CustomerId = vr.CustomerId;
                vendorReviewModel.CustomerName = _customerService.FormatUsername(customer, false, 0);
                vendorReviewModel.AllowViewingProfiles = _customerSettings.AllowViewingProfiles && customer != null && !_customerService.IsGuest(customer);
                vendorReviewModel.Title = vr.Title;
                vendorReviewModel.ReviewText = vr.ReviewText;
                vendorReviewModel.Rating = vr.Rating;
                vendorReviewModel.Helpfulness = new VendorReviewHelpfulnessModel
                {
                    VendorReviewId = vr.Id,
                    HelpfulYesTotal = vr.HelpfulYesTotal,
                    HelpfulNoTotal = vr.HelpfulNoTotal
                };
                DateTime userTime = _dateTimeHelper.ConvertToUserTime(vr.CreatedOnUtc, DateTimeKind.Utc);
                vendorReviewModel.WrittenOnStr = userTime.ToString("g");
                items.Add(vendorReviewModel);
                model.RatingSum = model.RatingSum + vr.Rating;
                VendorReviewsModel totalReviews = model;
                totalReviews.TotalReviews = totalReviews.TotalReviews + 1;
            }
            model.AddVendorReview.CanCurrentCustomerLeaveReview = vendorReviewSettings.AllowAnonymousUsersToReviewVendor || !_customerService.IsGuest(_workContext.CurrentCustomer);
        }


        public virtual int GetActiveStoreScopeConfiguration(IStoreService storeService, IWorkContext workContext)
        {
            //ensure that we have 2 (or more) stores
            if (storeService.GetAllStores().Count < 2)
                return 0;


            var storeId = _genericAttributeService.GetAttribute<int>(workContext.CurrentCustomer, NopCustomerDefaults.AdminAreaStoreScopeConfigurationAttribute);
            var store = storeService.GetStoreById(storeId);
            return store != null ? store.Id : 0;
        }

    }
}
