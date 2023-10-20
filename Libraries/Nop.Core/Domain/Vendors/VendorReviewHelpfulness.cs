using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Vendors
{
	public class VendorReviewHelpfulness : BaseEntity
	{
		public int CustomerId { get; set; }
		public int VendorReviewId { get; set; }
		public bool WasHelpful { get; set; }
		public virtual VendorReview VendorReview { get; set; }
	}
}
