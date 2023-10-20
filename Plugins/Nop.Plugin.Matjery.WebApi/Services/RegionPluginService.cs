using Nop.Core.Domain.Directory;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class RegionPluginService : BasePluginService, IRegionPluginService
    {
        public List<CountryResult> GetCountryList()
        {
            var countryResultList = new List<CountryResult>();

            var allCountries = this._countryService.GetAllCountries(this._workContext.WorkingLanguage.Id);
            foreach (Country country in allCountries)
            {
                var countryResult = new CountryResult
                {
                    Id = country.Id,
                    Name = _localizationService.GetLocalized(country,x => x.Name)
                };
                foreach (var language in _languageService.GetAllLanguages())
                {
                    countryResult.CustomProperties.Add(language.UniqueSeoCode,_localizationService.GetLocalized(country,x => x.Name, language.Id));
                }
                countryResultList.Add(countryResult);
            }
            return countryResultList;
        }

        public List<StateProvinceResult> GetStateProvinceList(int countryId)
        {
            var stateProviceResultList = new List<StateProvinceResult>();
            IList<StateProvince> stateProvincesBySpecifiedCountry = this._stateProvinceService.GetStateProvincesByCountryId(countryId, _workContext.WorkingLanguage.Id);
            foreach (StateProvince province in stateProvincesBySpecifiedCountry)
            {
                var stateProvice = new StateProvinceResult
                {
                    Id = province.Id,
                    Name = _localizationService.GetLocalized(province,x => x.Name)
                };
                foreach (var language in _languageService.GetAllLanguages())
                {
                    var localeModel = new
                    {
                        Name = _localizationService.GetLocalized(province,x => x.Name, language.Id),
                        Id = province.Id
                    };
                    stateProvice.CustomProperties.Add(language.UniqueSeoCode, localeModel);
                }
                stateProviceResultList.Add(stateProvice);
            }
            return stateProviceResultList;
        }
    }
}
