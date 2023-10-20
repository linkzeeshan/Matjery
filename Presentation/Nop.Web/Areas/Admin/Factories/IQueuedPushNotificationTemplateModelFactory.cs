using Nop.Core.Domain.Messages;
using Nop.Web.Areas.Admin.Models.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public interface IQueuedQueuedPushNotificationModelFactory
    {
        void UpdateLocales(QueuedPushNotification domainModel, QueuedPushNotificationModel model);
        QueuedPushNotificationListModel PrepareQueuedPushNotificationListModel(QueuedPushNotificationSearchModel searchModel, QueuedPushNotificationListModel QueuedPushNotificationListModel);
        QueuedPushNotificationModel PrepareQueuedPushNotificationModel(QueuedPushNotificationModel model, QueuedPushNotification pushNotificationTemplate, bool excludeProperties = false);
        QueuedPushNotificationSearchModel PrepareQueuedPushNotificationSearchModel(QueuedPushNotificationSearchModel searchModel);
        QueuedPushNotificationModel PrepareEditQueuedPushNotificationListModel(QueuedPushNotification model);
    }
}
