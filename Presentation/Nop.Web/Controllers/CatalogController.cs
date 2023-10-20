using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Extensions;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;
using Nop.Web.Models.Vendors;

namespace Nop.Web.Controllers
{
    public partial class CatalogController : BasePublicController
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IAclService _aclService;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly ICategoryService _categoryService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPermissionService _permissionService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IProductTagService _productTagService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IVendorService _vendorService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly IVendorFollowerService _vendorFollowerService;
        private readonly ICampaignService _campaignService;
        private readonly CommonSettings _commonSettings;
        private readonly IDownloadService _downloadService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ICacheKeyService _cacheKeyService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IPictureService _pictureService;
        private readonly ICustomerService _customerService;
        private readonly IVendorModelFactory _vendorModelFactory;
        #endregion

        #region Ctor

        public CatalogController(CatalogSettings catalogSettings,
            IAclService aclService,
            ICatalogModelFactory catalogModelFactory,
            ICategoryService categoryService, 
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IManufacturerService manufacturerService,
            IPermissionService permissionService, 
            IProductModelFactory productModelFactory,
            IProductService productService, 
            IProductTagService productTagService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext, 
            MediaSettings mediaSettings,
            IStaticCacheManager staticCacheManager,
            ICacheKeyService cacheKeyService,
            IUrlRecordService urlRecordService,
            IPictureService pictureService,
              ICustomerService customerService,
              IVendorModelFactory vendorModelFactory,
        VendorSettings vendorSettings)
        {
            _vendorModelFactory = vendorModelFactory;
            _catalogSettings = catalogSettings;
            _cacheKeyService = cacheKeyService;
            _aclService = aclService;
            _catalogModelFactory = catalogModelFactory;
            _categoryService = categoryService;
            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _manufacturerService = manufacturerService;
            _permissionService = permissionService;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _productTagService = productTagService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _vendorSettings = vendorSettings;
            _commonSettings = EngineContext.Current.Resolve<CommonSettings>();
            _vendorFollowerService = EngineContext.Current.Resolve<IVendorFollowerService>();
            _campaignService = EngineContext.Current.Resolve<ICampaignService>();
            _downloadService = EngineContext.Current.Resolve<IDownloadService>();
            _staticCacheManager = staticCacheManager;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _customerService = customerService;
        }

        #endregion
        
        #region Categories
        
