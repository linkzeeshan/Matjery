using FluentValidation;
using Nop.Core.Domain.Common;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Blackpoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Validators.BlackPoint
{
    public partial class BlackPointValidator : BaseNopValidator<BlackPointModel>
    {
        public BlackPointValidator(ILocalizationService localizationService, CommonSettings commonSettings)
        {
            RuleFor(x => x.Comment).NotEmpty().WithMessage(localizationService.GetResource("BlackPoint.Comment.Required"));
        }
    }
}
