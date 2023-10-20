using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Sms
{
    public class SmsPluginSettings : ISettings
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string SenderId { get; set; }
    }
}
