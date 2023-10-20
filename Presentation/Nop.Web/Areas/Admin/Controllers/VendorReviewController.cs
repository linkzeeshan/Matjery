using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Stores;
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
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class VendorReviewController : BaseAdminController
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
        private readonly ForumSettings _forumSettings;
        private readonly OrderSettings _orderSettings;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerService _customerService;
        private readonly IVendorRatingService _vendorRatingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly INotificationService _notificationService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;


        #endregion

        #region Constructor 

        public VendorReviewController(IPermissionService permissionService, IPictureService pictureService, IVendorService vendorService,
            ICountryService countryService, IStateProvinceService stateProvinceService, IStaticCacheManager cacheManager,
            ILocalizationService localizationService, IWorkContext workContext, IOrderService orderService, IProductService productService,
            CatalogSettings catalogSettings, IWorkflowMessageService workflowMessageService, ICustomerActivityService customerActivityService,
            LocalizationSettings localizationSettings, ISettingService settingService, IStoreService storeService, IStoreContext storeContext,
            IDateTimeHelper dateTimeHelper, CustomerSettings customerSettings, RewardPointsSettings rewardPointsSettings, ForumSettings forumSettings,
            OrderSettings orderSettings, IWebHelper webHelper, ICustomerService customerService,
            INotificationService notificationService,
            IBaseAdminModelFactory baseAdminModelFactory,
        IVendorRatingService vendorRatingService, IGenericAttributeService genericAttributeService)
        {
            _baseAdminModelFactory = baseAdminModelFactory;
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
            _forumSettings = forumSettings;
            _orderSettings = orderSettings;
            _webHelper = webHelper;
            _customerService = customerService;
            _vendorRatingService = vendorRatingService;
            _genericAttributeService = genericAttributeService;
        }

        #endregion

        #region Configure 
   
        public ActionResult Configure()
        {
            int storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            VendorReviewSettings vendorReviewSettings = _settingService.LoadSetting<VendorReviewSettings>(storeScope);

            var model = new ConfigurationModel();
            this.PrepareConfigureModel(model, vendorReviewSettings, storeScope);

            return View(model);
        }
        [HttpPost]
        public ActionResult Configure(ConfigurationModel model)
        {
            int storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext);
            VendorReviewSettings vendorRatingSettings = _settingService.LoadSetting<VendorReviewSettings>(storeScope);
            if (!ModelState.IsValid)
            {
                //foreach (ModelState modelState in ModelState.Values)
                //{
                //    foreach (ModelError error in modelState.Errors)
                //    {
                //        ErrorNotification(error.ErrorMessage);
                //    }
                //}
            }
            else
            {
                vendorRatingSettings.VendorReviewsWidgetZone = model.VendorReviewsWidgetZone;
                vendorRatingSettings.DefaultVendorRatingValue = model.DefaultVendorRatingValue;
                vendorRatingSettings.AllowAnonymousUsersToReviewVendor = model.AllowAnonymousUsersToReviewVendor;
                vendorRatingSettings.VendorReviewsMustBeApproved = model.VendorReviewsMustBeApproved;
                if (model.WidgetZone_VendorReviews_OverrideForStore || storeScope == 0)
                    _settingService.SaveSetting(vendorRatingSettings, x => x.VendorReviewsWidgetZone, storeScope, false);
                else if (storeScope > 0)
                    _settingService.DeleteSetting(vendorRatingSettings, x => x.VendorReviewsWidgetZone, storeScope);
                if (model.DefaultVendorRatingValue_OverrideForStore || storeScope == 0)
                    _settingService.SaveSetting(vendorRatingSettings, x => x.DefaultVendorRatingValue, storeScope, false);
                else if (storeScope > 0)
                    _settingService.DeleteSetting(vendorRatingSettings, x => x.DefaultVendorRatingValue, storeScope);

                if (model.AllowAnonymousUsersToReviewVendor_OverrideForStore || storeScope == 0)
                    _settingService.SaveSetting(vendorRatingSettings, x => x.AllowAnonymousUsersToReviewVendor, storeScope, false);
                else if (storeScope > 0)
                    _settingService.DeleteSetting(vendorRatingSettings, x => x.AllowAnonymousUsersToReviewVendor, storeScope);

                if (model.VendorReviewsMustBeApproved_OverrideForStore || storeScope == 0)
                    _settingService.SaveSetting(vendorRatingSettings, x => x.VendorReviewsMustBeApproved, storeScope, false);

                else if (storeScope > 0)
                    _settingService.DeleteSetting(vendorRatingSettings, x => x.VendorReviewsMustBeApproved, storeScope);

                //now clear cache
                _settingService.ClearCache();
                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            }

            PrepareConfigureModel(model, vendorRatingSettings, storeScope);
            return View("Configure", model);
        }


        #endregion

        [HttpPost]
        public ActionResult ApproveSelected(ICollection<int> selectedIds)
        {
            if (selectedIds != null)
            {
                foreach (Nop.Core.Domain.Vendors.VendorReview vendorReview in _vendorRatingService.GetVendorReviewsByIds(selectedIds.ToArray()))
                {
                    vendorReview.IsApproved = true;
                    _vendorRatingService.UpdateVendorReview(vendorReview);
                }
            }
            return Json(new { Result = true });
        }

        //[ActionName("Edit")]
        //[FormValueRequired("btnDeleteReview")]
        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            ActionResult actionResult;
            VendorReview vendorReview = _vendorRatingService.GetVendorReviewById(id);
            if (vendorReview != null)
            {
                _vendorRatingService.DeleteVendorReview(vendorReview);
                _notificationService.SuccessNotification(_localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Deleted"), true);
                actionResult = RedirectToAction("List");
            }
            else
            {
                actionResult = RedirectToAction("List");
            }
            return actionResult;
        }

        [HttpPost]
        public ActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (selectedIds != null)
            {
                foreach (Nop.Core.Domain.Vendors.VendorReview vendorReview in _vendorRatingService.GetVendorReviewsByIds(selectedIds.ToArray()))
                {
                    _vendorRatingService.DeleteVendorReview(vendorReview);
                }
            }
            return Json(new { Result = true });
        }

        [HttpPost]
        public ActionResult DisapproveSelected(ICollection<int> selectedIds)
        {
            if (selectedIds != null)
            {
                foreach (Nop.Core.Domain.Vendors.VendorReview vendorReview in _vendorRatingService.GetVendorReviewsByIds(selectedIds.ToArray()))
                {
                    vendorReview.IsApproved = false;
                    _vendorRatingService.UpdateVendorReview(vendorReview);
                }
            }
            return Json(new { Result = true });
        }

        public ActionResult Edit(int id, string returnUrl)
        {
           VendorReview vendorReview = _vendorRatingService.GetVendorReviewById(id);
            if (vendorReview == null)
                return RedirectToAction("List");

            AdminVendorReviewModel model = new AdminVendorReviewModel();

            PrepareVendorReviewModel(model, vendorReview, false, false);

            model.ReturnUrl = returnUrl;

            return View("Edit",model);
        }

     
        //[FormValueRequired("save")]
        [HttpPost]
        public IActionResult Edit(AdminVendorReviewModel model)
        {
            Nop.Core.Domain.Vendors.VendorReview vendorReview = _vendorRatingService.GetVendorReviewById(model.Id);
            if (vendorReview == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                vendorReview.Title = model.Title;
                vendorReview.ReviewText = model.ReviewText;
                vendorReview.IsApproved = model.IsApproved;
                _vendorRatingService.UpdateVendorReview(vendorReview);
                _notificationService.SuccessNotification(_localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Admin.VendorReviews.Updated"));

                if (!string.IsNullOrEmpty(model.ReturnUrl))
                    return RedirectToAction(model.ReturnUrl);

                return RedirectToAction("List");
            }

            PrepareVendorReviewModel(model, vendorReview, true, false);
            return View("Edit",model);
        }

        public IActionResult List()
        {
            var model = new VendorReviewListModel();
            model.AvailableStores.Add(new SelectListItem
            {
                Text = _localizationService.GetResource("Admin.Common.All"),
                Value = "0"
            });
            foreach (Store store in _storeService.GetAllStores())
            {
                model.AvailableStores.Add(new SelectListItem
                {
                    Text = store.Name,
                    Value = store.Id.ToString()
                });
            }
            //prepare available vendors
            _baseAdminModelFactory.PrepareVendors(model.AvailableVendors,
                defaultItemText: _localizationService.GetResource("Admin.Customers.Customers.Fields.Vendor.None"));
            return View(model);
        }

        public IActionResult ListPartial(int id)
        {
            var model = new VendorReviewListModel();
            model.SearchVendorId = id;

            return PartialView("_ListPartial.cshtml", model);
        }

        [HttpPost]
        public IActionResult List(VendorReviewListModel serahcmodel)
        {
            //DateTime? nullable;
            //DateTime? nullable1;
            //DateTime? createdOnFrom = serahcmodel.CreatedOnFrom;
            //if (!createdOnFrom.HasValue)
            //{
            //    createdOnFrom = null;
            //    nullable = createdOnFrom;
            //}
            //else
            //{
            //    IDateTimeHelper dateTimeHelper = _dateTimeHelper;
            //    createdOnFrom = serahcmodel.CreatedOnFrom;
            //    nullable = dateTimeHelper.ConvertToUtcTime(createdOnFrom.Value, _dateTimeHelper.CurrentTimeZone);
            //}
            //DateTime? createdOnFromValue = nullable;
            //createdOnFrom = serahcmodel.CreatedOnTo;
            //if (!createdOnFrom.HasValue)
            //{
            //    createdOnFrom = null;
            //    nullable1 = null;
            //}
            //else
            //{
            //    IDateTimeHelper dateTimeHelper1 = _dateTimeHelper;
            //    createdOnFrom = serahcmodel.CreatedOnTo;
            //    DateTime utcTime = dateTimeHelper1.ConvertToUtcTime(createdOnFrom.Value, _dateTimeHelper.CurrentTimeZone);
            //    nullable1 = utcTime.AddDays(1);
            //}
            //DateTime? createdToFromValue = nullable1;
            var startDateValue = !serahcmodel.CreatedOnFrom.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(serahcmodel.CreatedOnFrom.Value, _dateTimeHelper.CurrentTimeZone);
            var endDateValue = !serahcmodel.CreatedOnTo.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(serahcmodel.CreatedOnTo.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var vendorReviews = _vendorRatingService.GetAllVendorReviews(0, serahcmodel.SearchVendorId,
                serahcmodel.SearchStoreId, null, startDateValue, endDateValue, serahcmodel.SearchText, serahcmodel.Page, serahcmodel.PageSize);


            var model = new AddReviewToVendorReviewListwmodel().PrepareToGrid(serahcmodel, vendorReviews, () =>
            {
                return vendorReviews.Select(x =>
                {
                    var m = x.ToModel<AdminVendorReviewModel>();
                    PrepareVendorReviewModel(m, x, false, true);
                    return m;
                });
            });

            return Json(model);
        }

        [HttpPost]
        public ActionResult SetVendorReviewHelpfulness(int vendorReviewId, bool washelpful)
        {
            ActionResult actionResult;
            Nop.Core.Domain.Vendors.VendorReview vendorReview = _vendorRatingService.GetVendorReviewById(vendorReviewId);
            if (vendorReview == null)
            {
                throw new ArgumentException("No vendor review found with the specified id");
            }
            int storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext, _genericAttributeService);
            VendorReviewSettings vendorReviewSettings = _settingService.LoadSetting<VendorReviewSettings>(storeScope);
            if (_customerService.IsGuest(_workContext.CurrentCustomer) && !vendorReviewSettings.AllowAnonymousUsersToReviewVendor)
            {
                actionResult = Json(new
                {
                    Result = _localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Reviews.Helpfulness.OnlyRegistered"),
                    TotalYes = vendorReview.HelpfulYesTotal,
                    TotalNo = vendorReview.HelpfulNoTotal
                });
            }
            else if (vendorReview.CustomerId != _workContext.CurrentCustomer.Id)
            {
                VendorReviewHelpfulness vrh = vendorReview.VendorReviewHelpfulness.FirstOrDefault(x => x.CustomerId == _workContext.CurrentCustomer.Id);
                if (vrh == null)
                {
                    vrh = new VendorReviewHelpfulness
                    {
                        VendorReviewId = vendorReview.Id,
                        CustomerId = _workContext.CurrentCustomer.Id,
                        WasHelpful = washelpful
                    };
                    vendorReview.VendorReviewHelpfulness.Add(vrh);
                }
                else
                {
                    vrh.WasHelpful = washelpful;
                }
                _vendorRatingService.UpdateVendorReview(vendorReview);
                vendorReview.HelpfulYesTotal = vendorReview.VendorReviewHelpfulness.Count(x => x.WasHelpful);
                vendorReview.HelpfulNoTotal = vendorReview.VendorReviewHelpfulness.Count(x => !x.WasHelpful);
                _vendorRatingService.UpdateVendorReview(vendorReview);
                actionResult = Json(new
                {
                    Result = _localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Reviews.Helpfulness.SuccessfullyVoted"),
                    TotalYes = vendorReview.HelpfulYesTotal,
                    TotalNo = vendorReview.HelpfulNoTotal
                });
            }
            else
            {
                actionResult = Json(new
                {
                    Result = _localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Reviews.Helpfulness.YourOwnReview"),
                    TotalYes = vendorReview.HelpfulYesTotal,
                    TotalNo = vendorReview.HelpfulNoTotal
                });
            }
            return actionResult;
        }

        public ActionResult VendorSearchAutoComplete(string term)
        {
            ActionResult actionResult;
            if (!string.IsNullOrWhiteSpace(term) && term.Length >= 3)
            {
                var result = (
                    from v in _vendorService.GetAllVendors(term, "", 0, 15, true,_workContext.CurrentCustomer)
                    select new { label = v.Name, vendorid = v.Id }).ToList();
                actionResult = Json(result, 0);
            }
            else
            {
                actionResult = Content("");
            }
            return actionResult;
        }

        #region Public 

        // [ChildActionOnly]
        public ActionResult VendorReviews(int vendorId, VendorReviewsModel model, bool isEmptyModel = true)
        {
            Vendor vendor = _vendorService.GetVendorById(vendorId);
            if (vendor != null && !vendor.Deleted)
            {
                if (isEmptyModel)
                {
                    model.AddVendorReview.Title = null;
                    model.AddVendorReview.ReviewText = null;
                    model.Items.Clear();
                    ModelState.Clear();
                }

                int storeScope = GetActiveStoreScopeConfiguration(_storeService, _workContext, _genericAttributeService);
                VendorReviewSettings vendorReviewSettings = _settingService.LoadSetting<VendorReviewSettings>(storeScope);
                PrepareVendorReviewsModel(model, vendor, vendorReviewSettings);
                if (_customerService.IsGuest(_workContext.CurrentCustomer) && !vendorReviewSettings.AllowAnonymousUsersToReviewVendor)
                {
                    ModelState.AddModelError("", _localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Reviews.OnlyRegisteredUsersCanWriteReviews"));
                }
                model.AddVendorReview.Rating = vendorReviewSettings.DefaultVendorRatingValue;
                return View(model);
            }
            else
            {
                return Content("");
            }
        }

        [ActionName("VendorReviews")]
        //[FormValueRequired("add-vendor-review")]
        [HttpPost]
        public ActionResult VendorReviewsAdd(int vendorId, VendorReviewsModel model)
        {
            ActionResult route;
            Vendor vendor = _vendorService.GetVendorById(vendorId);
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
                if (!ModelState.IsValid || vendorCode != validationCode)
                {
                    PrepareVendorReviewsModel(model, vendor, vendorReviewSettings);
                    route = VendorReviews(vendor.Id, model, false);
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
                    route = VendorReviews(vendor.Id, model, true);
                }
            }
            else
            {
                route = RedirectToRoute("HomePage");
            }
            return route;
        }


        #endregion


        #region Helpers 

        
        [NonAction]
        protected virtual void PrepareVendorReviewModel(AdminVendorReviewModel model, Nop.Core.Domain.Vendors.VendorReview vendorReview,
            bool excludeProperties, bool formatReviewText)
        {
            vendorReview.Vendor = _vendorService.GetVendorById(vendorReview.VendorId);
            vendorReview.Store = _storeService.GetStoreById(vendorReview.StoreId);
            vendorReview.Customer = _customerService.GetCustomerById(vendorReview.CustomerId);

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (vendorReview == null)
                throw new ArgumentNullException(nameof(vendorReview));

            model.Id = vendorReview.Id;
            model.StoreName = vendorReview.Store.Name;
            model.VendorId = vendorReview.VendorId;
            model.VendorName =vendorReview.Vendor != null ? _localizationService.GetLocalized(_vendorService.GetVendorById(vendorReview.VendorId), x => x.Name) : string.Empty;
            model.CustomerId = vendorReview.CustomerId;
            if (vendorReview.Customer != null)
            {
                Customer customer = vendorReview.Customer;
                model.CustomerInfo = _customerService.IsRegistered(customer) ? customer.Email : _localizationService.GetResource("Admin.Customers.Guest");
            }
            model.Rating = vendorReview.Rating;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(vendorReview.CreatedOnUtc, DateTimeKind.Utc);
            if (!excludeProperties)
            {
                model.Title = vendorReview.Title;
                if (!formatReviewText)
                {
                    model.ReviewText = vendorReview.ReviewText;
                }
                else
                {
                    model.ReviewText = Nop.Core.Html.HtmlHelper.FormatText(vendorReview.ReviewText, false, true, false, false, false, false);
                }
                model.IsApproved = vendorReview.IsApproved;
            }
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

        #endregion
        public virtual int GetActiveStoreScopeConfiguration(IStoreService storeService, IWorkContext workContext)
        {
            //ensure that we have 2 (or more) stores
            if (storeService.GetAllStores().Count < 2)
                return 0;


            var storeId = _genericAttributeService.GetAttribute<int>(workContext.CurrentCustomer, NopCustomerDefaults.AdminAreaStoreScopeConfigurationAttribute);
            var store = storeService.GetStoreById(storeId);
            return store != null ? store.Id : 0;
        }

        [NonAction]
        protected virtual void PrepareConfigureModel(ConfigurationModel model, VendorReviewSettings vendorRatingSettings, int storeScope)
        {
            model.VendorReviewsWidgetZone = vendorRatingSettings.VendorReviewsWidgetZone;
            model.DefaultVendorRatingValue = vendorRatingSettings.DefaultVendorRatingValue;
            model.AllowAnonymousUsersToReviewVendor = vendorRatingSettings.AllowAnonymousUsersToReviewVendor;
            model.VendorReviewsMustBeApproved = vendorRatingSettings.VendorReviewsMustBeApproved;
            model.ActiveStoreScopeConfiguration = storeScope;
            if (storeScope > 0)
            {
                model.WidgetZone_VendorReviews_OverrideForStore = _settingService.SettingExists(vendorRatingSettings, x => x.VendorReviewsWidgetZone, storeScope);
                model.DefaultVendorRatingValue_OverrideForStore = _settingService.SettingExists(vendorRatingSettings, x => x.DefaultVendorRatingValue, storeScope);
                model.AllowAnonymousUsersToReviewVendor_OverrideForStore = _settingService.SettingExists(vendorRatingSettings, x => x.AllowAnonymousUsersToReviewVendor, storeScope);
                model.VendorReviewsMustBeApproved_OverrideForStore = _settingService.SettingExists(vendorRatingSettings, x => x.VendorReviewsMustBeApproved, storeScope);
            }
            IList<SelectListItem> availableWidgetZoneVendorReviews = model.AvailableWidgetZone_VendorReviews;
            SelectListItem selectListItem = new SelectListItem();
            selectListItem.Text = "vendordetails_top";
            selectListItem.Value = "vendordetails_top";
            availableWidgetZoneVendorReviews.Add(selectListItem);
            IList<SelectListItem> selectListItems = model.AvailableWidgetZone_VendorReviews;
            SelectListItem selectListItem1 = new SelectListItem();
            selectListItem1.Text = "vendordetails_bottom";
            selectListItem1.Value = "vendordetails_bottom";
            selectListItems.Add(selectListItem1);
        }

    }
}
