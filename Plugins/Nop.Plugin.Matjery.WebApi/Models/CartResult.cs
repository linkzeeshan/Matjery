using System.Collections.Generic;

namespace Nop.Plugin.Matjery.WebApi.Models
{
	public class CartResult
	{
		public string Discount
		{
			get;
			set;
		}

		public string ImageTitle
		{
			get;
			set;
		}

		public string ImageUrl
		{
			get;
			set;
		}

		public int ProductId
		{
			get;
			set;
		}
		public bool InStock { get; set; }
		public string ProductName
		{
			get;
			set;
		}

		public int Quantity
		{
			get;
			set;
		}

		public string SKU
		{
			get;
			set;
		}

		public string SubTotal
		{
			get;
			set;
		}

		public string UnitPrice
		{
			get;
			set;
		}

		public IList<string> Warnings
		{
			get;
			set;
		}

	    public int VendorId { get; set; }
        public string VendorName { get; set; }
        public string OldPrice { get; set; }
	    public string Price { get; set; }
        public decimal CachedPriceValue { get; set; }

        public CartResult()
		{
			this.Warnings = new List<string>();
		}
	}
}