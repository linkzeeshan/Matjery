using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Messages
{
    /// <summary>
    /// Represents a product category mapping
    /// </summary>
    public partial class ProductCampaign : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the category identifier
        /// </summary>
        public int CampaignId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets the category
        /// </summary>
        public virtual Campaign Campaign { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        public virtual Product Product { get; set; }

    }

}
