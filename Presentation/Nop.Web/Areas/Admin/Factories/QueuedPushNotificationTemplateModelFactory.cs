using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Areas.Admin.Models.Notification;
using Nop.Web.Areas.Admin.Models.PushNotification;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public class QueuedQueuedPushNotificationModelFactory : IQueuedQueuedPushNotificationModelFactory
    {
        #region Fields
        private readonly ILocalizedEntityService _localizedEntityService;
        private ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IQueuedPushNotificationService _queuedPushNotificationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private IWorkContext _workContext;
        #endregion
        #region Ctr
        public QueuedQueuedPushNotificationModelFactory(IQueuedPushNotificationService queuedPushNotificationService, ILocalizedEntityService localizedEntityService,
             ILocalizationService localizationService, ILocalizedModelFactory localizedModelFactory , IWorkContext workContext, IDateTimeHelper dateTimeHelper)
        {
            _localizedEntityService = localizedEntityService;
            _queuedPushNotificationService = queuedPushNotificationService;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _workContext = workContext;
            _dateTimeHelper = dateTimeHelper;
        }

        public QueuedPushNotificationSearchModel PrepareQueuedPushNotificationSearchModel(QueuedPushNotificationSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.SearchMaxSentTries = 10;
            return searchModel;
        }
        public QueuedPushNotificationModel PrepareQueuedPushNotificationModel(QueuedPushNotificationModel model, QueuedPushNotification QueuedPushNotification, bool excludeProperties = false)
        {
            //Action<QueuedPushNotificationLocalizedModel, int> localizedModelConfiguration = null;
            //if (QueuedPushNotification != null)
            //{
            //    //fill in model values from the entity
            //    model ??= QueuedPushNotification.ToModel<QueuedPushNotificationModel>();
            //    //define localized model configuration action
            //    localizedModelConfiguration = (locale, languageId) =>
            //    {
            //        locale.Message = _localizationService.GetLocalized(QueuedPushNotification, entity => entity.Message, languageId, false, false);
            //        locale.Title = _localizationService.GetLocalized(QueuedPushNotification, entity => entity.Title, languageId, false, false);
            //    };
            //}
            ////prepare localized models
            //if (!excludeProperties)
            //    model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);
            return model;
        }
        public QueuedPushNotificationModel PrepareEditQueuedPushNotificationListModel(QueuedPushNotification smsmodel)
        {
            if (smsmodel == null)
                //No message template found with the specified id
                return null;
            var queuedPushNotificationModel = smsmodel.ToModel<QueuedPushNotificationModel>();
            queuedPushNotificationModel.PriorityName = _localizationService.GetLocalizedEnum(smsmodel.Priority, _workContext.WorkingLanguage.Id);
            queuedPushNotificationModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(smsmodel.CreatedOnUtc, DateTimeKind.Utc);
            queuedPushNotificationModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(smsmodel.CreatedOnUtc, DateTimeKind.Utc);
            if (smsmodel.SentOnUtc.HasValue)
                queuedPushNotificationModel.SentOn = _dateTimeHelper.ConvertToUserTime(smsmodel.SentOnUtc.Value, DateTimeKind.Utc);
            return queuedPushNotificationModel;

        }

        public QueuedPushNotificationListModel PrepareQueuedPushNotificationListModel(QueuedPushNotificationSearchModel searchModel, QueuedPushNotificationListModel QueuedPushNotificationListModel=null)
        {
            var QueuedPushNotifications = _queuedPushNotificationService.SearchPushNotifications(false, searchModel.SearchMaxSentTries).ToPagedList(searchModel);
            var model = new QueuedPushNotificationListModel().PrepareToGrid(searchModel, QueuedPushNotifications, () =>
            {
                return QueuedPushNotifications.Select(point =>
                {
                    //fill in model values from the entity
                    var queuedPushNotificationModel = point.ToModel<QueuedPushNotificationModel>();
                    queuedPushNotificationModel.PriorityName = _localizationService.GetLocalizedEnum(point.Priority, _workContext.WorkingLanguage.Id);
                    queuedPushNotificationModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(point.CreatedOnUtc, DateTimeKind.Utc);
                    if (point.SentOnUtc.HasValue)
                        queuedPushNotificationModel.SentOn = _dateTimeHelper.ConvertToUserTime(point.SentOnUtc.Value, DateTimeKind.Utc);
                    return queuedPushNotificationModel;
                });
            });
           
            return model;
        }
        #endregion
        public void UpdateLocales(QueuedPushNotification domainModel, QueuedPushNotificationModel model)
        {
            //foreach (var localized in model.Locales)
            //{
            //    _localizedEntityService.SaveLocalizedValue(domainModel,
            //                                               x => x.Title,
            //                                               localized.Title,
            //                                               localized.LanguageId);

            //    _localizedEntityService.SaveLocalizedValue(domainModel,
            //                                               x => x.Message,
            //                                               localized.Message,
            //                                               localized.LanguageId);
            //}
        }
    }
}
