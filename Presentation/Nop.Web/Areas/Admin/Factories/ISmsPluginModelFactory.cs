using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Sms;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Areas.Admin.Models.Notification;
using Nop.Web.Areas.Admin.Models.PushNotification;
using Nop.Web.Areas.Admin.Models.Sms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public interface ISmsPluginModelFactory
    {
        void UpdateLocales(SmsTemplate domainModel, SmsTemplateModel model);
        SmsTemplateListModel PrepareSmsPluginListModel(SmsTemplateSearchModel searchModel);
        SmsTemplateModel PrepareSmsTemplateModel(SmsTemplateModel model, SmsTemplate SmsTemplate, bool excludeProperties = false);
        SmsTemplateSearchModel PrepareSmsTemplateSearchModel(SmsTemplateSearchModel searchModel);
        SmsTemplateModel PrepareEditSmsTemplateModel(int id);
    }
}
