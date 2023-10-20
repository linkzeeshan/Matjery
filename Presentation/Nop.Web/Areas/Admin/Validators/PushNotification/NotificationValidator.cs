using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Notification;
using Nop.Web.Framework.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Validators.PushNotification
{
    public partial class NotificationValidator : BaseNopValidator<NotificationModel>
    {
        public NotificationValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.Notification.Fields.Title.Required"));
            RuleFor(x => x.Message).NotEmpty().WithMessage(localizationService.GetResource("Admin.Promotions.Notification.Fields.Note.Message"));

        }

    }
}
