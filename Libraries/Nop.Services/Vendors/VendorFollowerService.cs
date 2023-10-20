using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Services.Vendors
{
    public class VendorFollowerService : IVendorFollowerService
    {
        private readonly IRepository<VendorFollower> _followerRepository;

        private readonly IRepository<Customer> _customerRepository;

        private readonly IStoreContext _storeContext;

        private readonly IWorkContext _workContext;

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Vendor> _vendorRepository;

        public VendorFollowerService(IRepository<VendorFollower> vendorReviewRepository,
            IRepository<Customer> customerRepository, IStoreContext storeContext, IWorkContext workContext)
        {
            this._followerRepository = vendorReviewRepository;
            this._customerRepository = customerRepository;
            this._storeContext = storeContext;
            this._workContext = workContext;
        }

        public void DeleteFollower(VendorFollower follower)
        {
            this._followerRepository.Delete(follower);
        }

        public IPagedList<VendorFollower> GetAllFollowers(int customerId = 0, int vendorId = 0, bool? showUnFollowed = false,
            DateTime? followOnFromUtc = null, DateTime? followOnToUtc = null, int pageNumber = 1, int pageSize = Int32.MaxValue)
        {
            IQueryable<VendorFollower> query = this._followerRepository.Table;
            if (showUnFollowed.HasValue && !showUnFollowed.Value)
            {
                query = from v in query
                        where !v.Unfollowed
                        select v;
            }
            if (customerId > 0)
            {
                query =
                    from v in query
                    where v.CustomerId == customerId
                    select v;
            }
            if (vendorId > 0)
            {
                query =
                    from v in query
                    where v.VendorId == vendorId
                    select v;
            }
            if (followOnFromUtc.HasValue)
            {
                query =
                    from v in query
                    where followOnFromUtc.Value <= v.FollowOnUtc
                    select v;
            }
            if (followOnToUtc.HasValue)
            {
                query =
                    from v in query
                    where followOnToUtc.Value >= v.UnFollowOnUtc
                    select v;
            }

            return new PagedList<VendorFollower>(query, pageNumber - 1, pageSize);
        }

        public VendorFollower GetFollowerById(int followerId)
        {
            return this._followerRepository.GetById(followerId);
        }

        public IList<VendorFollower> GetFollowersByIds(int[] followerIds)
        {
            var vendorReviews = new List<VendorFollower>();
            if (followerIds != null && followerIds.Length > 0)
            {
                var query = this._followerRepository.Table;
                query = query.Where(vr => followerIds.Contains(vr.Id));
                query = query.OrderBy(vr => vr.Id);

                vendorReviews.AddRange(query.ToList());
            }

            return vendorReviews;
        }

        public void InsertFollower(VendorFollower follower)
        {
            this._followerRepository.Insert(follower);
        }

        public void UpdateFollower(VendorFollower follower)
        {
            this._followerRepository.Update(follower);
        }
    }
}
