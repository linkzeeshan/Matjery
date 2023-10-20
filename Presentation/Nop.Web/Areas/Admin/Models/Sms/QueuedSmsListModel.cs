using System;
using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Sms
{
    public partial class QueuedSmsListModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.System.QueuedSms.List.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? SearchStartDate { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedSms.List.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? SearchEndDate { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedSms.List.MaxSentTries")]
        public int SearchMaxSentTries { get; set; }

        [NopResourceDisplayName("Admin.System.QueuedSms.List.GoDirectlyToNumber")]
        public int GoDirectlyToNumber { get; set; }


    }
}