
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.BlackPoint;
using Nop.Core;
using Nop.Core.Domain.BlackPoints;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Services;
using Nop.Services.BlackPoints;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Vendors;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Linq;
using Nop.Web.Areas.Admin.Factories;
using Nop.Services.Orders;

namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class BlackPointController : BaseAdminController
    {
        #region Constants & Fields & Constructor 
        private readonly IPermissionService _permissionService;
        private readonly IBlackPointService _blackPointService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkContext _workContext;
        private readonly ILocalizationService _localizationService;
        private readonly IVendorService _vendorService;
        private readonly VendorSettings _vendorSettings;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly INotificationService _notificationService;
        public readonly  IBlackPointModelFactory _blackPointFactory;
        public readonly  IOrderService _orderService;

        public BlackPointController(IPermissionService permissionService,
            IBlackPointService blackPointService,
            IOrderService orderService,
            ICustomerService customerService,
            IWorkContext workContext,
            ILocalizationService localizationService,
            IVendorService vendorService,
            VendorSettings vendorSettings,
            IWorkflowMessageService workflowMessageService,
            IDateTimeHelper dateTimeHelper,
            IBlackPointModelFactory blackPointFactory,
            INotificationService notificationService)
        {
            _orderService = orderService;
            this._permissionService = permissionService;
            this._blackPointService = blackPointService;
            this._customerService = customerService;
            this._dateTimeHelper = dateTimeHelper;
            this._workContext = workContext;
            this._localizationService = localizationService;
            this._vendorService = vendorService;
            this._workflowMessageService = workflowMessageService;
            this._vendorSettings = vendorSettings;
            _notificationService = notificationService;
            _blackPointFactory = blackPointFactory;
        }
        #endregion
        // GET: BlackPoint
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }
        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns) || !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();
            //prepare model
            var model = _blackPointFactory.PrepareBlackPointSearchModel(new BlackPointSearchModel());
            return View(model);
        }
        [HttpPost]
        public IActionResult List(BlackPointSearchModel searchmodel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns) || !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedDataTablesJson();

            //prepare model
            var model = _blackPointFactory.PrepareBlackPointListModel(searchmodel);

            return Json(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();
            var point = _blackPointService.GetBlackPointById(id);
            if (point == null)
                return RedirectToAction("List");

            var model = _blackPointFactory.PrepareCustomerRoleModel(null, point);
            return View(model);
        }
        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Edit(BlackPointModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var point = _blackPointService.GetBlackPointById(model.Id);
            if (point == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                point.Note = model.Note;
                point.BlackPointStatus = model.PointStatusId;
                point.UpdatedOnUtc = DateTime.UtcNow;
                _blackPointService.UpdateBlackPoint(point);


                if (model.PointStatusId == (int)BlackPointStatusEnum.Approved)
                {

                    var order = model.OrderId != null? _orderService.GetOrderById(Convert.ToInt32(model.OrderId)):null;
                    var blackPointCount = _blackPointService.GetVendorOrCustomerCount(point.VendorOrCustomerId, blackPointStatus: (int)BlackPointStatusEnum.Approved);
                    if (point.AddedOnTypeId == (int)BlackPointTypeEnum.Vendor)
                    {
                        var vendor = _vendorService.GetVendorById(point.VendorOrCustomerId);
                        //notify to vendor when the comments approved
                        if (vendor != null)
                        {
                            _workflowMessageService.SendBlackPointBlockedNotificationToVendor(vendor, _workContext.WorkingLanguage.Id, true, order);
                            _workflowMessageService.SendBlackPointApprovedNotificationToCustomerOrVendor(vendor.Email, _localizationService.GetLocalized(vendor, x => x.Name), _workContext.WorkingLanguage.Id);
                            if (blackPointCount >= _vendorSettings.MaxBlackPointLimit)
                            {
                                vendor.Active = false;
                                _vendorService.UpdateVendor(vendor);
                                _workflowMessageService.SendBlackPointBlockedNotificationToVendor(vendor, _workContext.WorkingLanguage.Id);
                            }
                        }
                     
                    }
                    else if (point.AddedOnTypeId == (int)BlackPointTypeEnum.Customer)
                    {
                        var customer = _customerService.GetCustomerById(point.VendorOrCustomerId);
                        //notify to customer when the comments approved
                        if (customer != null)
                        {
                            _workflowMessageService.SendBlackPointBlockedNotificationToCustomer(customer, _workContext.WorkingLanguage.Id,true, order);
                            _workflowMessageService.SendBlackPointApprovedNotificationToCustomerOrVendor(customer.Email, _customerService.GetCustomerFullName(customer), _workContext.WorkingLanguage.Id);
                            if (blackPointCount >= _vendorSettings.MaxBlackPointLimit)
                            {
                                customer.Active = false;
                                _customerService.UpdateCustomer(customer);
                                _workflowMessageService.SendBlackPointBlockedNotificationToCustomer(customer, _workContext.WorkingLanguage.Id);
                            }
                        }
                      
                    }
                }
                //var notificationmessag = $"{_localizationService.GetLocalized(custom, x => x.FriendlyUrlName, _workContext.WorkingLanguage.Id)} {_localizationService.GetResource("Admin.Promotions.BlackPoint.Updated")}";
                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Promotions.BlackPoint.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = point.Id }) : RedirectToAction("List");
            }
            _blackPointFactory.PrepareCustomerRoleModel(model, point);
            return View(model);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var point = _blackPointService.GetBlackPointById(id);
            if (point == null)
                return RedirectToAction("List");

            _blackPointService.DeleteVendor(point);

            //var notificationmessag = $"{_localizationService.GetLocalized(custom, x => x.FriendlyUrlName, _workContext.WorkingLanguage.Id)} {_localizationService.GetResource("Admin.Promotions.BlackPoint.Updated")}";
            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Promotions.BlackPoint.Deleted"));
            return RedirectToAction("List");
        }
    }
}