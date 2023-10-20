using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Vendors
{
    public class AddVendorReviewModel : BaseNopModel
    {
        public bool CanCurrentCustomerLeaveReview { get; set; }

        [NopResourceDisplayName("reviews.fields.rating")]
        public int Rating { get; set; }

        public string Result { get; set; }

        [NopResourceDisplayName("reviews.fields.reviewtext")]
        public string ReviewText { get; set; }

        public bool SuccessfullyAdded { get; set; }


        [NopResourceDisplayName("reviews.fields.title")]
        public string Title { get; set; }
    }
}
