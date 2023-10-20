using FluentValidation;
using Nop.Core.Domain.Foundations;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Foundation;
using Nop.Web.Framework.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Validators.Foundations
{
    public partial class FoundationValidator : BaseNopValidator<FoundationModel>
    {
        public FoundationValidator(ILocalizationService localizationService, INopDataProvider dbContext)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Foundations.Fields.Name.Required"));
            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Admin.Foundations.Fields.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Admin.Common.WrongEmail"));
            //SetDatabaseValidationRules<Vendor>(dataProvider);
            SetDatabaseValidationRules<Foundation>(dbContext);
        }
    }
}
