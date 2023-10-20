using Nop.Plugin.Matjery.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Interface
{
   public interface IOrderPluginService
    {
        (FinishOrderResult, ApiValidationResultResponse) PlaceOrder(ParamsModel.PlaceOrderParamsModel model);
        List<OrderResult> GetCustomerOrders();
        OrderResult GetOrderDetails(int orderId);
        bool CancelOrder(int orderId);
        void UpdateOrderItemByProductIdAnCustomerId(int productId, int customerId, int orderId = 0);
    }
}
