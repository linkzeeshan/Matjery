
using CorePush.Google;
using LinqToDB.DataProvider;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Mapping;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.QueuedPushNotificationTask;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Nop.Services.QueuedPushNotificationTask.GoogleNotification;

namespace Nop.Services.Messages
{
    public partial class QueuedPushNotificationService : IQueuedPushNotificationService
    {
        private readonly IRepository<QueuedPushNotification> _queuedPushNotificationRepository;
        private readonly IPushNotificationTemplateService _pushNotificationTemplateService;
        private readonly INopDataProvider _nopDataProvider;
        private readonly IDeviceInfoService _deviceInfoService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreService _storeService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ITokenizer _tokenizer;
        private readonly ICustomerService _customerService;
        private readonly ILanguageService _languageService;
        private readonly ISettingService _settingService;
        private readonly CommonSettings _commonSettings;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ILogger _logger;

        public QueuedPushNotificationService(
           IRepository<QueuedPushNotification> queuedPushNotificationRepository
          , IPushNotificationTemplateService pushNotificationTemplateService
            , INopDataProvider NopDataProvider, IDeviceInfoService deviceInfoService
            , IWorkContext workContext, IStoreContext storeContext, IMessageTokenProvider messageTokenProvider
            , IStoreService storeService, IEmailAccountService emailAccountService
            , ITokenizer tokenizer, ICustomerService customerService, ILanguageService languageService,
            IUrlHelperFactory UrlHelperFactory, IActionContextAccessor ActionContextAccessor,
            ISettingService settingService,
            ILocalizationService LocalizationService, IGenericAttributeService GenericAttributeService,
            ILogger logger
            )
        {
            _settingService = settingService;
            _urlHelperFactory = UrlHelperFactory;
            _actionContextAccessor = ActionContextAccessor;
            _localizationService = LocalizationService;
            _genericAttributeService = GenericAttributeService;
            this._queuedPushNotificationRepository = queuedPushNotificationRepository;
            this._pushNotificationTemplateService = pushNotificationTemplateService;
            _commonSettings = EngineContext.Current.Resolve<CommonSettings>();
            _nopDataProvider = NopDataProvider;
            this._deviceInfoService = deviceInfoService;
            this._workContext = workContext;
            this._storeContext = storeContext;
            this._messageTokenProvider = messageTokenProvider;
            this._storeService = storeService;
            this._emailAccountService = emailAccountService;
            this._emailAccountSettings = EngineContext.Current.Resolve<EmailAccountSettings>(); ;
            this._tokenizer = tokenizer;
            this._customerService = customerService;
            this._languageService = languageService;
            this._logger = logger;
        }

        public virtual IPagedList<QueuedPushNotification> SearchPushNotifications(bool loadNotSentItemsOnly, int maxSendTries
        , bool loadNewest, int? pushNotificationUserStatus = null, string registerationId = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _queuedPushNotificationRepository.Table;
            query = query.Where(qe => qe.SentTries < maxSendTries);

            if (loadNotSentItemsOnly)
                query = query.Where(qe => !qe.SentOnUtc.HasValue);

            if (pushNotificationUserStatus.HasValue)
                query = query.Where(qe => qe.UserStatusId == pushNotificationUserStatus.Value);

            if (!string.IsNullOrEmpty(registerationId))
                query = query.Where(qe => qe.RegisterationId == registerationId);

            query = loadNewest ?
                //load the newest records
                query.OrderByDescending(qe => qe.CreatedOnUtc) :
                //load by priority
                query.OrderByDescending(qe => qe.PriorityId).ThenBy(qe => qe.CreatedOnUtc);

            var queuedEmails = new PagedList<QueuedPushNotification>(query, pageIndex, pageSize);
            return queuedEmails;
        }
        public virtual IList<QueuedPushNotification> SearchPushNotifications(bool loadNotSentItemsOnly, int maxSendTries)
        {
            var query = _queuedPushNotificationRepository.Table;
            query = query.Where(qe => qe.SentTries < maxSendTries);

            if (loadNotSentItemsOnly)
                query = query.Where(qe => !qe.SentOnUtc.HasValue);

            //if (pushNotificationUserStatus.HasValue)
            //    query = query.Where(qe => qe.UserStatusId == pushNotificationUserStatus.Value);

            //if (!string.IsNullOrEmpty(registerationId))
            //    query = query.Where(qe => qe.RegisterationId == registerationId);

            query = query.OrderByDescending(qe => qe.CreatedOnUtc);

            //loadNewest ?
            ////load the newest records
            //query.OrderByDescending(qe => qe.CreatedOnUtc) :
            ////load by priority
            //query.OrderByDescending(qe => qe.PriorityId).ThenBy(qe => qe.CreatedOnUtc);

            return query.ToList();
        }
        public ResponseModel SendPushNotification(QueuedPushNotification queuedPushNotification)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                if (string.IsNullOrEmpty(queuedPushNotification.RegisterationId))
                    return null;
                var deviceInfo = _deviceInfoService.GetAllDeviceInfo(registerationId: queuedPushNotification.RegisterationId).LastOrDefault();
               
