using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Seo;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Foundations
{
    public partial class Foundation : BaseEntity, ILocalizedEntity, ISlugSupported
    {
        /// <summary>
        /// Name of Foundation
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Foundation Email
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// Foundation Phone Number
        /// </summary>
        public string PhoneNumber { get; set; }
        /// <summary>
        /// Description of Foundation
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Picture Id of Foundation
        /// </summary>
        public int? PictureId { get; set; }
        /// <summary>
        /// Date Of creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
        /// <summary>
        /// Delete flag 
        /// </summary>
        public bool Deleted { get; set; }
    }
}
