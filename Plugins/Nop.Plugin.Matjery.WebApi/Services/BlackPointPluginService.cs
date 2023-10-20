using Nop.Core.Domain.BlackPoints;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class BlackPointPluginService : BasePluginService, IBlackpointPluginService
    {
        public bool AddBlackPoint(ParamsModel.BlackPointParamsModel model)
        {
            if (!_customerService.IsRegistered(this._workContext.CurrentCustomer))
                throw new ApplicationException("Not allowed");

     
            Order order = _orderService.GetOrderById(model.OrderId);
            var orderitems=_orderService.GetOrderItems(order.Id);
            if (!this.IsValidOrder(order))
                throw new ApplicationException("Not found");

            if (!_blackPointService.CanPlaceBlackPoint(_workContext.CurrentCustomer, order))
                throw new ApplicationException("Not allowed to add blackpoint");

            Customer currentCustomer = _workContext.CurrentCustomer;

            //var vendorId = order.OrderItems.Select(orderItem => orderItem.Product.VendorId)
            //    .FirstOrDefault();

            var vendorOrCustomerId = 0;
            if (orderitems != null)
            {
                var item = orderitems.Select(orderItem => orderItem.ProductId).FirstOrDefault();
                var Productitem = _productService.GetProductById(item);
                vendorOrCustomerId = Productitem.VendorId;
            }
                

            var point = new BlackPoint
            {
                Comment = model.Message,
                AddedByCustomerId = _workContext.CurrentCustomer.Id,
                AddedOnTypeId = (int)BlackPointTypeEnum.Vendor,
                Note = string.Empty,
                VendorOrCustomerId = vendorOrCustomerId,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = null,
                OrderId = model.OrderId,
                BlackPointStatus = (int)BlackPointStatusEnum.Pending
            };
            _blackPointService.InsertBlackPoint(point);

            _workflowMessageService.SendBlackPointNotificationToAdmin(point, _workContext.WorkingLanguage.Id);


            return true;
        }

        private bool IsValidOrder(Order order)
        {
            return order != null && !order.Deleted && this._workContext.CurrentCustomer.Id == order.CustomerId;
        }

    }
}
