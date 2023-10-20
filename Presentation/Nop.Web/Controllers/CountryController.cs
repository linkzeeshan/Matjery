using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Constant;
using Nop.Core.Domain.Directory;
using Nop.Services.Constants;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Web.Controllers
{
    public partial class CountryController : BasePublicController
    {
        #region Fields

        private readonly ICountryModelFactory _countryModelFactory;
        private readonly IConstantService _constantService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        #endregion

        #region Ctor

        public CountryController(ICountryModelFactory countryModelFactory, IStateProvinceService stateProvinceService,
            ILocalizationService localizationService, IConstantService constantService, IWorkContext workContext)
        {
            _countryModelFactory = countryModelFactory;
            _constantService = constantService;
            this._stateProvinceService = stateProvinceService;
            this._localizationService = localizationService;
            _workContext = workContext;
        }


        #endregion

        #region States / provinces

        //available even when navigation is not allowed
        [CheckAccessPublicStore(true)]
        public virtual IActionResult GetStatesByCountryId(string countryId, bool addSelectStateItem)
        {
            var model = _countryModelFactory.GetStatesByCountryId(countryId, addSelectStateItem);
            return Json(model);
        }
        //[PublicStoreAllowNavigation(true)]
        //[AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetCitiesByStateId(string stateId)
        {
            //this action method gets called via an ajax request
            if (String.IsNullOrEmpty(stateId))
                throw new ArgumentNullException(nameof(stateId));

            StateProvince state = _stateProvinceService.GetStateProvinceById(Convert.ToInt32(stateId));
            IEnumerable<Constant> cities = _constantService.GetAllConstants().Where(c =>
                c.GroupName == "CI" && c.EmirateId == (state != null ? state.Id : 0).ToString());

            var result = (from city in cities
                          select new
                          {
                              id = city.Code,
                              name = _workContext.WorkingLanguage.Rtl ? city.ArabicName : city.EnglishName
                          })
                          .ToList();

            return Json(result);
        }

        //[PublicStoreAllowNavigation(true)]
        //[AcceptVerbs(HttpVerbs.Get)]
        public ActionResult GetAreasByStateId(string stateId)
        {
            //this action method gets called via an ajax request
            if (String.IsNullOrEmpty(stateId))
                throw new ArgumentNullException(nameof(stateId));

            StateProvince state = _stateProvinceService.GetStateProvinceById(Convert.ToInt32(stateId));
            IEnumerable<Constant> areas = _constantService.GetAllConstants().Where(c =>
                c.GroupName == "AR" && c.EmirateId == (state != null ? state.Id : 0).ToString());

            //var areas = new List<Constant>();
            //foreach (var city in cities)
            //{
            //    areas.AddRange(_constantService.GetAllConstants().Where(a =>
            //        a.GroupName == "AR" && a.CityId == city.Code));
            //}

            var result = (from area in areas
                          select new
                          {
                              id = area.Code,
                              name = _workContext.WorkingLanguage.Rtl ? area.ArabicName : area.EnglishName
                          })
                          .ToList();

            return Json(result);
        }
        #endregion
    }
}