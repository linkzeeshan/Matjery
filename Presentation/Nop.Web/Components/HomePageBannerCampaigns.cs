using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Messages;
using Nop.Services.Caching;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Web.Extensions;
using Nop.Web.Framework.Components;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Components
{
    public class HomePageBannerCampaignsViewComponent: NopViewComponent
    {
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ICacheKeyService _cacheKeyService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly ICampaignService _campaignService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        public HomePageBannerCampaignsViewComponent(
            IStaticCacheManager staticCacheManager,
            ICacheKeyService cacheKeyService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IWebHelper webHelper,
            ICampaignService campaignService,
            IPictureService pictureService,
            ILocalizationService localizationService
            )
        {
            _staticCacheManager = staticCacheManager;
            _cacheKeyService = cacheKeyService;
            _storeContext = storeContext;
            _workContext = workContext;
            _campaignService = campaignService;
            _webHelper = webHelper;
            _pictureService = pictureService;
            _localizationService = localizationService;

        }
        public IViewComponentResult Invoke()
        {
            var categoriesCacheKey = _cacheKeyService.PrepareKeyForDefaultCache(NopModelCacheDefaults.CampaignHomePageKey,
            CampaignDisplayAreaType.Banner,
                _storeContext.CurrentStore.Id,
                _workContext.WorkingLanguage.Id,
                _webHelper.IsCurrentConnectionSecured());

            var model = _staticCacheManager.Get(categoriesCacheKey, () =>
                _campaignService.GetAllCampaigns(displayAreaId: (int)CampaignDisplayAreaType.Banner
                 , showHidden: false)
                .Select(x =>
                {
                    var catModel = x.ToModel();

                    //prepare picture model

                    var categoryPictureCacheKey = _cacheKeyService.PrepareKeyForDefaultCache(NopModelCacheDefaults.CampaignHomePageKey,
              x.Id, true, _workContext.WorkingLanguage.Id,
                        _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);


                    catModel.PictureModel = _staticCacheManager.Get(categoryPictureCacheKey, () =>
                    {
                        var picture = _pictureService.GetPictureById(Convert.ToInt32(catModel.PictureId));
                        var pictureModel = new PictureModel
                        {
                            FullSizeImageUrl = _pictureService.GetPictureUrl(ref picture),
                            ImageUrl = _pictureService.GetPictureUrl(ref picture),
                            Title = string.Format(_localizationService.GetResource("Media.Category.ImageLinkTitleFormat"), catModel.Name),
                            AlternateText = string.Format(_localizationService.GetResource("Media.Category.ImageAlternateTextFormat"), catModel.Name)
                        };
                        return pictureModel;
                    });

                    return catModel;
                })
                .ToList()
            );

            if (!model.Any())
                return Content("");

            return View(model);
        }
    }
}
