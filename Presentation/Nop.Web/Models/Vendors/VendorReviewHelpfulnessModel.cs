using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Models.Vendors
{
    public class VendorReviewHelpfulnessModel : BaseNopModel
    {
        public int HelpfulNoTotal { get; set; }
        public int HelpfulYesTotal { get; set; }
        public int VendorReviewId { get; set; }
    }
}
