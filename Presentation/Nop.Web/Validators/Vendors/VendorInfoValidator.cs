    using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Vendors;
using System;
using System.Collections.Generic;

namespace Nop.Web.Validators.Vendors
{
    public partial class VendorInfoValidator : BaseNopValidator<VendorInfoModel>
    {
        public VendorInfoValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Account.VendorInfo.Name.Required"));

            RuleFor(x => x.Email).NotEmpty().WithMessage(localizationService.GetResource("Account.VendorInfo.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessage(localizationService.GetResource("Common.WrongEmail"));
            RuleFor(m => m.CategoryId)
               .NotNull().WithMessage(localizationService.GetResource("account.vendorinfo.category.required"))
               .NotEmpty().WithMessage(localizationService.GetResource("account.vendorinfo.category.required"))
               .Must(BeValidCategoryId).WithMessage(localizationService.GetResource("account.vendorinfo.category.required"));

            //commented for QA
            //RuleFor(m => m.ExpiryDate)
            //    .Must(ValidateExpiryDate).WithMessage(localizationService.GetResource("account.register.errors.licenseexpiry"));
        }

        private bool ValidateExpiryDate(string ExDate)
        {
            if (ExDate != null)
            {
                DateTime expiryDate = DateTime.Parse(ExDate);
                DateTime currentDate = DateTime.Now;

                if (expiryDate < currentDate)
                {
                    return false;
                }
            }
            return true;
        }
        private bool BeValidCategoryId(IEnumerable<int> categoryIds)
        {
            if (categoryIds != null)
            {
                foreach (int categoryId in categoryIds)
                {
                    // Implement your custom validation logic here.
                    // This example just checks if the category Id is a non-empty string.
                    if (categoryId ==null)
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