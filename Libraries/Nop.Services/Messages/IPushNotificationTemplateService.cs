using Nop.Core;
using Nop.Core.Domain.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Messages
{
    public interface IPushNotificationTemplateService
    {
        void DeletePushNotificationTemplate(PushNotificationTemplate pushNotificationTemplate);
        IList<PushNotificationTemplate> GetAllPushNotificationTemplates();
        IPagedList<PushNotificationTemplate> GetAllPushNotificationTemplates(string name = "", string title = "",
           int pageIndex = 0, int pageSize = int.MaxValue, bool IsActive = false);
        PushNotificationTemplate GetPushNotificationTemplateById(int pushNotificationTemplateId);
        PushNotificationTemplate GetPushNotificationTemplateByName(string pushNotificationTemplateName);
        void InsertPushNotificationTemplate(PushNotificationTemplate pushNotificationTemplate);
        void UpdatePushNotificationTemplate(PushNotificationTemplate pushNotificationTemplate);
        PushNotificationTemplate GetActivePushNotificationTemplate(string messageTemplateName);
    }
}
