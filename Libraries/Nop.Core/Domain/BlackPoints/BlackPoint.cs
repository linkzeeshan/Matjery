using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.BlackPoints
{
    public partial class BlackPoint : BaseEntity
    {
        /// <summary>
        /// Gets or sets the Comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets the CustomerId
        /// </summary>
        public int AddedByCustomerId { get; set; }
        /// <summary>
        /// Gets or sets the BlackPointType
        /// </summary>
        public int AddedOnTypeId { get; set; }
        /// <summary>
        /// Gets or sets the Note
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// Gets or sets the CreatedOnUtc
        /// </summary>
        public DateTime? CreatedOnUtc { get; set; }
        /// <summary>
        /// Gets or sets the UpdatedOnUtc
        /// </summary>
        public DateTime? UpdatedOnUtc { get; set; }
        /// <summary>
        /// Gets or sets the BlackPointStatus
        /// </summary>
        public int BlackPointStatus { get; set; }
        /// <summary>
        ///  Gets or sets the VendorId
        /// </summary>
        public int VendorOrCustomerId { get; set; }

        /// <summary>
        /// Gets or sets the OrderId
        /// </summary>
        public int? OrderId { get; set; }
    }
}
