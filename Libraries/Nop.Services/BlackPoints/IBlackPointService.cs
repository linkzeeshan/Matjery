using Nop.Core;
using Nop.Core.Domain.BlackPoints;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.BlackPoints
{
    public partial interface IBlackPointService
    {
        /// <summary>
        /// Gets a BlackPoint by BlackPoint identifier
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        BlackPoint GetBlackPointById(int Id);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="VendorOrCustomerId"></param>
        /// <returns></returns>
        int GetVendorOrCustomerCount(int VendorOrCustomerId, int blackPointStatus = 0);
        /// <summary>
        /// Delete a blackPoint
        /// </summary>
        /// <param name="blackPoint">Vendor</param>
        void DeleteVendor(BlackPoint blackPoint);

        bool CanPlaceBlackPoint(Customer customer, Order order);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="vendorOrCustomerId"></param>
        /// <param name="customerId"></param>
        /// <param name="orderId"></param>
        /// <param name="blackPointType"></param>
        /// <param name="blackPointStatus"></param>
        /// <returns></returns>
        IPagedList<BlackPoint> SearchBlackPoints(
            string Name="",
            int storeId = 0,
            int vendorOrCustomerId = 0,
            int customerId = 0,
            int orderId = 0,
            int blackPointType = 0,
            int blackPointStatus = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue);


        /// <summary>
        /// Inserts a blackPoint
        /// </summary>
        /// <param name="blackPoint">blackPoint</param>
        void InsertBlackPoint(BlackPoint blackPoint);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vendorId"></param>
        BlackPoint GetBlackPointByVendorId(int? vendorId);

        /// <summary>
        /// Updates the blackPoint
        /// </summary>
        /// <param name="blackPoint">blackPoint</param>
        void UpdateBlackPoint(BlackPoint blackPoint);

    }
}
