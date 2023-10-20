using FluentValidation;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Framework.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Validators.Messages
{
    public class PushNotificationTemplateValidator : BaseNopValidator<PushNotificationTemplateModel>
    {
        public PushNotificationTemplateValidator(ILocalizationService localizationService, INopDataProvider dbContext)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.PushNotificationTemplate.Fields.Name.Required"));
            RuleFor(x => x.Title).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.PushNotificationTemplate.Fields.Title.Required"));
            RuleFor(x => x.Message).NotEmpty().WithMessage(localizationService.GetResource("Admin.ContentManagement.PushNotificationTemplate.Fields.Message.Required"));
           // SetDatabaseValidationRules<Campaign>(dataProvider);
        }
    }
}
