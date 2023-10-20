using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Models.Customer
{
    public class CustomerProductReviewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSeName { get; set; }
        public string ProductImage { get; set; }
        public int CustomerId { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; } = String.Empty;
        public string ReviewText { get; set; }
    }
    public class CustomerProductReviewModelRequest
    {
        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("reviewtext")]
        public string ReviewText { get; set; }

        [JsonProperty("productid")]
        public int ProductId { get; set; }
    }

}
