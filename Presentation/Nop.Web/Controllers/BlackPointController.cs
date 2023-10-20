using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.BlackPoints;
using Nop.Services.BlackPoints;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Seo;
using Nop.Services.Vendors;
using Nop.Web.Models.Blackpoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Controllers
{
    public class BlackPointController : BasePublicController
    {
        #region Fields
        private readonly IWorkContext _workContext;
        private readonly IVendorService _vendorService;
        private readonly IBlackPointService _blackPointService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        #endregion

        #region Constructors
        public BlackPointController(
            IWorkContext workContext,
            IVendorService vendorService,
            IBlackPointService blackPointService,
            ILocalizationService localizationService,
            ICustomerService customerService,
            IOrderService orderService,
            IWorkflowMessageService workflowMessageService,
            IProductService productService,
            IUrlRecordService urlRecordService)
        {
            this._workContext = workContext;
            this._vendorService = vendorService;
            this._blackPointService = blackPointService;
            this._urlRecordService = urlRecordService;
            this._localizationService = localizationService;
            this._customerService = customerService;
            this._workflowMessageService = workflowMessageService;
            this._orderService = orderService;
            _productService = productService;
        }
        #endregion

        // GET: BlackPoint
        public ActionResult Index(int OrderId,string mode="")
        {
            if (_customerService.IsGuest(_workContext.CurrentCustomer))
                return RedirectToRoute("HomePage");

            var vendorOrCustomerId = 0;
            var order = _orderService.GetOrderById(OrderId);
            
        
            if (order != null)
                vendorOrCustomerId = order.CustomerId;

            var type = _customerService.IsVendor(_workContext.CurrentCustomer) ? BlackPointTypeEnum.Customer : BlackPointTypeEnum.Vendor;

            if (_customerService.IsVendor(_workContext.CurrentCustomer))
            {
                type = BlackPointTypeEnum.Customer;
            }
            if(_customerService.IsVendor(_workContext.CurrentCustomer) && mode == "buyermode")
            {
                type = BlackPointTypeEnum.Vendor;
            }
            else if(!_customerService.IsVendor(_workContext.CurrentCustomer))
            {
                type = BlackPointTypeEnum.Vendor;
            }
            var model = new BlackPointModel();
            model.BlackPointType = type;

            //if he is vendor get the customer information
            if (_customerService.IsVendor(_workContext.CurrentCustomer) && type== BlackPointTypeEnum.Customer)
            {
                var customer = _customerService.GetCustomerById(vendorOrCustomerId);
                vendorOrCustomerId = customer.Id;
                if (customer == null || !customer.Active || customer.Deleted)
                    return RedirectToRoute("HomePage");

               
            }
            else  //if he is customer get the vendor information
            {
                var product = _productService.GetProductById(_orderService.GetOrderItems(OrderId).FirstOrDefault().ProductId);
                //var vendorId = items.Select(orderItem => orderItem.Product.VendorId)
                //       .FirstOrDefault();
                var vendorId = product!=null?product.VendorId:0;

                vendorOrCustomerId = vendorId;
                var vendor = _vendorService.GetVendorById(vendorId);
                if (vendor == null || !vendor.Active || vendor.Deleted)
                    return RedirectToRoute("HomePage");

            }
          
            model.VendorOrCustomerId = vendorOrCustomerId;
            model.OrderId = OrderId;
            model.Imgname = _workContext.WorkingLanguage.Id == 1 ? "blackpointEn.png" : "blackpointAr.png";
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(BlackPointModel model)
        {
            if (ModelState.IsValid)
            {
                var type = model.BlackPointType;// _customerService.IsVendor(_workContext.CurrentCustomer) ? BlackPointTypeEnum.Customer : BlackPointTypeEnum.Vendor;
                var point = new BlackPoint
                {
                    Comment = model.Comment,
                    AddedByCustomerId = _workContext.CurrentCustomer.Id,
                    AddedOnTypeId = (int)type,
                    Note = string.Empty,
                    VendorOrCustomerId = model.VendorOrCustomerId,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = null,
                    OrderId = model.OrderId.HasValue && model.OrderId > 0 ? model.OrderId : 0,
                    BlackPointStatus = (int)BlackPointStatusEnum.Pending
                };
                _blackPointService.InsertBlackPoint(point);
                //notify to admin a new black point added
                _workflowMessageService.SendBlackPointNotificationToAdmin(point, _workContext.WorkingLanguage.Id);
                //notify to vendor or customer who added the black point
                //var order = model.OrderId.HasValue && model.OrderId > 0 ? _orderService.GetOrderById(Convert.ToInt32(model.OrderId)):null;
                //if (point.AddedOnTypeId == (int)BlackPointTypeEnum.Customer)
                //{
                //    var customer = _customerService.GetCustomerById(point.VendorOrCustomerId);
                //    if (customer != null)
                //        _workflowMessageService.SendBlackPointBlockedNotificationToCustomer(customer, _workContext.WorkingLanguage.Id, addNotification: true);
                //}
                //else if (point.AddedOnTypeId == (int)BlackPointTypeEnum.Vendor)
                //{
                //    var vendor = _vendorService.GetVendorById(point.VendorOrCustomerId);
                //    if (vendor != null)
                //        _workflowMessageService.SendBlackPointBlockedNotificationToVendor(vendor, _workContext.WorkingLanguage.Id, addNotification: true);
                //}
                model.SuccessfullyAdded = true;
                model.Result = _localizationService.GetResource("blackpoint.pointadded");
            
            }

            return View(model);
        }
    }
}
