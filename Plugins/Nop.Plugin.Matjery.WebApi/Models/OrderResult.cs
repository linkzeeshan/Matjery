using System;
using System.Collections.Generic;

namespace Nop.Plugin.Matjery.WebApi.Models
{
	public class OrderResult
	{
		public AddressResult BillingAddress
		{
			get;
			set;
		}

		public virtual int BillingAddressId
		{
			get;
			set;
		}

		public virtual string CheckoutAttributeDescription
		{
			get;
			set;
		}

		public virtual string CheckoutAttributesXml
		{
			get;
			set;
		}

		public virtual string CreatedOnUtc
		{
			get;
			set;
		}

		public virtual decimal CurrencyRate
		{
			get;
			set;
		}

		public virtual string CustomerCurrencyCode
		{
			get;
			set;
		}

		public virtual int CustomerId
		{
			get;
			set;
		}

		public virtual int CustomerLanguageId
		{
			get;
			set;
		}

		public virtual int CustomerTaxDisplayTypeId
		{
			get;
			set;
		}

		public virtual bool Deleted
		{
			get;
			set;
		}

		public virtual decimal OrderDiscount
		{
			get;
			set;
		}

		public virtual Guid OrderGuid
		{
			get;
			set;
		}

		public int OrderId
		{
			get;
			set;
		}

	    public ICollection<OrderProductResult> OrderProducts { get; set; }

        public virtual decimal OrderShippingExclTax
		{
			get;
			set;
		}

		public virtual decimal OrderShippingInclTax
		{
			get;
			set;
		}

		public virtual Nop.Core.Domain.Orders.OrderStatus OrderStatus
		{
			get;
			set;
		}

		public virtual decimal OrderSubTotalDiscountExclTax
		{
			get;
			set;
		}

		public virtual decimal OrderSubTotalDiscountInclTax
		{
			get;
			set;
		}

		public virtual decimal OrderSubtotalExclTax
		{
			get;
			set;
		}

		public virtual decimal OrderSubtotalInclTax
		{
			get;
			set;
		}

		public virtual decimal OrderTax
		{
			get;
			set;
		}

		public virtual decimal OrderTotal
		{
			get;
			set;
		}

		public virtual DateTime? PaidDateUtc
		{
			get;
			set;
		}

		public virtual decimal PaymentMethodAdditionalFeeExclTax
		{
			get;
			set;
		}

		public virtual decimal PaymentMethodAdditionalFeeInclTax
		{
			get;
			set;
		}

		public virtual string PaymentMethodSystemName
		{
			get;
			set;
		}

		public virtual Nop.Core.Domain.Payments.PaymentStatus PaymentStatus
		{
			get;
			set;
		}

		public AddressResult ShippingAddress
		{
			get;
			set;
		}

		public virtual int? ShippingAddressId
		{
			get;
			set;
		}

		public virtual string ShippingMethod
		{
			get;
			set;
		}

		public virtual string ShippingRateComputationMethodSystemName
		{
			get;
			set;
		}

		public virtual Nop.Core.Domain.Shipping.ShippingStatus ShippingStatus
		{
			get;
			set;
		}

		public virtual string TaxRates
		{
			get;
			set;
		}

		public virtual string VatNumber
		{
			get;
			set;
		}
        public string OrderStatusText { get; set; }
        public string OrderShipping { get; set; }
        public string CreatedOnUtcMonth { get; internal set; }
        public string CreatedOnUtcYear { get; internal set; }
        public IList<ShipmentResult> Shipments { get; set; }
        public bool CanPlaceBlackPoint { get; set; }

        public OrderResult()
		{
        }
	}
}