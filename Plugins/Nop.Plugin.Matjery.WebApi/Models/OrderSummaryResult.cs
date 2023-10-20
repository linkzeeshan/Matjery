using System.Collections.Generic;

namespace Nop.Plugin.Matjery.WebApi.Models
{
	public class OrderSummaryResult
	{
		public bool DisplayTax
		{
			get;
			set;
		}

		public bool DisplayTaxRates
		{
			get;
			set;
		}

		public IList<OrderSummaryResult.GiftCard> GiftCards
		{
			get;
			set;
		}

		public string OrderTotal
		{
			get;
			set;
		}

		public string OrderTotalDiscount
		{
			get;
			set;
		}

		public string PaymentMethodAdditionalFee
		{
			get;
			set;
		}

		public int RedeemedRewardPoints
		{
			get;
			set;
		}

		public string RedeemedRewardPointsAmount
		{
			get;
			set;
		}

		public bool RequiresShipping
		{
			get;
			set;
		}

		public string SelectedShippingMethod
		{
			get;
			set;
		}

		public string Shipping
		{
			get;
			set;
		}

		public string SubTotal
		{
			get;
			set;
		}

		public string SubTotalDiscount
		{
			get;
			set;
		}

		public string Tax
		{
			get;
			set;
		}

		public IList<OrderSummaryResult.TaxRate> TaxRates
		{
			get;
			set;
		}

		public string WillEarnRewardPointsText
		{
			get;
			set;
		}

		public OrderSummaryResult()
		{
			this.TaxRates = new List<OrderSummaryResult.TaxRate>();
			this.GiftCards = new List<OrderSummaryResult.GiftCard>();
		}

		public class GiftCard
		{
			public string Amount
			{
				get;
				set;
			}

			public string CouponCode
			{
				get;
				set;
			}

			public int Id
			{
				get;
				set;
			}

			public string Remaining
			{
				get;
				set;
			}

			public GiftCard()
			{
			}
		}

		public class TaxRate
		{
			public int Id
			{
				get;
				set;
			}

			public string Rate
			{
				get;
				set;
			}

			public string Value
			{
				get;
				set;
			}

			public TaxRate()
			{
			}
		}
	}
}