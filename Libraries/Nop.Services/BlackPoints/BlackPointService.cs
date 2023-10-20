using Nop.Core;
using Nop.Core.Domain.BlackPoints;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Services.BlackPoints
{
    public partial class BlackPointService : IBlackPointService
    {
        #region Fields

        private readonly IRepository<BlackPoint> _blackPointRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICustomerService _customerService;
        private readonly IRepository<Vendor> _vendorRepository;
        private readonly IRepository<GenericAttribute> _genericAttribute;
        private readonly IWorkContext _workContext;
        #endregion

        #region Ctor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="blackPointRepository"></param>
        /// <param name="localizedPropertyRepository"></param>
        public BlackPointService(
            IRepository<BlackPoint> blackPointRepository,
            IEventPublisher eventPublisher,
            ICustomerService customerService,
            IRepository<Vendor> vendorRepository,
            IRepository<GenericAttribute> genericAttribute,
            IRepository<LocalizedProperty> localizedPropertyRepository,
            IWorkContext workContext)
        {
            this._blackPointRepository = blackPointRepository;
            this._localizedPropertyRepository = localizedPropertyRepository;
            this._eventPublisher = eventPublisher;
            _customerService = customerService;
            _vendorRepository = vendorRepository;
            _genericAttribute = genericAttribute;
            _workContext = workContext;
        }

        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="blackPoint"></param>
        public void DeleteVendor(BlackPoint blackPoint)
        {
            if (blackPoint == null)
                throw new ArgumentNullException("blackPoint");
            _blackPointRepository.Delete(blackPoint);
            //event notification
            _eventPublisher.EntityDeleted(blackPoint);
        }

        public BlackPoint GetBlackPointById(int Id)
        {
            if (Id == 0)
                return null;

            return _blackPointRepository.GetById(Id); throw new NotImplementedException();
        }

        public void InsertBlackPoint(BlackPoint blackPoint)
        {
            if (blackPoint == null)
                throw new ArgumentNullException("blackPoint");

            _blackPointRepository.Insert(blackPoint);

            //event notification
            _eventPublisher.EntityInserted(blackPoint);
        }

        public void UpdateBlackPoint(BlackPoint blackPoint)
        {
            if (blackPoint == null)
                throw new ArgumentNullException("blackPoint");

            _blackPointRepository.Update(blackPoint);

            //event notification
            _eventPublisher.EntityUpdated(blackPoint);
        }

        public BlackPoint GetBlackPointByVendorId(int? vendorId)
        {
            if (vendorId == 0)
                return null;

            return _blackPointRepository.Table.FirstOrDefault(v => v.VendorOrCustomerId == vendorId);
        }

        public bool CanPlaceBlackPoint(Customer customer, Order order)
        {
            bool result = false;
            if ((_customerService.IsRegistered(customer) || _customerService.IsVendor(customer) || _customerService.IsFoundation(customer))
                && order.OrderStatus == OrderStatus.Complete)
            {
                var query = _blackPointRepository.Table;
                query = query.Where(x => x.AddedByCustomerId == _workContext.CurrentCustomer.Id && x.OrderId==order.Id);
                var blackPoints = query.ToList();// this.SearchBlackPoints(customerId: customer.Id, orderId: order.Id, pageSize: 1);
                if (blackPoints.Count == 0)
                    result = true;
            }

            return result;
        }

        public IPagedList<BlackPoint> SearchBlackPoints(string Name = "", int storeId = 0,
            int vendorOrCustomerId = 0,
            int customerId = 0,
            int orderId = 0,
            int blackPointType = 0,
            int blackPointStatus = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _blackPointRepository.Table;

            if (!string.IsNullOrEmpty(Name))
            {
                query = from q in query
                        join lpj in (from lp in _localizedPropertyRepository.Table where lp.LocaleKeyGroup == "Vendor" && lp.LocaleKey == "Name" select lp)
                        on q.VendorOrCustomerId equals lpj.EntityId
                        into join1
                        from j in join1.DefaultIfEmpty()
                        join vend in _vendorRepository.Table on q.VendorOrCustomerId equals vend.Id into join2
                        from v in join2.DefaultIfEmpty()
                        join lpj in _genericAttribute.Table on q.VendorOrCustomerId equals lpj.EntityId
                        into joing
                        from jg in joing.DefaultIfEmpty()
                        where (j.LocaleValue.Contains(Name) || v.Name.Contains(Name)) ||
                        (jg.KeyGroup == "Customer" && jg.Key == "FirstName" && jg.Value.Contains(Name)
                                     || jg.KeyGroup == "Customer" && jg.Key == "LastName" && jg.Value.Contains(Name))
                        select q;
                //    if(blackPointType== (int)BlackPointTypeEnum.Vendor)
                //    {
                //        query = from q in query
                //                join lpj in (from lp in _localizedPropertyRepository.Table where lp.LocaleKeyGroup == "Vendor" && lp.LocaleKey == "Name" select lp)
                //                on q.VendorOrCustomerId equals lpj.EntityId
                //                into join1
                //                from j in join1.DefaultIfEmpty()
                //                join vend in _vendorRepository.Table on q.VendorOrCustomerId equals vend.Id into join2
                //                from v in join2.DefaultIfEmpty()
                //                where j.LocaleValue.Contains(Name) || v.Name.Contains(Name)
                //                select q;
                //    }
                //    else if (blackPointType == (int)BlackPointTypeEnum.Customer)
                //    {
                //        query = from q in query
                //                join lpj in _genericAttribute.Table on q.VendorOrCustomerId equals lpj.EntityId
                //                into join1
                //                from j in join1.DefaultIfEmpty()
                //                where (j.KeyGroup == "Customer" && j.Key == "FirstName" && j.Value.Contains(Name)
                //                             || j.KeyGroup == "Customer" && j.Key == "LastName" && j.Value.Contains(Name))
                //                select q;
                //    }
            }

            //if we needd this filter we will add it later
            if (_workContext.CurrentVendor != null)
                vendorOrCustomerId = _workContext.CurrentVendor.Id;

            if (storeId > 0)
                storeId = 0;

            if (vendorOrCustomerId > 0)
                query = query.Where(x => x.VendorOrCustomerId == vendorOrCustomerId && x.BlackPointStatus == (int)BlackPointStatusEnum.Approved);

            if (customerId > 0)
                query = query.Where(x => x.AddedByCustomerId == customerId);

            if (orderId > 0)
                query = query.Where(x => x.OrderId == orderId);

            if (blackPointType > 0)
                query = query.Where(x => x.AddedOnTypeId == blackPointType);

            if (blackPointStatus > 0)
                query = query.Where(x => x.BlackPointStatus == blackPointStatus);

            query = query.OrderByDescending(o => o.CreatedOnUtc);

            //database layer paging
            return new PagedList<BlackPoint>(query, pageIndex, pageSize);
        }

        public int GetVendorOrCustomerCount(int VendorOrCustomerId, int blackPointStatus = 0)
        {
            var query = _blackPointRepository.Table;
            if (VendorOrCustomerId == 0)
                return 0;

            if (VendorOrCustomerId > 0)
                query = query.Where(x => x.VendorOrCustomerId == VendorOrCustomerId);

            if (blackPointStatus > 0)
                query = query.Where(x => x.BlackPointStatus == blackPointStatus);

            return query.Count();
        }



        #endregion
    }
}
