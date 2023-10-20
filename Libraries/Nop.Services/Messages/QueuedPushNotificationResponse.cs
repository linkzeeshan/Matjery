using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Messages
{
    class QueuedPushNotificationResponse
    {
        [JsonProperty("multicast_id")]
        public string MulticastId { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("failure")]
        public bool Failure { get; set; }
    }
}
