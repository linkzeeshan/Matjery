using Nop.Core.Domain.Messages;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Areas.Admin.Models.Notification;
using Nop.Web.Areas.Admin.Models.PushNotification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public interface IPushNotificationTemplateModelFactory
    {
        void UpdateLocales(PushNotificationTemplate domainModel, PushNotificationTemplateModel model);
        PushNotificationTemplateListModel PreparePushNotificationTemplateListModel(PushNotificationSearchModel searchModel, PushNotificationTemplateListModel pushNotificationTemplateListModel);
        PushNotificationTemplateModel PreparePushNotificationTemplateModel(PushNotificationTemplateModel model, PushNotificationTemplate pushNotificationTemplate, bool excludeProperties = false);
        PushNotificationSearchModel PreparePushNotificationSearchModel(PushNotificationSearchModel searchModel);
        PushNotificationTemplateModel PrepareEditPushNotificationTemplateListModel(int Id);
    }
}
