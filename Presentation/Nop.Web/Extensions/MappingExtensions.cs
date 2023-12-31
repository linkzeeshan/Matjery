﻿using Nop.Core.Domain.Common;
using Nop.Core.Domain.Messages;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Common;

namespace Nop.Web.Extensions
{
    public static class MappingExtensions
    {//category

       
        public static CampaignModel ToModel(this Campaign entity)
        {
            var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
            if (entity == null)
                return null;

            var model = new CampaignModel
            {
                Id = entity.Id,
                Name = localizationService.GetLocalized(entity,e => e.Name),
                Body = localizationService.GetLocalized(entity,e => e.Body),
                DisplayAreaId = entity.DisplayAreaId,
                MinDiscountPercentage = entity.MinDiscountPercentage,
                Subject = localizationService.GetLocalized(entity,e => e.Subject),
                PictureId = localizationService.GetLocalized(entity,e => e.PictureId),
                PictureIdMobile = localizationService.GetLocalized(entity,e => e.PictureIdMobile),
                Active = entity.Active
            };
            return model;
        }
        //address
        public static Address ToEntity(this AddressModel model, bool trimFields = true)
        {
            if (model == null)
                return null;

            var entity = new Address();
            return ToEntity(model, entity, trimFields);
        }

        public static Address ToEntity(this AddressModel model, Address destination, bool trimFields = true)
        {
            if (model == null)
                return destination;

            if (trimFields)
            {
                if (model.FirstName != null)
                    model.FirstName = model.FirstName.Trim();
                if (model.LastName != null)
                    model.LastName = model.LastName.Trim();
                if (model.Email != null)
                    model.Email = model.Email.Trim();
                if (model.Company != null)
                    model.Company = model.Company.Trim();
                if (model.County != null)
                    model.County = model.County.Trim();
                if (model.City != null)
                    model.City = model.City.Trim();
                if (model.Area != null)
                    model.Area = model.Area.Trim();
                if (model.Address1 != null)
                    model.Address1 = model.Address1.Trim();
                if (model.Address2 != null)
                    model.Address2 = model.Address2.Trim();
                if (model.ZipPostalCode != null)
                    model.ZipPostalCode = model.ZipPostalCode.Trim();
                if (model.PhoneNumber != null)
                    model.PhoneNumber = model.PhoneNumber.Trim();
                if (model.FaxNumber != null)
                    model.FaxNumber = model.FaxNumber.Trim();
            }
            destination.Id = model.Id;
            destination.FirstName = model.FirstName;
            destination.LastName = model.LastName;
            destination.Email = model.Email;
            destination.Company = model.Company;
            destination.CountryId = model.CountryId;
            destination.StateProvinceId = model.StateProvinceId;
            destination.County = model.County;
            destination.City = model.City;
            destination.Area = model.Area;
            destination.Address1 = model.Address1;
            destination.Address2 = model.Address2;
            destination.ZipPostalCode = model.ZipPostalCode;
            destination.PhoneNumber = model.PhoneNumber;
            destination.FaxNumber = model.FaxNumber;

            return destination;
        }
    }
}