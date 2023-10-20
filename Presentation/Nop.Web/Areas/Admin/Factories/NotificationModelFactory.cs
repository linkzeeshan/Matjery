using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Notifications;
using Nop.Services;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Areas.Admin.Models.Notification;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public class NotificationModelFactory : INotificationModelFactory
    {
        #region Fields
        private readonly ILocalizedEntityService _localizedEntityService;
        private ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly Nop.Services.Notifications.INotificationService _notificationService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ILanguageService _languageService;
        private readonly IDeviceInfoService _deviceInfoService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IQueuedPushNotificationService _queuedPushNotificationService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IStoreContext _storeContext;
        private IWorkContext _workContext;
        #endregion
        #region Ctr
        public NotificationModelFactory(Nop.Services.Notifications.INotificationService notificationService, ILocalizedEntityService localizedEntityService,
             ILocalizationService localizationService, ILocalizedModelFactory localizedModelFactory,
             IWorkflowMessageService workflowMessageService, IStoreContext storeContext, ILanguageService languageService,
             IDeviceInfoService deviceInfoService, ICustomerService customerService, IGenericAttributeService genericAttributeService,
             IQueuedPushNotificationService queuedPushNotificationService, INewsLetterSubscriptionService newsLetterSubscriptionService,
             IEmailAccountService emailAccountService,  EmailAccountSettings emailAccountSettings, IDateTimeHelper dateTimeHelper, IWorkContext workContext)
        {
            _localizedEntityService = localizedEntityService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _workflowMessageService = workflowMessageService;
            _customerService = customerService;
            _storeContext = storeContext;
            _languageService = languageService;
            _deviceInfoService = deviceInfoService;
            _queuedPushNotificationService = queuedPushNotificationService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _genericAttributeService = genericAttributeService;
            _emailAccountService = emailAccountService;
            _emailAccountSettings = emailAccountSettings;
            _dateTimeHelper = dateTimeHelper;
            _workContext = workContext;
        }
        #endregion
        public NotificationListModel PrepareNotificationListModel(NotificationSearchModel searchModel, NotificationListModel notificationListModel)
        {
            //prepare page parameters

             var notifications = _notificationService.Search(type: 0,languageId: _workContext.WorkingLanguage.Id).ToPagedList(searchModel);

            var model =  new NotificationListModel().PrepareToGrid(searchModel, notifications, () =>
            {
                return notifications.Select(point =>
                {
                    //fill in model values from the entity
                    var notificationModel = point.ToModel<NotificationModel>();
                    notificationModel.Title = _localizationService.GetLocalized(point, c => c.Title, _workContext.WorkingLanguage.Id);
                    notificationModel.Type = this.GetNotificationTypeValues(point.Type);
                    notificationModel.CreatedOnUtc = _dateTimeHelper.ConvertToUserTime(point.CreatedOnUtc, DateTimeKind.Utc);

                    return notificationModel;
                });
                
            });
           

            return model;
        }
        public NotificationListModel PrepareNotificationListModel()
        {
           
            return new NotificationListModel
            {
                AvailableTypes = NotificationTypeEnum.Notification.ToSelectListInServices(false).ToList(),
                IsAdmin = _customerService.IsAdmin(_workContext.CurrentCustomer),
            };
        }
        public NotificationModel PrepareNotificationModel(NotificationModel model, Notification notification, bool excludeProperties = false)
        {
            throw new NotImplementedException();
        }

        public NotificationSearchModel PrepareNotificationSearchModel(NotificationSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel = new NotificationSearchModel {
                AvailableTypes = NotificationTypeEnum.Notification.ToSelectListInServices(false).ToList(),
                IsAdmin = _customerService.IsAdmin(_workContext.CurrentCustomer),
            };
            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public void UpdateLocales(Notification domainModel, NotificationModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(domainModel,
                                                           x => x.Title,
                                                           localized.Title,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(domainModel,
                                                           x => x.Message,
                                                           localized.Message,
                                                           localized.LanguageId);
            }
        }

        public string GetNotificationTypeValues(int typeId)
        {
            string value = string.Empty;
            if (typeId == 0)
                return string.Empty;

            if (typeId == (int)NotificationTypeEnum.Notification)
                value = _localizationService.GetResource("enums.Nop.Core.Domain.Notifications.NotificationTypeEnum.Notification");

            if (typeId == (int)NotificationTypeEnum.Email)
                value = _localizationService.GetResource("enums.Nop.Core.Domain.Notifications.NotificationTypeEnum.Email");

            return value;
        }

        public NotificationModel PrepareCreateNotification(int typeId)
        {
            var model = new NotificationModel();
            Action<NotificationLocalizedModel, int> localizedModelConfiguration = null;
            if (typeId > 0)
                model.TypeId = typeId;
            model.TypeName = GetNotificationTypeValues(typeId);

            if (typeId == (int)NotificationTypeEnum.Email)
            {
                var messageTemplate = _workflowMessageService.GetNotificationEmailTemplate(_storeContext.CurrentStore.Id, _workContext.WorkingLanguage.Id);
                //locales
                model.Message = _localizationService.GetLocalized(messageTemplate, x => x.Body, languageId: 2);
                model.Message = model.Message.Replace("%Store.URL%", _storeContext.CurrentStore.Url);
                model.Message = model.Message.Replace("%Store.Name%", _localizationService.GetLocalized(_storeContext.CurrentStore, x => x.Name, languageId: 2));

                localizedModelConfiguration = (locale, languageId) =>
                {
                       locale.Message = _localizationService.GetLocalized(messageTemplate, x => x.Body, languageId, false, true);
                    if (!string.IsNullOrEmpty(locale.Message))
                        locale.Message = locale.Message.Replace("%Store.Name%", _localizationService.GetLocalized(_storeContext.CurrentStore, x => x.Name));
                };
                model.Locales = model.Locales.Count == 0 ? _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration) : model.Locales;
            }
            else
            {
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);
            }
            return model;
        }

        public NotificationModel PrepareCreateNotification(NotificationModel model, bool continueEditing)
        {
            int maxId = 0;
            //we will discuss

            string Message = string.Empty;

            var notification = new Notification
            {
                Title = model.Title,
                Message = model.Message,
                Type = model.TypeId,
                CreatedOnUtc = DateTime.UtcNow
            };
            _notificationService.Insert(notification);
            //locales
            UpdateLocales(notification, model);
            //send notification to these all devices
            if (model.TypeId == (int)NotificationTypeEnum.Notification)
            {
                var devicesInfo = _deviceInfoService.GetAllDeviceInfo();

                if (devicesInfo.Count > 0)
                {
                    foreach (var dInfo in devicesInfo)
                    {
                        if (string.IsNullOrEmpty(dInfo.RegisterationId))
                            continue;

                        var customer = _customerService.GetCustomerById(dInfo.CustomerId);
                        if (customer == null)
                            continue;

                        int langId = 2; //arabic default
                        var custLanguageId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.LanguageIdAttribute, _storeContext.CurrentStore.Id); //customer.GetAttribute<int>(SystemCustomerAttributeNames.LanguageId, _storeContext.CurrentStore.Id);
                        if (custLanguageId > 0)
                            langId = custLanguageId;

                        //foreach (var localized in model.Locales)
                        //{
                        //    if (localized.LanguageId == langId) //this should be always en
                        //    {
                        //        var registerationId = dInfo.RegisterationId;
                        //        maxId = this.QueuePushNotification(localized.Title, localized.Message, registerationId);
                        //    }
                        //    else // this should be always ae
                        //    {
                        //        var registerationId = dInfo.RegisterationId;
                        //        maxId = this.QueuePushNotification(model.Title, model.Message, registerationId);
                        //    }
                        //}
                    }
                }
            }
            //email notification to users
            else if (model.TypeId == (int)NotificationTypeEnum.Email)
            {
                var subscription = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(isActive: true);

                if (subscription.Count > 0)
                {
                    var emailAccount = _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId);
                    if (emailAccount == null)
                        throw new NopException("Email account could not be loaded");
                    _notificationService.SendEmailNotification(notification, emailAccount, subscription);
                }
            }

            return model;
        }
        public int QueuePushNotification(string title, string message, string registerationId = "", string extraData = "")
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

           _queuedPushNotificationService.InsertPushNotificationQueue(pushNotification);
            return pushNotification.Id;
        }

        
    }

        
    }
