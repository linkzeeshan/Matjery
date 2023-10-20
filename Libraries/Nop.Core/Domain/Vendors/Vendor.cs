using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Seo;
using System;

namespace Nop.Core.Domain.Vendors
{
    /// <summary>
    /// Represents a vendor
    /// </summary>
    public partial class Vendor : BaseEntity, ILocalizedEntity, ISlugSupported
    {
        public int? VendorOldSystemId { get; set; }
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; }
        public int? CreatedBy { get; set; }
        public string LicenseNo { get; set; }
        public int? UpdatedBy { get; set; }
        public int LicenseCopyId { get; set; }
        public string LicensedBy { get; set; }
        public bool EnrollForTraining { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the picture identifier
        /// </summary>
        public int PictureId { get; set; }

        /// <summary>
        /// Gets or sets the address identifier
        /// </summary>
        public int AddressId { get; set; }

        /// <summary>
        /// Gets or sets the admin comment
        /// </summary>
        public string AdminComment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
        public string EmiratesId { get; set; }
        public int EmiratesIdCopyId { get; set; }
        public string WhatsApp { get; set; }
        public string Instagram { get; set; }
        public string Twitter { get; set; }
        public string Facebook { get; set; }
        public string Googleplus { get; set; }
        public string BBM { get; set; }
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the date and time of vendor creation
        /// </summary>
        public DateTime? CreatedOnUtc { get; set; }
        /// <summary>
        /// Gets or sets the date and time of vendor update
        /// </summary>
        public DateTime? UpdatedOnUtc { get; set; }
        public bool? CanPublish { get; set; }
        public int? SupportedByFoundationId { get; set; }

        public int? FoundationAprovalByCustomerId { get; set; }
        public virtual Customer FoundationAprovalByCustomer { get; set; }

        public int? FoundationTypeId { get; set; }
        public FoundationTypeStatus FoundationTypeStatus
        {
            get
            {
                return (FoundationTypeStatus)this.FoundationTypeId;
            }
            set
            {
                this.FoundationTypeId = (int)value;
            }
        }
        public int? FoundationAprovalStatusId { get; set; }
        public FoundationAprovalStatus FoundationAprovalStatus
        {
            get
            {
                return this.FoundationAprovalStatusId.HasValue
                    ? (FoundationAprovalStatus)this.FoundationAprovalStatusId.Value
                    : FoundationAprovalStatus.None;
            }
            set
            {
                this.FoundationAprovalStatusId = (int)value;
            }
        }
        /// <summary>
        /// Gets or sets the meta keywords
        /// </summary>
        public string MetaKeywords { get; set; }

        /// <summary>
        /// Gets or sets the meta description
        /// </summary>
        public string MetaDescription { get; set; }

        /// <summary>
        /// Gets or sets the meta title
        /// </summary>
        public string MetaTitle { get; set; }

        /// <summary>
        /// Gets or sets the page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers can select the page size
        /// </summary>
        public bool AllowCustomersToSelectPageSize { get; set; }

        /// <summary>
        /// Gets or sets the available customer selectable page size options
        /// </summary>
        public string PageSizeOptions { get; set; }
        public int RegisterationTypeId { get; set; }
        /// <summary>
        /// Gets or sets the vendor type
        /// </summary>
        public VendorRegisterationType RegisterationType
        {
            get
            {
                return (VendorRegisterationType)this.RegisterationTypeId;
            }
            set
            {
                this.RegisterationTypeId = (int)value;
            }
        }
    }
}
