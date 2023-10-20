using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Messages
{
    public partial class QueuedPushNotification : BaseEntity
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string ResponseLog { get; set; }
        public DateTime? SentOnUtc { get; set; }
        public int SentTries { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public int PriorityId { get; set; }
        public string RegisterationId { get; set; }
        public int CustomerId { get; set; }
        public string ExtraData { get; set; }
        public QueuedPushNotificationPriority Priority
        {
            get
            {
                return (QueuedPushNotificationPriority)this.PriorityId;
            }
            set
            {
                this.PriorityId = (int)value;
            }
        }
        public int UserStatusId { get; set; }
        public PushNotificationUserStatus PushNotificationUserStatus
        {
            get
            {
                return (PushNotificationUserStatus)this.UserStatusId;
            }
            set
            {
                this.UserStatusId = (int)value;
            }
        }

    }
}