        public virtual IActionResult Category(int categoryId, CatalogPagingFilteringModel command)
        {
            var category = _categoryService.GetCategoryById(categoryId);
            if (category == null || category.Deleted)
                return InvokeHttp404();

            var notAvailable =
                //published?
                !category.Published ||
                //ACL (access control list) 
                !_aclService.Authorize(category) ||
                //Store mapping
                !_storeMappingService.Authorize(category);
            //Check whether the current user has a "Manage categories" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageCategories);
            if (notAvailable && !hasAdminAccess)
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, 
                NopCustomerDefaults.LastContinueShoppingPageAttribute, 
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageCategories))
                DisplayEditLink(Url.Action("Edit", "Category", new { id = category.Id, area = AreaNames.Admin }));

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewCategory",
                string.Format(_localizationService.GetResource("ActivityLog.PublicStore.ViewCategory"), category.Name), category);

            //model
            var model = _catalogModelFactory.PrepareCategoryModel(category, command);

            //template
            var templateViewPath = _catalogModelFactory.PrepareCategoryTemplateViewPath(category.CategoryTemplateId);
            return View(templateViewPath, model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult GetCatalogRoot()
        {
            var model = _catalogModelFactory.PrepareRootCategories();

            return Json(model);
        }
        //[ActionName("VendorReviews")]
        //[FormValueRequired("add-vendor-review")]
        [HttpPost]
        public virtual IActionResult VendorReviewsAdd(VendorReviewsModel model)
        {
            return View();
        }
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual IActionResult GetCatalogSubCategories(int id)
        {
            var model = _catalogModelFactory.PrepareSubCategories(id);

            return Json(model);
        }

        #endregion

        #region Manufacturers

        public virtual IActionResult Manufacturer(int manufacturerId, CatalogPagingFilteringModel command)
        {
            var manufacturer = _manufacturerService.GetManufacturerById(manufacturerId);
            if (manufacturer == null || manufacturer.Deleted)
                return InvokeHttp404();

            var notAvailable =
                //published?
                !manufacturer.Published ||
                //ACL (access control list) 
                !_aclService.Authorize(manufacturer) ||
                //Store mapping
                !_storeMappingService.Authorize(manufacturer);
            //Check whether the current user has a "Manage categories" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = _permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageManufacturers);
            if (notAvailable && !hasAdminAccess)
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, 
                NopCustomerDefaults.LastContinueShoppingPageAttribute, 
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);
            
            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageManufacturers))
                DisplayEditLink(Url.Action("Edit", "Manufacturer", new { id = manufacturer.Id, area = AreaNames.Admin }));

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewManufacturer",
                string.Format(_localizationService.GetResource("ActivityLog.PublicStore.ViewManufacturer"), manufacturer.Name), manufacturer);

            //model
            var model = _catalogModelFactory.PrepareManufacturerModel(manufacturer, command);
            
            //template
            var templateViewPath = _catalogModelFactory.PrepareManufacturerTemplateViewPath(manufacturer.ManufacturerTemplateId);
            return View(templateViewPath, model);
        }

        public virtual IActionResult ManufacturerAll()
        {
            var model = _catalogModelFactory.PrepareManufacturerAllModels();
            return View(model);
        }
        
        #endregion

        #region Vendors

        public virtual IActionResult Vendor(int vendorId, CatalogPagingFilteringModel command)
        {
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null || vendor.Deleted || !vendor.Active)
                return InvokeHttp404();

            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            var costumerLogin = _workContext.CurrentCustomer.Username == null ? false : true;
            var imFollow = false;
            VendorFollower existingFollower = _vendorFollowerService.GetAllFollowers(_workContext.CurrentCustomer.Id,
                    vendor.Id, showUnFollowed: true).FirstOrDefault();
            if (existingFollower != null && !existingFollower.Unfollowed)
            {
                imFollow = true;
            }

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageVendors))
                DisplayEditLink(Url.Action("Edit", "Vendor", new { id = vendor.Id, area = AreaNames.Admin }));

            //model
            var model = _catalogModelFactory.PrepareVendorModel(vendor, command);
            model.ShowFollowbutton = costumerLogin;
            model.imFollowing = imFollow;
            model.SupportedByFoundationId = vendor.SupportedByFoundationId;
            
            return View(model);
        }

       // [NopHttpsRequirement( SslRequirement.No)]
        //public ActionResult HomePageVendors(bool? displayEntities)
        //{
        //    var model = _catalogModelFactory.PrepareVendor(displayEntities);
        //    return View(model);
        //}

        [HttpPost]
        public JsonResult FollowVendor(int vendorId)
        {

            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null || !vendor.Active || vendor.Deleted)
                return Json("");

            if (!ModelState.IsValid)
                return Json("");

            if (_customerService.IsGuest(_workContext.CurrentCustomer))
                return Json("");

            VendorFollower existingFollower = _vendorFollowerService.GetAllFollowers(_workContext.CurrentCustomer.Id,
                vendor.Id, showUnFollowed: true).FirstOrDefault();
            if (existingFollower != null && !existingFollower.Unfollowed)
            {
                existingFollower.UnFollowOnUtc = DateTime.UtcNow;
                existingFollower.Unfollowed = true;
                _vendorFollowerService.UpdateFollower(existingFollower);
                return Json(false);
            }
            else
            {
                if (existingFollower != null && existingFollower.Unfollowed)
                {
                    existingFollower.UnFollowOnUtc = null;
                    existingFollower.Unfollowed = false;
                    existingFollower.FollowOnUtc = DateTime.UtcNow;
                    _vendorFollowerService.UpdateFollower(existingFollower);
                }
                else
                {
                    var follower = new VendorFollower
                    {
                        CustomerId = _workContext.CurrentCustomer.Id,
                        VendorId = vendor.Id,
                        FollowOnUtc = DateTime.UtcNow
                    };
                    _vendorFollowerService.InsertFollower(follower);
                }
            }

            return Json(true);

        }
        public virtual IActionResult VendorAll(CatalogPagingFilteringModel command, bool displayEntities = false)
        {
            //we don't allow viewing of vendors if "vendors" block is hidden
            if (_vendorSettings.VendorsBlockItemsToDisplay == 0)
                return RedirectToRoute("Homepage");

            var model = _catalogModelFactory.PrepareVendorAllModels(command, displayEntities);
            return View(model);
        }

        #endregion

        #region Product tags
        
        public virtual IActionResult ProductsByTag(int productTagId, CatalogPagingFilteringModel command)
        {
            var productTag = _productTagService.GetProductTagById(productTagId);
            if (productTag == null)
                return InvokeHttp404();

            var model = _catalogModelFactory.PrepareProductsByTagModel(productTag, command);
            return View(model);
        }

        public virtual IActionResult ProductTagsAll()
        {
            var model = _catalogModelFactory.PrepareProductTagsAllModel();
            return View(model);
        }

        #endregion

        #region Searching

        public virtual IActionResult Search(SearchModel model, CatalogPagingFilteringModel command)
        {
            //'Continue shopping' URL
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(true),
                _storeContext.CurrentStore.Id);

            if (model == null)
                model = new SearchModel();

            model = _catalogModelFactory.PrepareSearchModel(model, command);
            return View(model);
        }


        public ActionResult SearchNew(SearchModel model, CatalogPagingFilteringModel command)
        {
            //'Continue shopping' URL
            model.adv = true;
            _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
                NopCustomerDefaults.LastContinueShoppingPageAttribute,
                _webHelper.GetThisPageUrl(false),
                _storeContext.CurrentStore.Id);

            if (model == null)
                model = new SearchModel();


            ViewBag.vendors = "";

            model = _catalogModelFactory.PrepareNewSearchModel(model, command);
            return View(model);
        }

        //SearchModel model, CatalogPagingFilteringModel command
        public virtual IActionResult SearchTermAutoComplete(string term)
        {
            if (string.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
                return Content("");

            //products
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;            

            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                keywords: term,
                languageId: _workContext.WorkingLanguage.Id,
                visibleIndividuallyOnly: true,
                pageSize: productNumber);

                    for (int i = 0; i < products.Count; i++)
                    {
                        var vendor = _vendorService.GetVendorById(products[i].VendorId);
                        if (vendor != null && !vendor.Active)
                        {
                            products.RemoveAt(i);
                            i--;
                        }

                    }

  
        
            var showLinkToResultSearch = _catalogSettings.ShowLinkToAllResultInSearchAutoComplete && (products.TotalCount > productNumber);

            var models =  _productModelFactory.PrepareProductOverviewModels(products, true, _catalogSettings.ShowProductImagesInSearchAutoComplete, _mediaSettings.AutoCompleteSearchThumbPictureSize).ToList();

            #region vendor
            var vendors = _vendorService.GetAllVendors(term, "", 0, int.MaxValue, false, customer: _workContext.CurrentCustomer);
            List<VendorModel> vendorList = new List<VendorModel>();
            if (vendors.Any())
            {
                foreach (var vendor in vendors)
                {
                    var vendorView = new VendorModel
                    {
                        Id = vendor.Id,
                        Name = _localizationService.GetLocalized(vendor, x => x.Name),
                        Description = _localizationService.GetLocalized(vendor, x => x.Description),
                        MetaKeywords = _localizationService.GetLocalized(vendor, x => x.MetaKeywords),
                        MetaDescription = _localizationService.GetLocalized(vendor, x => x.MetaDescription),
                        MetaTitle = _localizationService.GetLocalized(vendor, x => x.MetaTitle),
                        SeName = _urlRecordService.GetSeName(vendor),
                        AllowCustomersToContactVendors = _vendorSettings.AllowCustomersToContactVendors,
                        VendorUrl = Url.RouteUrl("Vendor", new { SeName = _urlRecordService.GetSeName(vendor) })
                    };

                    //prepare picture model
                    int pictureSize = _mediaSettings.AutoCompleteSearchThumbPictureSize;

                    var pictureCacheKey = _cacheKeyService.PrepareKeyForDefaultCache(NopModelCacheDefaults.VendorPictureModelKey,
                    vendor.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    vendorView.PictureModel = _staticCacheManager.Get(pictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(vendor.PictureId);
                        var pictureModel = new PictureModel
                        {
                            // _pictureService.GetPictureUrl(ref picture, pictureSize),
                            FullSizeImageUrl = _pictureService.GetPictureUrl(ref picture, defaultPictureType: PictureType.VendorOrEntity),
                            ImageUrl = _pictureService.GetPictureUrl(ref picture, pictureSize, defaultPictureType: PictureType.VendorOrEntity),
                            Title = string.Format(_localizationService.GetResource("Media.Vendor.ImageLinkTitleFormat"), vendorView.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Vendor.ImageAlternateTextFormat"), vendorView.Name)
                        };
                        return pictureModel;
                    });
                    vendorList.Add(vendorView);
                }
            }
            #endregion


            var result = (from p in models
                    select new
                    {
                        label = p.Name,
                        productprice = p.ProductPrice.Price,
                        producturl = Url.RouteUrl("Product", new {SeName = p.SeName}),
                        productpictureurl = p.DefaultPictureModel.ImageUrl,
                        showlinktoresultsearch = showLinkToResultSearch
                    })
                .ToList();

            var vendorresult = (from p in vendorList
                                select new
                                {
                                    label = p.Name,
                                    productprice = "",
                                    producturl = Url.RouteUrl("Vendor", new { SeName = p.SeName }),
                                    productpictureurl = p.PictureModel.ImageUrl,
                                    showlinktoresultsearch = showLinkToResultSearch
                                })
       .ToList();

            result.AddRange(vendorresult);
            return Json(result);
        }

        public ActionResult VendorSearchTermAutoComplete(string term)
        {
            if (String.IsNullOrWhiteSpace(term) || term.Length < _catalogSettings.ProductSearchTermMinimumLength)
                return Content("");

            //products
            var productNumber = _catalogSettings.ProductSearchAutoCompleteNumberOfProducts > 0 ?
                _catalogSettings.ProductSearchAutoCompleteNumberOfProducts : 10;

            var products = _productService.SearchProducts(
                storeId: _storeContext.CurrentStore.Id,
                keywords: term,
                languageId: _workContext.WorkingLanguage.Id,
                visibleIndividuallyOnly: true,
                pageSize: productNumber);


            var models = _productModelFactory.PrepareProductOverviewModels(products, false, _catalogSettings.ShowProductImagesInSearchAutoComplete, _mediaSettings.AutoCompleteSearchThumbPictureSize).ToList();
            var result = (from p in models
                          select new
                          {
                              label = p.Name,
                              producturl = Url.RouteUrl("Product", new { SeName = p.SeName }),
                              productpictureurl = p.DefaultPictureModel.ImageUrl,
                              showlinktoresultsearch = p.DefaultPictureModel.ImageUrl
                          })
                .ToList();

            var vendors = _vendorService.GetAllVendors(term, "", 0, int.MaxValue, false,customer:_workContext.CurrentCustomer);
            List<VendorModel> vendorList = new List<VendorModel>();
            if (vendors.Any())
            {
                foreach (var vendor in vendors)
                {
                    var vendorView = new VendorModel
                    {
                        Id = vendor.Id,
                        Name = _localizationService.GetLocalized(vendor, x => x.Name),
                        Description = _localizationService.GetLocalized(vendor, x => x.Description),
                        MetaKeywords = _localizationService.GetLocalized(vendor, x => x.MetaKeywords),
                        MetaDescription = _localizationService.GetLocalized(vendor, x => x.MetaDescription),
                        MetaTitle = _localizationService.GetLocalized(vendor, x => x.MetaTitle),
                        SeName = _urlRecordService.GetSeName(vendor),
                        AllowCustomersToContactVendors = _vendorSettings.AllowCustomersToContactVendors,
                        VendorUrl = Url.RouteUrl("Vendor", new { SeName = _urlRecordService.GetSeName(vendor) })
                    };

                    //prepare picture model
                    int pictureSize = _mediaSettings.AutoCompleteSearchThumbPictureSize;

                    var pictureCacheKey = _cacheKeyService.PrepareKeyForDefaultCache(NopModelCacheDefaults.VendorPictureModelKey,
                    vendor.Id, pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);
                    vendorView.PictureModel = _staticCacheManager.Get(pictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(vendor.PictureId);
                        var pictureModel = new PictureModel
                        {
                            // _pictureService.GetPictureUrl(ref picture, pictureSize),
                            FullSizeImageUrl = _pictureService.GetPictureUrl(ref picture, defaultPictureType: PictureType.VendorOrEntity),
                            ImageUrl = _pictureService.GetPictureUrl(ref picture, pictureSize, defaultPictureType: PictureType.VendorOrEntity),
                            Title = string.Format(_localizationService.GetResource("Media.Vendor.ImageLinkTitleFormat"), vendorView.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Vendor.ImageAlternateTextFormat"), vendorView.Name)
                        };
                        return pictureModel;
                    });
                    vendorList.Add(vendorView);
                }
            }
            return Json(vendorList);
        }

        #endregion
        #region Campaigns
        //[NopHttpsRequirement(SslRequirement.No)]
        public ActionResult Campaign(int campaignId, CatalogPagingFilteringModel command)
        {
            var campaign = _campaignService.GetCampaignById(campaignId);
            if (campaign == null || !campaign.Active)
                return NotFound();

            var model = campaign.ToModel();

            //sorting
            //PrepareSortingOptions(model.PagingFilteringContext, command);
            //view mode
            //PrepareViewModes(model.PagingFilteringContext, command);
            //page size
            //PreparePageSizeOptions(model.PagingFilteringContext, command,
            //    campaign.AllowCustomersToSelectPageSize,
            //    campaign.PageSizeOptions,
            //    campaign.PageSize);

            var pictureSize = _mediaSettings.CategoryThumbPictureSize;

            //products
            command.PageNumber = command.PageNumber > 0 ? command.PageNumber : 1;
            command.PageSize = command.PageSize > 0 ? command.PageSize : 6;

            var campaignProducts = _campaignService.GetProductCampaignsByCampaignId(campaignId,
                pageIndex: command.PageNumber - 1, pageSize: command.PageSize);
            var products = campaignProducts
                .Select(cp => cp.Product);
            //var products = _productService.SearchProducts(out filterableSpecificationAttributeOptionIds,
            //    true,
            //    storeId: _storeContext.CurrentStore.Id,
            //    visibleIndividuallyOnly: true,
            //    featuredProducts: _catalogSettings.IncludeFeaturedProductsInNormalLists ? null : (bool?)false,
            //    //filteredSpecs: alreadyFilteredSpecOptionIds,
            //    orderBy: (ProductSortingEnum)command.OrderBy,
            //    pageIndex: command.PageNumber - 1,
            //    pageSize: command.PageSize);
            if(products.FirstOrDefault() == null)
                products = campaignProducts.Select(cp => _productService.GetProductById(cp.ProductId));

            model.Products =_productModelFactory.PrepareProductOverviewModels(products).ToList();

            model.PagingFilteringContext.LoadPagedList(campaignProducts);

            //display "edit" (manage) link
            if (_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel) && _permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                DisplayEditLink(Url.Action("Edit", "Campaign", new { id = campaign.Id, area = "Admin" }));

            //activity log
            _customerActivityService.InsertActivity("PublicStore.ViewCampaign", _localizationService.GetResource("ActivityLog.PublicStore.ViewCampaign"), campaign);

            return View(model);
        }

        //public IActionResult HomePageBannerCampaigns()
        //{
       

           
        //}
        #endregion
    }
}