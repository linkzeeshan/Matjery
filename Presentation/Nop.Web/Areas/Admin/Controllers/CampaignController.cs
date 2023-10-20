using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Messages;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using static Nop.Web.Areas.Admin.Models.Messages.CampaignModel;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class CampaignController : BaseAdminController
    {
        #region Fields
        private readonly ICustomerService _customerService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly ICampaignModelFactory _campaignModelFactory;
        private readonly ICampaignService _campaignService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IQueuedPushNotificationService _queuedPushNotificationService;
        private readonly IDiscountService _discountService;
        private readonly IProductService _productService;
        private readonly IPictureService _pictureService;
        private readonly IVendorService _vendorService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ICategoryService _categoryService;

        #endregion

        #region Ctor

        public CampaignController(EmailAccountSettings emailAccountSettings,
            ICampaignModelFactory campaignModelFactory,
            ICampaignService campaignService,
            ICustomerActivityService customerActivityService,
            IDateTimeHelper dateTimeHelper,
            IEmailAccountService emailAccountService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IPermissionService permissionService,
            IStoreContext storeContext,IStoreService storeService,
            IWorkContext workContext,
            ILanguageService languageService,ILocalizedEntityService localizedEntityService,
            IQueuedPushNotificationService queuedPushNotificationService,
            IDiscountService discountService,IProductService productService,
            IPictureService pictureService,IVendorService vendorService,
             ICustomerService customerService,
            IStaticCacheManager cacheManager,ICategoryService categoryService)
        {
            _emailAccountSettings = emailAccountSettings;
            _campaignModelFactory = campaignModelFactory;
            _campaignService = campaignService;
            _customerActivityService = customerActivityService;
            _dateTimeHelper = dateTimeHelper;
            _emailAccountService = emailAccountService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _permissionService = permissionService;
            _storeContext = storeContext;
            _storeService = storeService;
            _workContext = workContext;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _queuedPushNotificationService = queuedPushNotificationService;
            _discountService = discountService;
            _productService = productService;
            _pictureService = pictureService;
            _vendorService = vendorService;
            _cacheManager = cacheManager;
            _categoryService = categoryService;
            _customerService = customerService;
        }

        #endregion

        #region Utilities

        protected virtual EmailAccount GetEmailAccount(int emailAccountId)
        {
            return _emailAccountService.GetEmailAccountById(emailAccountId)
                ?? _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId)
                ?? throw new NopException("Email account could not be loaded");
        }
        [NonAction]
        protected virtual void UpdateLocales(Campaign campaign, CampaignModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(campaign,
                                                               x => x.Name,
                                                               localized.Name,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(campaign,
                                                               x => x.Subject,
                                                               localized.Subject,
                                                               localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(campaign,
                                                           x => x.Body,
                                                           localized.Body,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(campaign,
                                                           x => x.PictureId,
                                                           localized.PictureId,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(campaign,
                                                           x => x.PictureIdMobile,
                                                           localized.PictureId,
                                                           localized.LanguageId);
            }
        }
        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            //prepare model
            var model = _campaignModelFactory.PrepareCampaignSearchModel(new CampaignSearchModel());
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult List(CampaignSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _campaignModelFactory.PrepareCampaignListModel(searchModel);

            return Json(model);
        }

        public virtual IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            //prepare model
            var model = _campaignModelFactory.PrepareCampaignModel(new CampaignModel(), null);
            model.Active = true;
            AddLocales(_languageService, model.Locales);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual IActionResult Create(CampaignModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var campaign = model.ToEntity<Campaign>();

                campaign.CreatedOnUtc = DateTime.UtcNow;
                campaign.DontSendBeforeDateUtc = model.DontSendBeforeDate.HasValue ?
                    (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DontSendBeforeDate.Value) : null;

                _campaignService.InsertCampaign(campaign);

                UpdateLocales(campaign, model);

                //push notification
                //if (campaign.Active)
                //    _queuedPushNotificationService.QueueNewCampaignCustomerNotification(campaign);


                //activity log
                _customerActivityService.InsertActivity("AddNewCampaign",
                    string.Format(_localizationService.GetResource("ActivityLog.AddNewCampaign"), campaign.Id), campaign);

                var notificationmessag = $"{_localizationService.GetLocalized(campaign, x => x.Name, _workContext.WorkingLanguage.Id)} {_localizationService.GetResource("Admin.Promotions.Campaigns.Added")}";
                _notificationService.SuccessNotification(notificationmessag);

                return continueEditing ? RedirectToAction("Edit", new { id = campaign.Id }) : RedirectToAction("List");
            }

            //prepare model
            model = _campaignModelFactory.PrepareCampaignModel(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            //try to get a campaign with the specified id
            var campaign = _campaignService.GetCampaignById(id);
            if (campaign == null)
                return RedirectToAction("List");

            //prepare model
            var model = _campaignModelFactory.PrepareCampaignModel(null, campaign);

            //locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Name = _localizationService.GetLocalized(campaign,x => x.Name, languageId, false, false);
                locale.Subject = _localizationService.GetLocalized(campaign,x => x.Subject, languageId, false, false);
                locale.PictureId = _localizationService.GetLocalized(campaign, x => x.PictureId, languageId, false, false);
                locale.PictureIdMobile = _localizationService.GetLocalized(campaign, x => x.PictureIdMobile, languageId, false, false);
                locale.Body = _localizationService.GetLocalized(campaign, x => x.Body, languageId, false, false);
            });

            //discount
            model.DiscountPercentage = campaign.MinDiscountPercentage;
            if (_customerService.IsVendor(_workContext.CurrentCustomer))
            {
                Discount vendorDiscount = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus,
                    campaignId: model.Id, vendorId: _workContext.CurrentVendor.Id)
                    .FirstOrDefault();
                if (vendorDiscount != null)
                    model.DiscountPercentage = vendorDiscount.DiscountPercentage;
            }
            return View(model);
        }

        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual IActionResult Edit(CampaignModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            //try to get a campaign with the specified id
            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var prvActiveState = campaign.Active;
                campaign = model.ToEntity(campaign);

                campaign.DontSendBeforeDateUtc = model.DontSendBeforeDate.HasValue ?
                    (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.DontSendBeforeDate.Value) : null;

                _campaignService.UpdateCampaign(campaign);

                //locales
                UpdateLocales(campaign, model);

                //push notification
                if (prvActiveState != campaign.Active && campaign.Active)
                    _queuedPushNotificationService.QueueNewCampaignCustomerNotification(campaign);

                //activity log
                _customerActivityService.InsertActivity("EditCampaign",
                    string.Format(_localizationService.GetResource("ActivityLog.EditCampaign"), campaign.Id), campaign);
                //discount
                var campaignDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, campaignId: model.Id, showHidden: true);
                if (campaign.Active)
                {
                    //update all discount start date and remove end date
                    foreach (Discount discount in campaignDiscounts)
                    {
                        if (!discount.StartDateUtc.HasValue)
                            discount.StartDateUtc = DateTime.UtcNow;

                        discount.EndDateUtc = null;
                        _discountService.UpdateDiscount(discount);
                    }
                }
                else
                {
                    //update all discounts end date to end discount. Otherwise discount will still show up
                    foreach (Discount discount in campaignDiscounts)
                    {
                        discount.EndDateUtc = DateTime.UtcNow;
                        _discountService.UpdateDiscount(discount);
                    }
                }
                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Promotions.Campaigns.Updated"));

                return continueEditing ? RedirectToAction("Edit", new { id = campaign.Id }) : RedirectToAction("List");
            }

            //prepare model
            model = _campaignModelFactory.PrepareCampaignModel(model, campaign, true);

            //discount
            if (_customerService.IsVendor(_workContext.CurrentCustomer))
            {
                Discount vendorDiscount = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus,
                    campaignId: model.Id, vendorId: _workContext.CurrentVendor.Id)
                    .FirstOrDefault();
                if (vendorDiscount != null)
                    model.DiscountPercentage = vendorDiscount.DiscountPercentage;
            }

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("send-test-email")]
        public virtual IActionResult SendTestEmail(CampaignModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            //try to get a campaign with the specified id
            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
                return RedirectToAction("List");

            //prepare model
            model = _campaignModelFactory.PrepareCampaignModel(model, campaign);

            //ensure that the entered email is valid
            if (!CommonHelper.IsValidEmail(model.TestEmail))
            {
                _notificationService.ErrorNotification(_localizationService.GetResource("Admin.Common.WrongEmail"));
                return View(model);
            }

            try
            {
                var emailAccount = GetEmailAccount(model.EmailAccountId);
                var subscription = _newsLetterSubscriptionService
                    .GetNewsLetterSubscriptionByEmailAndStoreId(model.TestEmail, _storeContext.CurrentStore.Id);
                if (subscription != null)
                {
                    //there's a subscription. let's use it
                    _campaignService.SendCampaign(campaign, emailAccount, new List<NewsLetterSubscription> { subscription });
                }
                else
                {
                    //no subscription found
                    _campaignService.SendCampaign(campaign, emailAccount, model.TestEmail);
                }

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Promotions.Campaigns.TestEmailSentToCustomers"));

                return View(model);
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
            }

            //prepare model
            model = _campaignModelFactory.PrepareCampaignModel(model, campaign, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("send-mass-email")]
        public virtual IActionResult SendMassEmail(CampaignModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            //try to get a campaign with the specified id
            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
                return RedirectToAction("List");

            //prepare model
            model = _campaignModelFactory.PrepareCampaignModel(model, campaign);

            try
            {
                var emailAccount = GetEmailAccount(model.EmailAccountId);

                //subscribers of certain store?
                var storeId = _storeService.GetStoreById(campaign.StoreId)?.Id ?? 0;
                var subscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(storeId: storeId,
                    customerRoleId: model.CustomerRoleId,
                    isActive: true);
                var totalEmailsSent = _campaignService.SendCampaign(campaign, emailAccount, subscriptions);

                _notificationService.SuccessNotification(string.Format(_localizationService.GetResource("Admin.Promotions.Campaigns.MassEmailSentToCustomers"), totalEmailsSent));

                return View(model);
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
            }

            //prepare model
            model = _campaignModelFactory.PrepareCampaignModel(model, campaign, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            //try to get a campaign with the specified id
            var campaign = _campaignService.GetCampaignById(id);
            if (campaign == null)
                return RedirectToAction("List");

            var discounts = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus, campaignId: campaign.Id);
            foreach (var discount in discounts)
            {
                //applied to products
                var products = discount.AppliedToProducts.ToList();

                _discountService.DeleteDiscount(discount);

                //update "HasDiscountsApplied" properties
                foreach (var p in products)
                    _productService.UpdateHasDiscountsApplied(p);
            }

            var productCampaigns = _campaignService.GetProductCampaignsByCampaignId(campaign.Id);
            foreach (var cp in productCampaigns)
                _campaignService.DeleteProductCampaign(cp);

            _campaignService.DeleteCampaign(campaign);

            //activity log
            _customerActivityService.InsertActivity("DeleteCampaign",
                string.Format(_localizationService.GetResource("ActivityLog.DeleteCampaign"), campaign.Id), campaign);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Promotions.Campaigns.Deleted"));

            return RedirectToAction("List");
        }

        #endregion

        #region Products

        [HttpPost]
        public IActionResult ProductList(CampaignProductSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            IPagedList<ProductCampaign> productCampaigns = null;
            if (_customerService.IsVendor(_workContext.CurrentCustomer))
            {
                productCampaigns = _campaignService.GetProductCampaignsByCampaignId(searchModel.campaignId, _workContext.CurrentCustomer.VendorId,
                    searchModel.Page - 1, searchModel.PageSize, false);
            }
            else
            {
                productCampaigns = _campaignService.GetProductCampaignsByCampaignId(searchModel.campaignId, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, showHidden: true);
            }
            var model = new CampaignProductListModel().PrepareToGrid(searchModel, productCampaigns, () =>
            {
                return productCampaigns.Select(x =>
                {
 
                    var ProductModel = x.ToModel<CampaignProductModel>();

                    ProductModel.Id = x.Id;
                    ProductModel.ProductName = _productService.GetProductById(x.ProductId)?.Name;
                    ProductModel.CampaignId = x.CampaignId;
                    ProductModel.ProductId = x.ProductId;
                    ProductModel.DisplayOrder = x.DisplayOrder;
                    //ProductModel.PictureThumbnailUrl = _pictureService.GetPictureUrl(_pictureService.GetPicturesByProductId(x.ProductId, 1).FirstOrDefault().Id, 75, true);
                    var defaultProductPicture = _pictureService.GetPicturesByProductId(x.ProductId, 1).FirstOrDefault();
                    ProductModel.PictureThumbnailUrl = _pictureService.GetPictureUrl(ref defaultProductPicture);

                    if (defaultProductPicture != null)
                        ProductModel.PictureThumbnailUrl = defaultProductPicture.Id > 0 ? _pictureService.GetPictureUrl(defaultProductPicture.Id, 75, true) : "";

                    return ProductModel;
                });
            });
            return Json(model);
        }

        public IActionResult ProductUpdate(CampaignModel.CampaignProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var productCampaign = _campaignService.GetProductCampaignById(model.Id);
            if (productCampaign == null)
                throw new ArgumentException("No product campaign mapping found with the specified id");

            productCampaign.DisplayOrder = model.DisplayOrder;
            _campaignService.UpdateProductCampaign(productCampaign);

            return new NullJsonResult();
        }

        public IActionResult ProductDelete(int id, int campaignId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var productCampaign = _campaignService.GetProductCampaignById(id);
            if (productCampaign == null)
                throw new ArgumentException("No product category mapping found with the specified id");

            //var categoryId = productCategory.CategoryId;
            _campaignService.DeleteProductCampaign(productCampaign);

            Discount vendorDiscount = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus,
                campaignId: campaignId, vendorId: _workContext.CurrentCustomer.VendorId)
                .FirstOrDefault();
            if (vendorDiscount != null)
            {
                var product = _productService.GetProductById(productCampaign.ProductId);
                if (product == null)
                    throw new Exception("No product found with the specified id");

                //remove discount
                if (product.AppliedDiscounts.Count(d => d.Id == vendorDiscount.Id) > 0)
                    product.AppliedDiscounts.Remove(vendorDiscount);

                _productService.UpdateProduct(product);
                _productService.UpdateHasDiscountsApplied(product);
            }
            return new NullJsonResult();
        }

        public IActionResult ProductAddPopup(int campaignId,decimal discountPercentage)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var model = new AddProductToCampaignSearchModel();
            if (!_customerService.IsVendor(_workContext.CurrentCustomer))
            {
                //categories
                model.AvailableCategories.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
                _campaignModelFactory.PrepareCategories(model.AvailableCategories);
                //foreach (var c in categories)
                //    model.AvailableCategories.Add(c);
            }

            if (!_customerService.IsVendor(_workContext.CurrentCustomer))
            {
                //vendors
                model.AvailableVendors.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
                foreach (var v in _vendorService.GetAllVendors(showHidden: true))
                    model.AvailableVendors.Add(new SelectListItem { Text = v.Name, Value = v.Id.ToString() });
            }
            model.CampaignId = campaignId;
            model.DiscountPercentage = discountPercentage;
            return View(model);
        }

        [HttpPost]
        public IActionResult ProductAddPopupList(AddProductToCampaignSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();
            if (_customerService.IsVendor(_workContext.CurrentCustomer))
            {
                searchModel.SearchVendorId = _workContext.CurrentVendor.Id;
            }
            var products = _productService.SearchProducts(
             categoryIds: new List<int> { searchModel.SearchCategoryId },
             vendorId: searchModel.SearchVendorId,
             keywords: searchModel.SearchProductName,
             pageIndex: searchModel.Page - 1,
             pageSize: searchModel.PageSize,
             showHidden: _customerService.IsAdmin(_workContext.CurrentCustomer)
             );

            //prepare grid model
            var model = new AddProductToCampaignListModel().PrepareToGrid(searchModel, products, () =>
            {
                return products.Select(x =>
                {
                    var productModel = x.ToModel<ProductModel>();
                    var defaultProductPicture = _pictureService.GetPicturesByProductId(x.Id, 1).FirstOrDefault();
                    productModel.PictureThumbnailUrl = _pictureService.GetPictureUrl(ref defaultProductPicture);
                    if (defaultProductPicture != null)
                    {
                        productModel.PictureThumbnailUrl = defaultProductPicture.Id > 0 ? _pictureService.GetPictureUrl(defaultProductPicture.Id, 75, true) : "";

                    }
                    return productModel;
                });
            });

            //gridModel.Total = products.TotalCount;
            return Json(model);
            //return Json(new { Data = result,Total= products.TotalCount });
        }

        [HttpPost]
        [FormValueRequired("save")]
        public IActionResult ProductAddPopup(string btnId, string formId,AddCampaignProductModel model) //, string currentPageUrl,
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(model.CampaignId);
            if (campaign == null)
                //No campaign found with the specified id
                return View(model);

            if (model.DiscountPercentage < campaign.MinDiscountPercentage)
                return View(model);

            if (model.SelectedProductIds != null)
            {
                //check if any discount is added for this vendor
                Discount vendorDiscount = _discountService.GetAllDiscounts(DiscountType.AssignedToSkus,
                    campaignId: model.CampaignId, vendorId: _workContext.CurrentCustomer.VendorId)
                    .FirstOrDefault();
                if (vendorDiscount == null)
                {
                    //add vendor discount
                    var vendorNewDiscount = new Discount();
                    if (_customerService.IsVendor(_workContext.CurrentCustomer))
                        vendorNewDiscount.Name = _localizationService.GetLocalized(_workContext.CurrentVendor,x => x.Name) + " - " + _localizationService.GetLocalized(campaign,x => x.Name);
                    else
                        vendorNewDiscount.Name = _localizationService.GetLocalized(campaign,x => x.Name);

                    vendorNewDiscount.DiscountTypeId = (int)DiscountType.AssignedToSkus;
                    vendorNewDiscount.UsePercentage = true;
                    vendorNewDiscount.DiscountPercentage = model.DiscountPercentage;
                    vendorNewDiscount.StartDateUtc = campaign.CreatedOnUtc;
                    vendorNewDiscount.VendorId = _workContext.CurrentCustomer.VendorId;
                    vendorNewDiscount.CampaignId = campaign.Id;
                    _discountService.InsertDiscount(vendorNewDiscount);

                    //assign it to current
                    vendorDiscount = vendorNewDiscount;
                }
                else
                {
                    //update discount value if was changed
                    if (vendorDiscount.DiscountPercentage != model.DiscountPercentage)
                    {
                        vendorDiscount.DiscountPercentage = model.DiscountPercentage;
                        _discountService.UpdateDiscount(vendorDiscount);
                    }
                }
                foreach (int id in model.SelectedProductIds)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        var existingProductCampaigns = _campaignService.GetProductCampaignsByCampaignId(model.CampaignId, showHidden: true);
                        if (FindProductCampaign(existingProductCampaigns, id, model.CampaignId) == null)
                        {
                            _campaignService.InsertProductCampaign(new ProductCampaign
                            {
                                CampaignId = model.CampaignId,
                                ProductId = id,
                                DisplayOrder = 1,
                                VendorId = _workContext.CurrentCustomer.VendorId
                            });
                        }

                        //insert it in discount too
                        if (product.AppliedDiscounts.Count(d => d.Id == vendorDiscount.Id) == 0)
                        {
                            product.AppliedDiscounts.Add(vendorDiscount);

                            _productService.UpdateProduct(product);
                            _productService.UpdateHasDiscountsApplied(product);
                        }
                    }
                }

                //}
            }

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
           // ViewBag.currentPageUrl = currentPageUrl;
            return View(new AddProductToCampaignSearchModel());
        
            //return View(model);
        }

        #endregion


        public ProductCampaign FindProductCampaign(IList<ProductCampaign> source, int productId, int campaignId)
        {
            foreach (var productCampaign in source)
                if (productCampaign.ProductId == productId && productCampaign.CampaignId == campaignId)
                    return productCampaign;

            return null;
        }
    }
}