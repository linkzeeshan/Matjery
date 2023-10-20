using System.Collections.Generic;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public class WishlistResult
    {
        public string Discount { get; set; }

        public string ImageTitle { get; set; }

        public string ImageUrl { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public string Sku { get; set; }

        public string SubTotal { get; set; }

        public string UnitPrice { get; set; }

        public IList<string> Warnings { get; set; }
        public bool InStock { get; set; }

        public WishlistResult()
        {
            this.Warnings = new List<string>();
        }
    }
}