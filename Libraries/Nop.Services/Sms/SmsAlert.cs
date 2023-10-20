using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Sms
{
    public class Alert
    {
        public string Reference { get; set; }
        public string Channel { get; set; }
        public string Destination { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string Priority { get; set; }
        public string Sender { get; set; }
        public string ContentLanguage { get; set; }
    }


    public class SendAlerts
    {
        public Alert Alert { get; set; }
    }

    public class SmsToBizTalk
    {
        public SendAlerts SendAlerts { get; set; }
    }
}