                if (deviceInfo != null)
                {
                    if (deviceInfo.Platform.ToLower() == "android")
                        response = SendPushNotificationAndriod(queuedPushNotification).GetAwaiter().GetResult();
                    else if (deviceInfo.Platform.ToLower() == "ios")
                        response = APNsAppleNotification(queuedPushNotification).GetAwaiter().GetResult();
                }
                return response;
            }catch(Exception ex)
            {
                _logger.Error("PushNotification Error", ex);
            }
            return response;

        }
        public virtual async Task<ResponseModel> SendPushNotificationAndriod(QueuedPushNotification queuedPushNotification)
        {
            NotificationModel notificationModel = new NotificationModel();
            notificationModel.IsAndroiodDevice = true;

            ResponseModel response = new ResponseModel();
            try
            {
                /* FCM Sender (Android Device) */
                FcmSettings settings = new FcmSettings()
                {
                    SenderId = _settingService.GetSetting("queuedPushNotification.fcmSenderId").Value,
                    ServerKey = _settingService.GetSetting("queuedPushNotification.fcmServerKey").Value
                };
                HttpClient httpClient = new HttpClient();
                notificationModel = new NotificationModel
                {
                    Title = queuedPushNotification.Title, // Regex.Replace(queuedPushNotification.Title, "<.*?>", String.Empty),
                    Body = queuedPushNotification.Message, // Regex.Replace(queuedPushNotification.Message, "<.*?>", String.Empty),
                    DeviceId = queuedPushNotification.RegisterationId
                };
                string authorizationKey = string.Format("keyy={0}", settings.ServerKey);
                string deviceToken = notificationModel.DeviceId;

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationKey);
                httpClient.DefaultRequestHeaders.Accept
                        .Add(new MediaTypeWithQualityHeaderValue("application/json"));

                DataPayload dataPayload = new DataPayload();
                dataPayload.Title = notificationModel.Title;
                dataPayload.Body = notificationModel.Body;

                GoogleNotification notification = new GoogleNotification();
                notification.Data = dataPayload;
                notification.Notification = dataPayload;


                var fcm = new FcmSender(settings, httpClient);
                var fcmSendResponse = await fcm.SendAsync(deviceToken, notification);

                    if (fcmSendResponse.IsSuccess())
                    {
                        response.IsSuccess = true;
                        response.Message = "Notification sent successfully to andriod. Device Id is :"+ queuedPushNotification.RegisterationId;
                        return response;
                    }
                    else
                    {
                        response.IsSuccess = false;
                        response.Message = "Error to andriod and Device Id is :" + queuedPushNotification.RegisterationId+" Error:  "+ fcmSendResponse.Results[0].Error;
                        return response;
                    }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public virtual string TestSendPushNotificationAndriod(QueuedPushNotification queuedPushNotification)
        {
            if (string.IsNullOrEmpty(queuedPushNotification.RegisterationId))
                return null;

            string reponse = string.Empty;
            var endPointUrl = "https://fcm.googleapis.com/fcm/send";
            try
            {
                var fcmServerKey = _settingService.GetSetting("queuedPushNotification.fcmServerKey");
                var fcmSenderId = _settingService.GetSetting("queuedPushNotification.fcmSenderId");
                WebRequest tRequest = WebRequest.Create(endPointUrl);
                tRequest.Method = "post";
                tRequest.ContentType = "application/json";

                var unReadNotifications = this.SearchPushNotifications(false, Int32.MaxValue, loadNewest: false,
                    pushNotificationUserStatus: (int)PushNotificationUserStatus.NoYetRead, registerationId: queuedPushNotification.RegisterationId).TotalCount;

                object deviceSpecificObj = null;
               
                //if(string.Equals(deviceInfo.Platform, "iOS", StringComparison.OrdinalIgnoreCase))
                //{
                deviceSpecificObj = new
                {
                    to = queuedPushNotification.RegisterationId,
                    //required by push plugin to work properly
                    notification = new
                    {
                        body = queuedPushNotification.Message,
                        title = queuedPushNotification.Title,
                        sound = "default",
                        //click_action: must be present with the specified value for Android
                        click_action = "FCM_PLUGIN_ACTIVITY",
                        badge = unReadNotifications
                    },
                    data = new
                    {
                        queuedPushNotificationId = queuedPushNotification.Id,
                        registerationId = queuedPushNotification.RegisterationId,
                        extraData = queuedPushNotification.ExtraData
                    },
                    //priority: must be set to "high" for delivering notifications on closed iOS apps
                    priority = "high"
                };
                //}
                //else
                //{
                //    deviceSpecificObj = new
                //    {
                //        to = queuedPushNotification.RegisterationId,
                //        //required by push plugin to work properly
                //        data = new
                //        {
                //            body = queuedPushNotification.Message,
                //            title = queuedPushNotification.Title,
                //            badge = unReadNotifications,
                //            notId = queuedPushNotification.Id
                //        },
                //        //if GCM Messages Not Arriving, priority: must be set to "high"
                //        priority = "high"
                //    };
                //}

                var json = JsonConvert.SerializeObject(deviceSpecificObj);
                Byte[] byteArray = Encoding.UTF8.GetBytes(json);
                tRequest.Headers.Add(string.Format("Authorization: key={0}", fcmServerKey.Value));
                tRequest.Headers.Add(string.Format("Sender: id={0}", fcmSenderId.Value));
                tRequest.ContentLength = byteArray.Length;

                using (Stream dataStream = tRequest.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    using (WebResponse tResponse = tRequest.GetResponse())
                    {
                        using (Stream dataStreamResponse = tResponse.GetResponseStream())
                        {
                            using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                reponse = tReader.ReadToEnd();
                            }
                        }
                    }
                }
                return reponse;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<ResponseModel> APNsAppleNotification(QueuedPushNotification queuedPushNotification)
        {
            ResponseModel responseModel = new ResponseModel();
            string certificatePath = string.Empty;
            try
            {
                string deviceToken = queuedPushNotification.RegisterationId;
                var urlval = _settingService.GetSetting("ApplePushnotificationRequestURL").Value;
                var url = string.Format(_settingService.GetSetting("ApplePushnotificationRequestURL").Value, deviceToken);
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                X509Certificate2 clientCertificate = LoadCertificateFromStore();
                X509Certificate2Collection certificatesCollection = new X509Certificate2Collection(clientCertificate);

                // request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Headers.TryAddWithoutValidation("apns-push-type", "alert"); // or background
                request.Headers.TryAddWithoutValidation("apns-id", Guid.NewGuid().ToString("D"));
                request.Headers.TryAddWithoutValidation("apns-expiration", Convert.ToString(0));
                request.Headers.TryAddWithoutValidation("apns-priority", Convert.ToString(10));
                request.Headers.TryAddWithoutValidation("apns-topic", "com.gwu.store");
                var payload = "{\"aps\":{\"alert\":\"" + queuedPushNotification.Message + "\",\"badge\":" + 1 + ",\"sound\":\"default\"}}";
                request.Content = new StringContent(payload);
                // also tried without yourcustomkey
                request.Version = new Version(2, 0); // tried directly with System.Net.HttpVersion.Version20;
                var handler = new HttpClientHandler();
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ClientCertificates.Add(clientCertificate);
                handler.SslProtocols = SslProtocols.Tls12; //| SslProtocols.Tls13 | SslProtocols.Tls11 | SslProtocols.Tls; // Tried with only Tls12
                handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
               // _logger.Information($"IOS Log : {handler.ServerCertificateCustomValidationCallback.ToString()}");
                using (HttpClient client = new HttpClient(handler))
                {
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                    HttpResponseMessage resp = await client.SendAsync(request).ContinueWith(responseTask =>
                    {
                        return responseTask.Result; // line of error
                    });
                    if (resp != null)
                    {
                        string apnsResponseString = await resp.Content.ReadAsStringAsync();
                        handler.Dispose();

                        if (resp.IsSuccessStatusCode)
                        {
                            responseModel = new ResponseModel
                            {
                                IsSuccess = true,
                                Message = "Notification sent successfully to IOS. Device Id is :" + queuedPushNotification.RegisterationId + " Response: " + apnsResponseString
                            };
                            _logger.Information("Apple PushNotification :" + certificatePath + ": " + responseModel.Message);
                        }
                        else
                        {

                            responseModel = new ResponseModel
                            {
                                IsSuccess = false,
                                Message = "Notification sent to IOS failed. Device Id is :" + queuedPushNotification.RegisterationId + " Error: " + apnsResponseString
                            };
                            _logger.Information($"Apple PushNotification response: {responseModel.Message} ");
                        }
                    }


                    handler.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Apple PushNotification Error : {certificatePath}", ex);
            }
            return responseModel;
        }
        public X509Certificate2 LoadCertificateFromStore(string CertificateFriendlyName = null)
        {
            string FriendlyName = "";
            if (CertificateFriendlyName == null)
            {
                FriendlyName = _settingService.GetSetting("AppleCertificateFriendlyName").Value;
            }
            else
            {
                FriendlyName = CertificateFriendlyName;
            }
#if DEBUG
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

#else
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
#endif
            store.Open(OpenFlags.ReadOnly);
            foreach (X509Certificate2 mCert in store.Certificates)
            {
                //to check newly certificate came.
                if (mCert.FriendlyName == FriendlyName)
                {
                    return mCert;
                }
            }
            return null;
        }
        public void QueueVendorAddProductNotification(Vendor vendor, Product product, Customer customer, string registerationId)
        {
            var messageTemplate = _pushNotificationTemplateService.GetActivePushNotificationTemplate("Vendor.CreateProduct.Notification");
            if (messageTemplate == null)
                return;

            var customerLanguageId = _genericAttributeService.GetAttribute<Customer, int>(customer.Id, NopCustomerDefaults.LanguageIdAttribute);
            if (customerLanguageId == 0)
                customerLanguageId = _workContext.WorkingLanguage.Id;

            var title = string.Format(_localizationService.GetLocalized(messageTemplate, m => m.Title, customerLanguageId));
            //Vendor {0} has published {1}. Visit {2} store for more information
            var message = string.Format(_localizationService.GetLocalized(messageTemplate, pnt => pnt.Message, customerLanguageId),
                _localizationService.GetLocalized(vendor, v => v.Name, customerLanguageId),
                _localizationService.GetLocalized(product, p => product.Name, customerLanguageId),
                _localizationService.GetLocalized(vendor, v => v.Name, customerLanguageId));

            var extraData = JsonConvert.SerializeObject(new { goTo = "product", id = product.Id });
            this.QueuePushNotification(title, message, registerationId, extraData);
        }

        public void QueueOrderPlaceCustomerNotification(Order order)
        {
            var messageTemplate = _pushNotificationTemplateService.GetActivePushNotificationTemplate("OrderPlaced.CustomerNotification");
            if (messageTemplate == null)
                return;

            //Thanks for buying from {2}. You can track your order#{0} at {1}
            // var urlHelper = new UrlHelper(HttpContext..Request.RequestContext);
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);


            string currentStoreUrl = _storeContext.CurrentStore.Url;
            if (currentStoreUrl.EndsWith("/"))
                currentStoreUrl = currentStoreUrl.TrimEnd('/');
            var storeName = _localizationService.GetLocalized(_storeContext.CurrentStore, x => x.Name, order.CustomerLanguageId);
            string orderDetailUrl = currentStoreUrl + urlHelper.RouteUrl("OrderDetails", new { orderId = order.Id });

            var title = string.Format(_localizationService.GetLocalized(messageTemplate, m => m.Title, order.CustomerLanguageId), storeName);
            string message = _localizationService.GetLocalized(messageTemplate, x => x.Message);
            message = String.Format(message, order.Id, orderDetailUrl, storeName);

            //grab registeration id
            var devicesInfo = _deviceInfoService.GetAllDeviceInfo(customerId: order.CustomerId);
            if (devicesInfo.Count > 0)
            {
                foreach (var dInfo in devicesInfo)
                {
                    if (string.IsNullOrEmpty(dInfo.RegisterationId))
                        continue;

                    var registerationId = dInfo.RegisterationId;
                    this.QueuePushNotification(title, message, registerationId);
                }
            }
        }

        public void QueueOrderPlaceVendorNotification(Order order, Vendor vendor)
        {
            if (order == null)
                return;

            if (vendor == null)
                return;

            var messageTemplate = _pushNotificationTemplateService.GetActivePushNotificationTemplate("OrderPlaced.VendorNotification");
            if (messageTemplate == null)
                return;

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;

            //email account
            var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, order.CustomerLanguageId, vendor.Id);
            _messageTokenProvider.AddCustomerTokens(tokens, order.Customer);

            //retrieve localized message template data
            var title = _localizationService.GetLocalized(messageTemplate, mt => mt.Title, order.CustomerLanguageId);
            var message = _localizationService.GetLocalized(messageTemplate, mt => mt.Message, order.CustomerLanguageId);

            //Replace subject and body tokens 
            var titleReplaced = _tokenizer.Replace(title, tokens, false);
            var messageReplaced = _tokenizer.Replace(message, tokens, true);

            //var extraData = JsonConvert.SerializeObject(new { goTo = "product", id = product.Id });

            var vendorCustomers = _customerService.GetAllCustomers(vendorId: vendor.Id);
            foreach (var vCustomer in vendorCustomers)
            {
                //grab registeration id
                var devicesInfo = _deviceInfoService.GetAllDeviceInfo(customerId: vCustomer.Id);
                if (devicesInfo.Count > 0)
                {
                    foreach (var dInfo in devicesInfo)
                    {
                        if (string.IsNullOrEmpty(dInfo.RegisterationId))
                            continue;

                        var registerationId = dInfo.RegisterationId;
                        this.QueuePushNotification(titleReplaced, messageReplaced, registerationId);
                    }
                }
            }
        }

        public void QueueOrderCompletedCustomerNotification(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;

            var messageTemplate = _pushNotificationTemplateService.GetActivePushNotificationTemplate("OrderCompleted.CustomerNotification");
            if (messageTemplate == null)
                return;

            //email account
            var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, order.CustomerLanguageId);
            var customer = _customerService.GetCustomerById(order.CustomerId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //retrieve localized message template data
            var title = _localizationService.GetLocalized(messageTemplate, mt => mt.Title, order.CustomerLanguageId);
            var message = _localizationService.GetLocalized(messageTemplate, mt => mt.Message, order.CustomerLanguageId);

            //Replace subject and body tokens 
            var titleReplaced = _tokenizer.Replace(title, tokens, false);
            var messageReplaced = _tokenizer.Replace(message, tokens, false);

            //var extraData = JsonConvert.SerializeObject(new { goTo = "product", id = product.Id });

            //grab registeration id
            var devicesInfo = _deviceInfoService.GetAllDeviceInfo(customerId: order.CustomerId);
            if (devicesInfo.Count > 0)
            {
                foreach (var dInfo in devicesInfo)
                {
                    if (string.IsNullOrEmpty(dInfo.RegisterationId))
                        continue;

                    var registerationId = dInfo.RegisterationId;
                    this.QueuePushNotification(titleReplaced, messageReplaced, registerationId);
                }
            }
        }

        public void QueueOrderCustomerNotification(Order order, string templateType)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            var store = _storeService.GetStoreById(order.StoreId) ?? _storeContext.CurrentStore;

            var messageTemplate = _pushNotificationTemplateService.GetActivePushNotificationTemplate("OrderCompleted.CustomerNotification");
            if (templateType == "OrderCancelled")
                messageTemplate = _pushNotificationTemplateService.GetActivePushNotificationTemplate("OrderCancelled.CustomerNotification");
            if (templateType == "OrderPlacedCustomerNotification")
                messageTemplate = _pushNotificationTemplateService.GetActivePushNotificationTemplate("OrderPlaced.CustomerNotification");
            if (templateType == "OrderPlacedVendorNotification")
                messageTemplate = _pushNotificationTemplateService.GetActivePushNotificationTemplate("OrderPlaced.VendorNotification");
            if (templateType == "OrderPlacedCustomerNotification")
                messageTemplate = _pushNotificationTemplateService.GetActivePushNotificationTemplate("OrderPlaced.CustomerNotification");
            if (templateType == "orderplacedstoreownernotification")
                messageTemplate = _pushNotificationTemplateService.GetActivePushNotificationTemplate("OrderPlaced.StoreOwnerNotification");
            if (templateType == "OrderRefundedStoreOwnerNotification")
                messageTemplate = _pushNotificationTemplateService.GetActivePushNotificationTemplate("OrderRefunded.StoreOwnerNotification");
            if (templateType == "OrderRefundedCustomerNotification")
                messageTemplate = _pushNotificationTemplateService.GetActivePushNotificationTemplate("OrderRefunded.CustomerNotification");

            if (messageTemplate == null)
                return;

            //email account
            var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);
            _messageTokenProvider.AddOrderTokens(tokens, order, order.CustomerLanguageId);
            var customer = _customerService.GetCustomerById(order.CustomerId);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            //retrieve localized message template data
            var title = _localizationService.GetLocalized(messageTemplate, mt => mt.Title, order.CustomerLanguageId);
            var message = _localizationService.GetLocalized(messageTemplate, mt => mt.Message, order.CustomerLanguageId);

            //Replace subject and body tokens 
            var titleReplaced = _tokenizer.Replace(title, tokens, false);
            var messageReplaced = _tokenizer.Replace(message, tokens, false);

            //var extraData = JsonConvert.SerializeObject(new { goTo = "product", id = product.Id });

            //grab registeration id
            var devicesInfo = _deviceInfoService.GetAllDeviceInfo(customerId: order.CustomerId);
            if (devicesInfo.Count > 0)
            {
                foreach (var dInfo in devicesInfo)
                {
                    if (string.IsNullOrEmpty(dInfo.RegisterationId))
                        continue;

                    var registerationId = dInfo.RegisterationId;
                    this.QueuePushNotification(titleReplaced, messageReplaced, registerationId);
                }
            }
        }
        public void QueueNewCampaignCustomerNotification(Campaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException("campaign");

            var store = _storeContext.CurrentStore;

            var messageTemplate = _pushNotificationTemplateService.GetActivePushNotificationTemplate("NewCampaign.CustomerNotification");
            if (messageTemplate == null)
                return;

            //email account
            var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = _emailAccountService.GetAllEmailAccounts().FirstOrDefault();

            //tokens
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

            //var extraData = JsonConvert.SerializeObject(new { goTo = "product", id = product.Id });
            int languageId = _languageService.GetAllLanguages().FirstOrDefault().Id;

            //grab registeration id
            var devicesInfo = _deviceInfoService.GetAllDeviceInfo();
            foreach (var dInfo in devicesInfo)
            {
                if (string.IsNullOrEmpty(dInfo.RegisterationId))
                    continue;

                if (dInfo.CustomerId > 0)
                {
                    var customer = _customerService.GetCustomerById(dInfo.CustomerId);
                    if (customer != null)
                        languageId = _genericAttributeService.GetAttribute<Customer, int>(customer.Id, NopCustomerDefaults.StateProvinceIdAttribute);
                }

                _messageTokenProvider.AddCampaignTokens(tokens, campaign, languageId);

                //retrieve localized message template data
                var title = _localizationService.GetLocalized(messageTemplate, mt => mt.Title, languageId);
                var message = _localizationService.GetLocalized(messageTemplate, mt => mt.Message, languageId);

                //Replace subject and body tokens 
                var titleReplaced = _tokenizer.Replace(title, tokens, false);
                var messageReplaced = _tokenizer.Replace(message, tokens, false);

                var registerationId = dInfo.RegisterationId;
                this.QueuePushNotification(titleReplaced, messageReplaced, registerationId);
            }
        }

        private void QueuePushNotification(string title, string message, string registerationId = "", string extraData = "")
        {
            var pushNotification = new QueuedPushNotification
            {
                CreatedOnUtc = DateTime.UtcNow,
                Message = message,
                Title = title,
                PushNotificationUserStatus = PushNotificationUserStatus.NoYetRead,
                Priority = QueuedPushNotificationPriority.High,
                ResponseLog = string.Empty
            };
            if (!string.IsNullOrEmpty(registerationId))
                pushNotification.RegisterationId = registerationId;

            if (!string.IsNullOrEmpty(extraData))
                pushNotification.ExtraData = extraData;

            this.InsertPushNotificationQueue(pushNotification);

        }

        public virtual void DeleteQueuedPushNotification(QueuedPushNotification queuedPushNotification)
        {
            if (queuedPushNotification == null)
                throw new ArgumentNullException("queuedPushNotification");

            _queuedPushNotificationRepository.Delete(queuedPushNotification);
        }

        public virtual void DeleteAllPushNotifications()
        {
            if (_commonSettings.UseStoredProceduresIfSupported && _nopDataProvider.StoredProceduredSupported)
            {
                //although it's not a stored procedure we use it to ensure that a database supports them


                //do all databases support "Truncate command"?
                string queuedSmsTableName = NameCompatibilityManager.GetTableName(typeof(QueuedPushNotification));     // _dbContext.GetTableName<QueuedPushNotification>();
                _nopDataProvider.ExecuteNonQuery(String.Format("TRUNCATE TABLE [{0}]", queuedSmsTableName));
            }
            else
            {
                var queuedSmss = _queuedPushNotificationRepository.Table.ToList();
                foreach (var qe in queuedSmss)
                    _queuedPushNotificationRepository.Delete(qe);
            }
        }

        public virtual void DeleteQueuedPushNotification(IList<QueuedPushNotification> queuedPushNotifications)
        {
            if (queuedPushNotifications == null)
                throw new ArgumentNullException("queuedPushNotifications");

            _queuedPushNotificationRepository.Delete(queuedPushNotifications);
        }

        public virtual QueuedPushNotification GetQueuedPushNotificationById(int queuedPushNotificationId)
        {
            if (queuedPushNotificationId == 0)
                return null;

            return _queuedPushNotificationRepository.GetById(queuedPushNotificationId);

        }

        public virtual IList<QueuedPushNotification> GetQueuedPushNotificationByIds(int[] queuedPushNotificationIds)
        {
            if (queuedPushNotificationIds == null || queuedPushNotificationIds.Length == 0)
                return new List<QueuedPushNotification>();

            var query = from qe in _queuedPushNotificationRepository.Table
                        where queuedPushNotificationIds.Contains(qe.Id)
                        select qe;

            var queuedPushNotifications = query.ToList();
            //sort by passed identifiers
            var sortedQueuedPushNotifications = new List<QueuedPushNotification>();
            foreach (int id in queuedPushNotificationIds)
            {
                var queuedPushNotification = queuedPushNotifications.Find(x => x.Id == id);
                if (queuedPushNotification != null)
                    sortedQueuedPushNotifications.Add(queuedPushNotification);
            }
            return sortedQueuedPushNotifications;
        }

        public virtual void InsertPushNotificationQueue(QueuedPushNotification pushNotificationQueue)
        {
            if (pushNotificationQueue == null)
                throw new ArgumentNullException(nameof(pushNotificationQueue));

            _queuedPushNotificationRepository.Insert(pushNotificationQueue);
        }

        public virtual void UpdatePushNotificationQueue(QueuedPushNotification pushNotificationQueue)
        {
            if (pushNotificationQueue == null)
                throw new ArgumentNullException(nameof(pushNotificationQueue));

            _queuedPushNotificationRepository.Update(pushNotificationQueue);
        }

        public void QueuePushNotificationData(string title, string message, string registerationId = "", string email = "", string extraData = "",Customer customer=null)
        {

            customer = customer==null? _customerService.GetCustomerByEmail(email):customer;
            if (customer != null)
            {
                var devicesInfo = _deviceInfoService.GetAllDeviceInfo(customerId: customer.Id);
                if (devicesInfo.Count() > 0)
                    foreach (var dInfo in devicesInfo)
                    {
                        if (string.IsNullOrEmpty(dInfo.RegisterationId))
                            continue;
                        if (string.IsNullOrEmpty(dInfo.Platform))
                            continue;

                        var pushNotification = new QueuedPushNotification
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            Message = message,
                            Title = title,
                            PushNotificationUserStatus = PushNotificationUserStatus.NoYetRead,
                            Priority = QueuedPushNotificationPriority.High,
                            ResponseLog = string.Empty,
                            CustomerId = customer.Id
                        };
                        if (!string.IsNullOrEmpty(dInfo.RegisterationId))
                            pushNotification.RegisterationId = dInfo.RegisterationId;

                        if (!string.IsNullOrEmpty(extraData))
                            pushNotification.ExtraData = extraData;

                        this.InsertPushNotificationQueue(pushNotification);
                    }
            }
        }
    }
}
