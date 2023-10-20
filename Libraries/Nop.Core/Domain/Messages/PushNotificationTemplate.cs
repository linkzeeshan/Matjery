using Nop.Core.Domain.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Messages
{
    public partial class PushNotificationTemplate : BaseEntity, ILocalizedEntity
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsActive { get; set; }
    }
}
