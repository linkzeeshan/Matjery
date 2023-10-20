using Nop.Core.Domain.Localization;
using System;

namespace Nop.Core.Domain.Messages
{
    /// <summary>
    /// Represents a campaign
    /// </summary>
    public partial class Campaign : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the subject
        /// </summary>
        public string Subject { get; set; }
        /// Gets or sets the picture identifier
        /// </summary>
        public string PictureId { get; set; }

        /// <summary>
        /// Gets or sets the picture identifier
        /// </summary>
        public string PictureIdMobile { get; set; }

        /// <summary>

        /// <summary>
        /// Gets or sets the body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the store identifier  which subscribers it will be sent to; set 0 for all newsletter subscribers
        /// </summary>
        public int StoreId { get; set; }
        public bool Active { get; set; }


        /// <summary>
        /// Gets or sets the customer role identifier  which subscribers it will be sent to; set 0 for all newsletter subscribers
        /// </summary>
        public int CustomerRoleId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
        public decimal MinDiscountPercentage { get; set; }


        /// <summary>
        /// Gets or sets the date and time in UTC before which this email should not be sent
        /// </summary>
        public DateTime? DontSendBeforeDateUtc { get; set; }
        public int DisplayAreaId { get; set; }
        public CampaignDisplayAreaType DisplayArea
        {
            get
            {
                return (CampaignDisplayAreaType)this.DisplayAreaId;
            }
            set
            {
                this.DisplayAreaId = (int)value;
            }
        }

    }
}
