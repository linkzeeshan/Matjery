namespace Nop.Plugin.Matjery.WebApi.Models
{
	public class OrderProductResult
    {
		public int OrderId
		{
			get;
			set;
		}

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
        public VendorResult Vendor { get; set; }
        public string ImageUrl { get; set; }

        public OrderProductResult()
		{

        }
	}
}