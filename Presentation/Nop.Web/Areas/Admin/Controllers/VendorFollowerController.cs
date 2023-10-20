using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
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
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Vendors;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class VendorFollowerController : BaseAdminController
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
        private readonly IVendorFollowerService _vendorFollowerService;
        private readonly IGenericAttributeService _genericAttributeService;

        #endregion

        #region Constructor 

        public VendorFollowerController(IPermissionService permissionService, IPictureService pictureService, IVendorService vendorService,
            ICountryService countryService, IStateProvinceService stateProvinceService, IStaticCacheManager cacheManager,
            ILocalizationService localizationService, IWorkContext workContext, IOrderService orderService, IProductService productService,
            CatalogSettings catalogSettings, IWorkflowMessageService workflowMessageService, ICustomerActivityService customerActivityService,
            LocalizationSettings localizationSettings, ISettingService settingService, IStoreService storeService, IStoreContext storeContext,
            IDateTimeHelper dateTimeHelper, CustomerSettings customerSettings, RewardPointsSettings rewardPointsSettings, ForumSettings forumSettings,
            OrderSettings orderSettings, IWebHelper webHelper, ICustomerService customerService,
            IVendorRatingService vendorRatingService, IGenericAttributeService genericAttributeService, IVendorFollowerService vendorFollowerService)
        {
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
            _vendorFollowerService = vendorFollowerService;
        }

        #endregion


        public IActionResult ListPartial(int id)
        {
            var model = new VendorFollowerListModel();
            model.SearchVendorId = id;
            return View("_ListPartialf.cshtml", model);
        }


        [HttpPost]
        public IActionResult List(VendorReviewListModel searchmodel)
        {
            DateTime? nullable;
            DateTime? nullable1;
            DateTime? createdOnFrom = searchmodel.CreatedOnFrom;
            if (!createdOnFrom.HasValue)
            {
                createdOnFrom = null;
                nullable = createdOnFrom;
            }
            else
            {
                IDateTimeHelper dateTimeHelper = _dateTimeHelper;
                createdOnFrom = searchmodel.CreatedOnFrom;
                nullable = dateTimeHelper.ConvertToUtcTime(createdOnFrom.Value, _dateTimeHelper.CurrentTimeZone);
            }
            DateTime? createdOnFromValue = nullable;
            createdOnFrom = searchmodel.CreatedOnTo;
            if (!createdOnFrom.HasValue)
            {
                createdOnFrom = null;
                nullable1 = null;
            }
            else
            {
                IDateTimeHelper dateTimeHelper1 = _dateTimeHelper;
                createdOnFrom = searchmodel.CreatedOnTo;
                DateTime utcTime = dateTimeHelper1.ConvertToUtcTime(createdOnFrom.Value, _dateTimeHelper.CurrentTimeZone);
                nullable1 = utcTime.AddDays(1);
            }
            DateTime? createdToFromValue = nullable1;
            var vendorReviews = _vendorFollowerService.GetAllFollowers(vendorId: searchmodel.SearchVendorId, showUnFollowed: true,
                pageNumber: searchmodel.Page, pageSize: searchmodel.PageSize);

            var model = new AddReviewTovendorFollowmodel().PrepareToGrid(searchmodel, vendorReviews, () =>
            {
                return vendorReviews.Select(x =>
                {
                    var vendorFollowerModel = x.ToModel<VendorFollowerModel>();
                    vendorFollowerModel.Status = x.Unfollowed

                 ? _localizationService.GetResource("common.no")
                 : _localizationService.GetResource("common.yes");
                    var customer = _customerService.GetCustomerById(x.CustomerId);
                    vendorFollowerModel.CustomerName = customer!=null? _customerService.GetCustomerFullName(customer):"Unknown";
                    return vendorFollowerModel;
                });
            });

            return Json(model);
        }



        #region Helpers

        [NonAction]
        protected void PrepareVendorFollowersModel(VendorFollowerModel model, VendorFollower vendorReview)
        {
            if (vendorReview == null)
                throw new ArgumentNullException(nameof(vendorReview));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.VendorId = vendorReview.VendorId;
            model.VendorName = vendorReview.Vendor.Name;

        }

        #endregion
    }
}
