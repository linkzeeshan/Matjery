using System;
using System.Xml.Serialization;
using RestSharp;
using System.Net;
using Microsoft.Extensions.Logging;
using Nop.Services.Messages;
using Newtonsoft.Json;

namespace Nop.Services.Messages
{
    [XmlRoot(ElementName = "EMailDetails", Namespace = "http://PCS.Integration.SendEMailAttachments.EMailMetaData")]
    public class BizTalkEMailClient : IEmailClient
    {
        private ILogger<BizTalkEMailClient> _logger;

        public BizTalkEMailClient(ILogger<BizTalkEMailClient> logger)
        {
            this._logger = logger;
        }

        [XmlIgnore]
        public string BaseUrl { get; set; }

        [XmlIgnore]
        public string AuthUrl { get; set; }

        [XmlIgnore]
        public string AuthUserName { get; set; }

        [XmlIgnore]
        public string AuthPassword { get; set; }

        [XmlIgnore]
        public string ApiMethod { get; set; }
        [XmlIgnore]
        public string Proxy { get; set; }
        [XmlElement(ElementName = "From")]
        public string From { get; set; }
        [XmlElement(ElementName = "ToList")]
        public string ToList { get; set; }
        [XmlElement(ElementName = "CCList")]
        public string CCList { get; set; }
        [XmlElement(ElementName = "Subject")]
        public string Subject { get; set; }
        [XmlElement(ElementName = "EmailNote")]
        public string EmailNote { get; set; }
        [XmlElement(ElementName = "IsHTMLReceived")]
        public string IsHTMLReceived { get; set; }

       

        public bool SendEmail()
        {

            RestClient client = new RestSharp.RestClient(BaseUrl);
            if (!string.IsNullOrEmpty(Proxy) && !String.IsNullOrEmpty(Proxy.Trim()))
                client.Proxy = new WebProxy(Proxy);

            RestRequest request = new RestRequest(ApiMethod)
            {
                RequestFormat = DataFormat.Json
            };
            request.AddJsonBody(this);
            var response = client.Execute(request, Method.POST);
            _logger.LogError("Response:"+response.ToString());
            _logger.LogError("Request:" + this.ToString());

            return response.IsSuccessful;
        }


    }
}

