using FluentValidation;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;
using Nop.Web.Models.Vendors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Validators.VendorReview
{
    public class VendorReviewsValidator : BaseNopValidator<VendorReviewsModel>
    {
        public VendorReviewsValidator(ILocalizationService localizationService)
        {
            this.RuleFor(x => x.AddVendorReview.Title).NotEmpty()
                .WithMessage(localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Reviews.Fields.Title.Required"))
                .When(x => x.AddVendorReview != null);

            this.RuleFor(x => x.AddVendorReview.Title)
                .Length(1, 200)
                .WithMessage(string.Format(localizationService.GetResource("Nop.Plugin.NopWarehouse.VendorReview.Reviews.Fields.Title.MaxLengthValidation"), 200))
                .When(x => x.AddVendorReview != null && !string.IsNullOrEmpty(x.AddVendorReview.Title));

            this.RuleFor(x => x.AddVendorReview.ReviewText)
                .NotEmpty()
                .WithMessage(localizationService
                .GetResource("Nop.Plugin.NopWarehouse.VendorReview.Reviews.Fields.ReviewText.Required"))
                .When(x => x.AddVendorReview != null);
        }
    }
}
