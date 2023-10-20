using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Customers
{
    public partial class CustomerTranslationMapping : BaseEntity
    {
        /// <summary>
        /// EntityId like productId vendorId etc
        /// </summary>
        public int EntityId { get; set; }
        /// <summary>
        /// EntityName like Product, vendor etc
        /// </summary>
        public String EntityName { get; set; }
        /// <summary>
        /// CustomerId the customerId that have translator role 
        /// </summary>
        public int CustomerId { get; set; }
        /// <summary>
        /// when translation is doem it will true
        /// </summary>
        public bool IsTranslated { get; set; }
        /// <summary>
        /// Gets or sets the Customer
        /// </summary>
        public virtual Customer Customer { get; set; }

    }
}
