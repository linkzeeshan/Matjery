using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.QueuedPushNotificationTask
{
    public class FcmNotificationSetting
    {
        public string SenderId { get; set; }
        public string ServerKey { get; set; }
        public string TeamId { get; set; }
        public string AppBundleIdentifier { get; set; }
    }
}
