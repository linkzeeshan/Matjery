using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Messages
{
    public class QueuedPushNotificationSearchModel : BaseSearchModel
    {

        [NopResourceDisplayName("Admin.QueuedPushNotificationTemplate.Fields.Name")]
        public string SearchName { get; set; }
        public int SearchMaxSentTries { get; set; }
    }
}
