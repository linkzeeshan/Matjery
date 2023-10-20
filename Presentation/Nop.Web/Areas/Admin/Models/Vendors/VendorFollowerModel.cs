using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Web.Areas.Admin.Models.Vendors
{
    public class VendorFollowerModel : BaseNopEntityModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public bool Unfollowed { get; set; }
        public string Status { get; set; }
        public DateTime FollowOnUtc { get; set; }
        public DateTime UnFollowOnUtc { get; set; }
        public string FollowOnUtcStr { get; set; }
        public string UnFollowOnUtcStr { get; set; }
    }
}
