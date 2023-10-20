using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Sms;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Nop.Services.Sms
{
    public partial class QueuedSmsSendTask : IScheduleTask
    {
        private readonly IQueuedSmsService _queuedSmsService;
        private readonly ILogger _logger;
        private readonly SmsPluginSettings _smsPluginSettings;



        public QueuedSmsSendTask(IQueuedSmsService queuedSmsService, ILogger logger, SmsPluginSettings smsPluginSettings)
        {
            this._queuedSmsService = queuedSmsService;
            this._logger = logger;
            _smsPluginSettings = smsPluginSettings;
        }

        public  virtual void Execute()
        {
            int maxTries = 3;
            var commonsettings = EngineContext.Current.Resolve<CommonSettings>();
            IPagedList<QueuedSms> queuedSmss = _queuedSmsService.SearchSms(null, null, true, maxTries, false, 0, 100);
            foreach (QueuedSms queuedSms in queuedSmss)
            {
                try
                {

                    #region Old
                    if (!string.IsNullOrEmpty(queuedSms.PhoneNumber))
                    {

                        if (commonsettings.IsBiztalkServer)
                        {
                            SendSMSNotification(queuedSms.PhoneNumber, queuedSms.IsRtl, queuedSms.Message);
                            queuedSms.SentOnUtc = DateTime.UtcNow;
                        }
                        else
                        {
                            //var query = HttpUtility.ParseQueryString(string.Empty);
                            //SMS format = 97156...
                            //Trim leading 0s
                            queuedSms.PhoneNumber = queuedSms.PhoneNumber.TrimStart('0');
                            if (!queuedSms.PhoneNumber.StartsWith("971"))
                                queuedSms.PhoneNumber = "971" + queuedSms.PhoneNumber;

                            //http://www.smartcall.ae
                            var xml = "<?xml version='1.0' encoding='utf-8' ?><SMS><MobileNumber>" + queuedSms.PhoneNumber + "</MobileNumber>";
                            xml += "<Message>" + queuedSms.Message + "</Message>";
                            xml += "<Unicode>1</Unicode>";
                            xml += "<CampaignID>0</CampaignID>";
                            xml += "<UserID>" + _smsPluginSettings.UserName + "</UserID>";
                            xml += "<Pwd>" + _smsPluginSettings.Password + "</Pwd></SMS>";

                            var httpClient = new HttpClient();
                            httpClient.BaseAddress = new Uri("http://www.smartcall.ae");
                            var content = new StringContent(xml, Encoding.UTF8, "text/plain");
                            HttpResponseMessage response = httpClient.PostAsync("/ClientAPIV3/submitXML.aspx", content).Result;
                            if (response.IsSuccessStatusCode)
                            {
                                HttpContent responseContent = response.Content;
                                string responseString = responseContent.ReadAsStringAsync().Result;
                                var xmlDoc = new XmlDocument();
                                xmlDoc.LoadXml(responseString);

                                XElement xElement = XElement.Parse(xmlDoc.InnerXml);
                                var status = xElement.Element("Status").Value;

                                if (status == "OK")
                                {
                                    string smsReferenceNo = "";
                                    if (!string.IsNullOrEmpty(smsReferenceNo))
                                        smsReferenceNo = xElement.Element("SMSREF").Value;

                                    _logger.Information(string.Format("SMS Sent. Ref: {0}. Response: {1} ", smsReferenceNo, xmlDoc.InnerXml));
                                    queuedSms.SentOnUtc = DateTime.UtcNow;
                                }
                                else
                                {
                                    _logger.Error(string.Format("Sending SMS: {0} failed. Response string: {1}",
                                        queuedSms.PhoneNumber, xmlDoc.InnerXml));
                                }




                            }
                        }
                    }
                    #endregion
                }
                catch (Exception exc)
                {
                    _logger.Error(string.Format("Sending SMS: {0}", exc.Message), exc);
                }
                finally
                {
                    queuedSms.SentTries = queuedSms.SentTries + 1;
                    _queuedSmsService.UpdateQueuedSms(queuedSms);
                }
            }
        }


        public void SendSMSNotification(string mobile, bool isRtl, string smsContent)
        {
            _logger.Information($"Sending Users sms");

            var smsToBizTalk = new SmsToBizTalk()
            {
                SendAlerts = new SendAlerts()
                {
                    Alert = new Alert()
                    {
                        Reference = mobile,
                        Channel = "SMS",
                        Subject = "Matjery",
                        Sender = "Maqta.ae",
                        Content = smsContent,
                        ContentLanguage = isRtl ? "Arabic" : "English",
                        Priority = "1",
                        Destination = mobile.StartsWith("0") ? "971" + mobile.TrimStart('0') : mobile
                    }
                }
            };
           SendSMS(_smsPluginSettings.SenderId, JsonConvert.SerializeObject(smsToBizTalk));
        }



        public async void SendSMS(string smsUri, string smsContent)
        {
            dynamic objResponse = new ExpandoObject();
            HttpResponseMessage responseMessage = new HttpResponseMessage();
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            StringContent myStringContent = new StringContent(smsContent, Encoding.UTF8, "application/json");
            responseMessage = await httpClient.PostAsync(smsUri, myStringContent);
            using (HttpContent contentData = responseMessage.Content)
            {
                string result = await contentData.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<dynamic>(result);
            }
            objResponse.Message = responseMessage.IsSuccessStatusCode.ToString();
            _logger.Information(string.Format("SMS Sent. Req: {0}. Response: {1} ", smsContent, objResponse.Message));

        }

    }
}
