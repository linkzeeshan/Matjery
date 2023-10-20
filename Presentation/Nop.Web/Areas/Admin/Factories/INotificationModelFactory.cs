using Nop.Core.Domain.Notifications;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Areas.Admin.Models.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public interface INotificationModelFactory
    {
        void UpdateLocales(Notification domainModel, NotificationModel model);
        NotificationListModel PrepareNotificationListModel(NotificationSearchModel searchModel, NotificationListModel notificationListModel);
        NotificationListModel PrepareNotificationListModel();
       NotificationModel PrepareNotificationModel(NotificationModel model, Notification pushNotificationTemplate, bool excludeProperties = false);
        NotificationSearchModel PrepareNotificationSearchModel(NotificationSearchModel searchModel);
        string GetNotificationTypeValues(int typeId);
        NotificationModel PrepareCreateNotification(int Id);
        NotificationModel PrepareCreateNotification(NotificationModel model, bool continueEditing);
        int QueuePushNotification(string title, string message, string registerationId = "", string extraData = "");
    }
}
