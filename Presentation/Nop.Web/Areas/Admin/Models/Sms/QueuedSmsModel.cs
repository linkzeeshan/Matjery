using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;

namespace Nop.Web.Areas.Admin.Models.Sms
{
    public partial class QueuedSmsModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.System.QueuedSms.Fields.Id")]
        public override int Id { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedSms.Fields.Priority")]
        public string PriorityName { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedSms.Fields.Message")]
        public string Message { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedSms.Fields.IsRtl")]
        public bool IsRtl { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedSms.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedSms.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedSms.Fields.SentTries")]
        public int SentTries { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedSms.Fields.SentOn")]
        public DateTime? SentOn { get; set; }
    }
}