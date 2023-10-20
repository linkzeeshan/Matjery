using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Vendors;
using System.Collections.Generic;

namespace Nop.Web.Validators.Vendors
{
    public partial class ApplyVendorValidator : BaseNopValidator<ApplyVendorModel>
    {
        public ApplyVendorValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Vendors.ApplyAccount.Name.Required"));

            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Vendors.ApplyAccount.Email.Required"));
            RuleFor(x => x.PhoneNumber).NotEmpty().WithMessage(localizationService.GetResource("Vendors.ApplyAccount.PhoneNumber.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(m => m.CategoryId)
           .NotNull().WithMessage(localizationService.GetResource("account.vendorinfo.category.required"))
           .NotEmpty().WithMessage(localizationService.GetResource("account.vendorinfo.category.required"))
           .Must(BeValidCategoryId).WithMessage(localizationService.GetResource("account.vendorinfo.category.required"));
        }
        
        private bool BeValidCategoryId(IEnumerable<int> categoryIds)
        {
            if (categoryIds != null)
            {
                foreach (int categoryId in categoryIds)
                {
                    // Implement your custom validation logic here.
                    // This example just checks if the category Id is a non-empty string.
                    if (categoryId == null)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }


            return true;
        }
    }
}