using FluentValidation;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Validators.Catalog
{
    public partial class ProductValidator : BaseNopValidator<ProductModel>
    {
        public ProductValidator(ILocalizationService localizationService, INopDataProvider dataProvider)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.Name.Required"));
            RuleFor(x => x.FullDescription).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.FullDescription.Required"));
            RuleFor(x => x.Price).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.Price.Required"));
            RuleFor(x => x.SelectedCategoryIds).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.Categories.Required"));
            var entityDescriptor = dataProvider.GetEntityDescriptor<Product>();
            SetStringPropertiesMaxLength(entityDescriptor);
        }
    }
}
