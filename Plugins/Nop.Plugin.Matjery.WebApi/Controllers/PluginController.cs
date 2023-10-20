using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Plugin.Matjery.WebApi.Infrastructure;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Plugin.Widgets.NivoSlider;
using Nop.Plugin.Widgets.NivoSlider.Infrastructure.Cache;
using Nop.Services.Caching;
using Nop.Services.Configuration;
using Nop.Services.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class PluginController : BaseController
    {
        private readonly IStaticCacheManager _cacheManager;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly ICacheKeyService _cacheKeyService;

        public PluginController(IStaticCacheManager cacheManager, IPictureService pictureService, ISettingService settingService, ICacheKeyService cacheKeyService)
        {
            _cacheManager = cacheManager;
            _pictureService = pictureService;
            _settingService = settingService;
            _cacheKeyService = cacheKeyService;
        }

        /// <summary>
        /// To Get Top menu Category
        /// </summary>
        /// <param name="rootCategoryId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetNivoSliderSlides", Name = "GetNivoSliderSlides")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult GetNivoSliderSlides()
        {
            try
            {
                var slidesList = new List<NivoSliderResult>();
                var nivoSliderSettings = _settingService.LoadSetting<NivoSliderSettings>(_storeContext.CurrentStore.Id);

                var model = new NivoSliderResult();
                model.PictureUrl = GetPictureUrl(nivoSliderSettings.MobilePicture1Id);
                if (!string.IsNullOrEmpty(model.PictureUrl))
                {
                    model.Text = nivoSliderSettings.Text1;
                    model.Link = nivoSliderSettings.Link1;
                    slidesList.Add(model);
                }

                model = new NivoSliderResult();
                model.PictureUrl = GetPictureUrl(nivoSliderSettings.MobilePicture2Id);
                if (!string.IsNullOrEmpty(model.PictureUrl))
                {
                    model.Text = nivoSliderSettings.Text2;
                    model.Link = nivoSliderSettings.Link2;
                    slidesList.Add(model);
                }

                model = new NivoSliderResult();
                model.PictureUrl = GetPictureUrl(nivoSliderSettings.MobilePicture3Id);
                if (!string.IsNullOrEmpty(model.PictureUrl))
                {
                    model.Text = nivoSliderSettings.Text3;
                    model.Link = nivoSliderSettings.Link3;
                    slidesList.Add(model);
                }

                model = new NivoSliderResult();
                model.PictureUrl = GetPictureUrl(nivoSliderSettings.MobilePicture4Id);
                if (!string.IsNullOrEmpty(model.PictureUrl))
                {
                    model.Text = nivoSliderSettings.Text4;
                    model.Link = nivoSliderSettings.Link4;
                    slidesList.Add(model);
                }

                model = new NivoSliderResult();
                model.PictureUrl = GetPictureUrl(nivoSliderSettings.MobilePicture5Id);
                if (!string.IsNullOrEmpty(model.PictureUrl))
                {
                    model.Text = nivoSliderSettings.Text5;
                    model.Link = nivoSliderSettings.Link5;
                    slidesList.Add(model);
                }

                var result = base.Prepare(slidesList);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        protected string GetPictureUrl(int pictureId)
        {
            var cacheKey = _cacheKeyService.PrepareKeyForDefaultCache(ModelCacheEventConsumer.PICTURE_URL_MODEL_KEY, _storeContext.CurrentStore.Id, pictureId);
            return _cacheManager.Get(cacheKey, () =>
            {
                var url = _pictureService.GetPictureUrl(pictureId, showDefaultPicture: false, storeLocation: _storeContext.CurrentStore.Url);
                if (url == null)
                    url = "";
                return url;
            });
        }

    }
}
