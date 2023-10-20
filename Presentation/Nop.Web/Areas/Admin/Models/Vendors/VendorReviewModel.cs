using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Vendors
{
    public class VendorReviewModel : BaseNopEntityModel
    {
        public bool AllowViewingProfiles { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public VendorReviewHelpfulnessModel Helpfulness { get; set; }

        public int Rating { get; set; }

        public string ReviewText { get; set; }

        public string Title { get; set; }

        public string WrittenOnStr { get; set; }
    }
}
