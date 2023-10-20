using System;
using System.Collections.Generic;
using System.Net;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public class ProductReviewsModelResult
    {
       public ProductReviewsModelResult()
        {
            Reviews = new List<ProductReviews>();
        }
        public string Message { get; set; }

        public HttpStatusCode Status { get; set; }

        public bool CanCurrentCustomerLeaveReview { get; set; }

        public int DefaultProductRatingValue { get; set; }

        public IList<ProductReviews> Reviews{get;set;}

        public int ProductId
        {
            get;
            set;
        }

        public string ProductName
        {
            get;
            set;
        }

        public string ProductSeName
        {
            get;
            set;
        }

        public class ProductReviews
        {
            public bool AllowViewingProfiles
            {
                get;
                set;
            }

            public int CustomerId
            {
                get;
                set;
            }

            public string CustomerName
            {
                get;
                set;
            }

            public int HelpfulNoTotal
            {
                get;
                set;
            }

            public int HelpfulYesTotal
            {
                get;
                set;
            }

            public int ProductReviewId
            {
                get;
                set;
            }

            public int Rating
            {
                get;
                set;
            }

            public string ReviewText
            {
                get;
                set;
            }

            public string Title
            {
                get;
                set;
            }

            public string CreatedON
            {
                get;
                set;
            }
        }

    }
}