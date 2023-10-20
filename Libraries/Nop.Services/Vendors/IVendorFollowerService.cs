using Nop.Core;
using Nop.Core.Domain.Vendors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Vendors
{
    public interface IVendorFollowerService
    {
        void DeleteFollower(VendorFollower follower);

        IPagedList<VendorFollower> GetAllFollowers(int customerId = 0, int vendorId = 0, bool? showUnFollowed = false,
            DateTime? followOnFromUtc = null, DateTime? followOnToUtc = null, int pageNumber = 1, int pageSize = Int32.MaxValue);

        VendorFollower GetFollowerById(int followerId);
        IList<VendorFollower> GetFollowersByIds(int[] followerIds);
        void InsertFollower(VendorFollower follower);
        void UpdateFollower(VendorFollower follower);
    }
}
