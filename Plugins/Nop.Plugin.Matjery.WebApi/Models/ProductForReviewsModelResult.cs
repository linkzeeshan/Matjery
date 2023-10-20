using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public class ProductForReviewsModelResult
    {
        public int ProductId
        {
            get;
            set;
        }
        public int CustomerId
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
        public string ProductImage
        {
            get;
            set;
        }
        public IList<string> Messages { get; set; }

        public HttpStatusCode Status { get; set; }
    }
}
