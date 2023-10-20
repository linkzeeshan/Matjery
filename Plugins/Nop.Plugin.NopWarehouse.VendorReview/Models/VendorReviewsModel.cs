using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.NopWarehouse.VendorReview.Models
{
    public class VendorReviewsModel : BaseNopModel
    {
        public AddVendorReviewModel AddVendorReview { get; set; }

        public string Button { get; set; }

        public bool IsDisplayed { get; set; }

        public IList<VendorReviewModel> Items { get; set; }

        public int RatingSum { get; set; }

        public int TotalReviews { get; set; }

        public int VendorId { get; set; }

        public string VendorName { get; set; }

        public VendorReviewsModel()
        {
            this.Items = new List<VendorReviewModel>();
            this.AddVendorReview = new AddVendorReviewModel();
        }
    }
}
