using Nop.Core;
using Nop.Core.Domain.Vendors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Vendors
{
    public interface IVendorRatingService
    {
        void DeleteVendorReview(VendorReview vendorReview);

        IPagedList<VendorReview> GetAllVendorReviews(int customerId = 0, int vendorId = 0, int storeId = 0, bool? approved = null,
            DateTime? fromUtc = null, DateTime? toUtc = null, string message = null, int pageNumber = 1, int pageSize = 2147483647);

        VendorReview GetVendorReviewById(int itemId);

        IList<VendorReview> GetVendorReviewsByIds(int[] vendorReviewIds);

        void InsertVendorReview(VendorReview vendorReview);

        void UpdateVendorReview(VendorReview vendorReview);
    }
}
