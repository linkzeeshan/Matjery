using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.NopWarehouse.VendorReview.Models
{
    public class VendorReviewHelpfulnessModel : BaseNopModel
    {
        public int HelpfulNoTotal { get; set; }
        public int HelpfulYesTotal { get; set; }
        public int VendorReviewId { get; set; }
    }
}
