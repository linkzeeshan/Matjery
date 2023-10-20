using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Services.Vendors
{
    public class VendorRatingService : IVendorRatingService
    {
        private readonly IRepository<VendorReview> _vendorReviewRepository;

        private readonly IRepository<Customer> _customerRepository;

        private readonly IStoreContext _storeContext;

        private readonly IWorkContext _workContext;

        public VendorRatingService(IRepository<VendorReview> vendorReviewRepository,
            IRepository<Customer> customerRepository, IStoreContext storeContext, IWorkContext workContext)
        {
            this._vendorReviewRepository = vendorReviewRepository;
            this._customerRepository = customerRepository;
            this._storeContext = storeContext;
            this._workContext = workContext;
        }

        public void DeleteVendorReview(VendorReview vendorReview)
        {
            this._vendorReviewRepository.Delete(vendorReview);
        }

        public IPagedList<VendorReview> GetAllVendorReviews(int customerId = 0, int vendorId = 0, int storeId = 0, bool? approved = null,
            DateTime? fromUtc = null, DateTime? toUtc = null, string message = null, int pageNumber = 1, int pageSize = Int32.MaxValue)
        {
            IQueryable<VendorReview> query = this._vendorReviewRepository.Table;
            if (approved.HasValue)
            {
                query =
                    from v in query
                    where v.IsApproved == approved.Value
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
            if (storeId > 0)
            {
                query =
                    from c in query
                    where c.StoreId == storeId
                    select c;
            }
            if (fromUtc.HasValue)
            {
                query =
                    from v in query
                    where fromUtc.Value <= v.CreatedOnUtc
                    select v;
            }

            if (toUtc.HasValue)
            {
                query =
                    from v in query
                    where toUtc.Value >= v.CreatedOnUtc
                    select v;
            }
            if (!string.IsNullOrEmpty(message))
            {
                query =
                    from v in query
                    where v.Title.Contains(message) || v.ReviewText.Contains(message)
                    select v;
            }
            List<VendorReview> records = (
                from r in query
                orderby r.CreatedOnUtc
                select r).ToList<VendorReview>();
            return new PagedList<VendorReview>(records, pageNumber - 1, pageSize);
        }

        public VendorReview GetVendorReviewById(int itemId)
        {
            VendorReview byId;
            if (itemId != 0)
            {
                byId = this._vendorReviewRepository.GetById(itemId);
            }
            else
            {
                byId = null;
            }
            return byId;
        }

        public IList<VendorReview> GetVendorReviewsByIds(int[] vendorReviewIds)
        {
            var vendorReviews = new List<VendorReview>();
            if (vendorReviewIds != null && vendorReviewIds.Length > 0)
            {
                var query = this._vendorReviewRepository.Table;
                query = query.Where(vr => vendorReviewIds.Contains(vr.Id));
                query = query.OrderBy(vr => vr.Id);

                vendorReviews.AddRange(query.ToList());
            }

            return vendorReviews;
        }

        public void InsertVendorReview(VendorReview vendorReview)
        {
            this._vendorReviewRepository.Insert(vendorReview);
        }

        public void UpdateVendorReview(VendorReview vendorReview)
        {
            this._vendorReviewRepository.Update(vendorReview);
        }
    }
}
