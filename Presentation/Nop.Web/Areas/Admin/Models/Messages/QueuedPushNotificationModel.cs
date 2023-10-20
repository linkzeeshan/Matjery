using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;

namespace Nop.Web.Areas.Admin.Models.Messages
{
    public partial class QueuedPushNotificationModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.System.QueuedPushNotification.Fields.Id")]
        public override int Id { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedPushNotification.Fields.Priority")]
        public string PriorityName { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedPushNotification.Fields.Title")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedPushNotification.Fields.Message")]
        public string Message { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedPushNotification.Fields.IsRtl")]
        public bool IsRtl { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedPushNotification.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedPushNotification.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedPushNotification.Fields.SentTries")]
        public int SentTries { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedPushNotification.Fields.SentOn")]
        public DateTime? SentOn { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedPushNotification.List.MaxSentTries")]
        public int SearchMaxSentTries { get; set; }
        [NopResourceDisplayName("Admin.System.QueuedPushNotification.List.RegisterationId")]
        public string RegisterationId { get; set; }

    }
}