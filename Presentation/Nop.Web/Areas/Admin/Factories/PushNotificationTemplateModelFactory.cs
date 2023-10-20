using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Areas.Admin.Models.Notification;
using Nop.Web.Areas.Admin.Models.PushNotification;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public class PushNotificationTemplateModelFactory : IPushNotificationTemplateModelFactory
    {
        #region Fields
        private readonly ILocalizedEntityService _localizedEntityService;
        private ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IPushNotificationTemplateService _pushNotificationTemplateService;
        private IWorkContext _workContext;
        #endregion
        #region Ctr
        public PushNotificationTemplateModelFactory(IPushNotificationTemplateService pushNotificationTemplateService, ILocalizedEntityService localizedEntityService,
             ILocalizationService localizationService, ILocalizedModelFactory localizedModelFactory , IWorkContext workContext)
        {
            _localizedEntityService = localizedEntityService;
            _pushNotificationTemplateService = pushNotificationTemplateService;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _workContext = workContext;
        }

        public PushNotificationSearchModel PreparePushNotificationSearchModel(PushNotificationSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }
        public PushNotificationTemplateModel PreparePushNotificationTemplateModel(PushNotificationTemplateModel model, PushNotificationTemplate pushNotificationTemplate, bool excludeProperties = false)
        {
            Action<PushNotificationTemplateLocalizedModel, int> localizedModelConfiguration = null;
            if (pushNotificationTemplate != null)
            {
                //fill in model values from the entity
                model ??= pushNotificationTemplate.ToModel<PushNotificationTemplateModel>();
                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Message = _localizationService.GetLocalized(pushNotificationTemplate, entity => entity.Message, languageId, false, false);
                    locale.Title = _localizationService.GetLocalized(pushNotificationTemplate, entity => entity.Title, languageId, false, false);
                };
            }
            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);
            return model;
        }
        public PushNotificationTemplateModel PrepareEditPushNotificationTemplateListModel(int Id)
        {

            var messageTemplate = _pushNotificationTemplateService.GetPushNotificationTemplateById(Id);
            if (messageTemplate == null)
                //No message template found with the specified id
                return null;

            return messageTemplate.ToModel<PushNotificationTemplateModel>();

        }

        public PushNotificationTemplateListModel PreparePushNotificationTemplateListModel(PushNotificationSearchModel searchModel, PushNotificationTemplateListModel pushNotificationTemplateListModel=null)
        {
            var pushNotificationTemplates = _pushNotificationTemplateService.GetAllPushNotificationTemplates().ToPagedList(searchModel);

            var model = new PushNotificationTemplateListModel().PrepareToGrid(searchModel, pushNotificationTemplates, () =>
            {
                return pushNotificationTemplates.Select(point =>
                {
                    //fill in model values from the entity
                    var pushNotificationTemplateModel = point.ToModel<PushNotificationTemplateModel>();
                    pushNotificationTemplateModel.Name = _localizationService.GetLocalized(point, c => c.Name, _workContext.WorkingLanguage.Id);
                    return pushNotificationTemplateModel;
                });
            });
           
            return model;
        }
        #endregion
        public void UpdateLocales(PushNotificationTemplate domainModel, PushNotificationTemplateModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(domainModel,
                                                           x => x.Title,
                                                           localized.Title,
                                                           localized.LanguageId);

                _localizedEntityService.SaveLocalizedValue(domainModel,
                                                           x => x.Message,
                                                           localized.Message,
                                                           localized.LanguageId);
            }
        }
    }
}
