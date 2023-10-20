using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Messages
{
    public interface IEmailClient
    {
        string From { get; set; }
        string ToList { get; set; }
        string CCList { get; set; }
        string Subject { get; set; }
        string EmailNote { get; set; }
        string IsHTMLReceived { get; set; }
        public string BaseUrl { get; set; }
        public string ApiMethod { get; set; }


        bool SendEmail();


    }
}
