using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Vendors
{
    public class VendorReviewSettings : ISettings
    {
        public bool AllowAnonymousUsersToReviewVendor { get; set; }
        public int DefaultVendorRatingValue { get; set; }
        public bool VendorReviewsMustBeApproved { get; set; }
        public string VendorReviewsWidgetZone { get; set; }
    }
}
