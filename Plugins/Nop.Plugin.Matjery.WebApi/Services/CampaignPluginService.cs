using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class CampaignPluginService : BasePluginService, ICampaignPluginService
    {
        public List<CampaignResult> GetAllCampaigns()
        {
            var campaignResultList = new List<CampaignResult>();

            IList<Campaign> campaigns = _campaignService.GetAllCampaigns(showHidden: false);
            foreach (Campaign campaign in campaigns)
            {
                var model = new CampaignResult
                {
                    Id = campaign.Id,
                    Name = campaign.Name,
                    Body = campaign.Body,
                    DisplayAreaId = campaign.DisplayAreaId,
                    MinDiscountPercentage = campaign.MinDiscountPercentage,
                    Subject = campaign.Subject,
                    Active = campaign.Active
                };
                foreach (Language language in _languageService.GetAllLanguages())
                {
                    var subModel = new CampaignResult
                    {
                        Name =_localizationService.GetLocalized(campaign,x => x.Name, language.Id),
                        Subject = _localizationService.GetLocalized(campaign,x => x.Subject, language.Id),
                        Body = _localizationService.GetLocalized(campaign,x => x.Body, language.Id),
                    };
                    if (!string.IsNullOrEmpty(campaign.PictureId))
                    {
                        int localizedPictureId = Int32.Parse(_localizationService.GetLocalized(campaign,c => c.PictureId, language.Id));
                        if (localizedPictureId > 0)
                        {
                            var picture = _pictureService.GetPictureById(localizedPictureId);
                            subModel.PictureUrl = _pictureService.GetPictureUrl(picture.Id);
                        }
                    }
                    if (!string.IsNullOrEmpty(campaign.PictureIdMobile))
                    {
                        int localizedPictureMobileId = Int32.Parse(_localizationService.GetLocalized(campaign,c => c.PictureIdMobile, language.Id));
                        if (localizedPictureMobileId > 0)
                        {
                            var picture = _pictureService.GetPictureById(localizedPictureMobileId);
                            subModel.PictureMobileUrl = _pictureService.GetPictureUrl(picture.Id);
                        }
                    }
                    model.CustomProperties.Add(language.UniqueSeoCode, subModel);
                }
                campaignResultList.Add(model);
            }
            return campaignResultList;
        }
    }
}
