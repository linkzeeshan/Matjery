using System;
using Nop.Core;

namespace Nop.Core.Domain.Sms
{
    public class QueuedSms : BaseEntity
    {
        /// <summary>
        /// Gets or sets the send tries
        /// </summary>
        public int SentTries { get; set; }

        /// <summary>
        /// Gets or sets the sent date and time
        /// </summary>
        public DateTime? SentOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the body
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the date and time of item creation in UTC
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the priority
        /// </summary>
        public int PriorityId { get; set; }

        /// <summary>
        /// Gets or sets the priority
        /// </summary>
        public QueuedSmsPriority Priority
        {
            get { return (QueuedSmsPriority) this.PriorityId; }
            set { this.PriorityId = (int) value; }

        }

        public bool IsRtl { get; set; }
        public string PhoneNumber { get; set; }
    }
}
