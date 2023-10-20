using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Sms;
using Nop.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Areas.Admin.Models.Notification;
using Nop.Web.Areas.Admin.Models.Sms;
using Nop.Web.Areas.Admin.Models.Sms;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public class SmsPluginModelFactory : ISmsPluginModelFactory
    {
        #region Fields
        private readonly ISmsTemplateService _smsTemplateService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly ISmsTemplateService _SmsTemplateService;
        private IWorkContext _workContext;
        #endregion
        #region Ctr
        public SmsPluginModelFactory(ISmsTemplateService smsTemplateService, IStoreMappingService storeMappingService, IStoreService storeService,
            ISmsTemplateService SmsTemplateService, ILocalizedEntityService localizedEntityService,
             ILocalizationService localizationService, ILocalizedModelFactory localizedModelFactory , IWorkContext workContext)
        {
            _smsTemplateService = smsTemplateService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _localizedEntityService = localizedEntityService;
            _SmsTemplateService = SmsTemplateService;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _workContext = workContext;
        }

        public SmsTemplateSearchModel PrepareSmsTemplateSearchModel(SmsTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.AvailableStores.Add(new SelectListItem { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var s in _storeService.GetAllStores())
                searchModel.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }
        public SmsTemplateModel PrepareSmsTemplateModel(SmsTemplateModel model, SmsTemplate smsTemplate, bool excludeProperties = false)
        {
            Action<SmsTemplateLocalizedModel, int> localizedModelConfiguration = null;
             
            if (smsTemplate != null)
            {
                //fill in model values from the entity
                model ??= smsTemplate.ToModel<SmsTemplateModel>();

                PrepareStoresMappingModel(model, smsTemplate, false);
                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Message = _localizationService.GetLocalized(smsTemplate, entity => entity.Message, languageId, false, false);
                };
            }
            
            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);
            return model;
        }
        public SmsTemplateModel PrepareEditSmsTemplateModel(int id)
        {

            Action<SmsTemplateLocalizedModel, int> localizedModelConfiguration = null;

            var smsTemplate = _smsTemplateService.GetSmsTemplateById(id);
            var model = smsTemplate.ToModel<SmsTemplateModel>();
            //Store
            PrepareStoresMappingModel(model, smsTemplate, false);

            if (smsTemplate != null)
            {
                PrepareStoresMappingModel(model, smsTemplate, false);
                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Message = _localizationService.GetLocalized(smsTemplate, entity => entity.Message, languageId, false, false);
                };
            }

            //prepare localized models
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);
            return model;

        }

        public SmsTemplateListModel PrepareSmsPluginListModel(SmsTemplateSearchModel searchModel)
        {
            var smspTemplates = _smsTemplateService.GetAllSmsTemplates(searchModel.SearchStoreId).ToPagedList(searchModel);

            var model = new SmsTemplateListModel().PrepareToGrid(searchModel, smspTemplates, () =>
            {
                return smspTemplates.Select(point =>
                {
                    //fill in model values from the entity
                    var smspTemplateModel = point.ToModel<SmsTemplateModel>();
                    smspTemplateModel.Message = _localizationService.GetLocalized(point, x => x.Message);
                    smspTemplateModel.Name = _localizationService.GetLocalized(point, x => x.Name);
                    var stores = _storeService
                           .GetAllStores()
                           .Where(s => !point.LimitedToStores || smspTemplateModel.SelectedStoreIds.Contains(s.Id))
                           .ToList();
                    for (int i = 0; i < stores.Count; i++)
                    {
                        smspTemplateModel.ListOfStores += stores[i].Name;
                        if (i != stores.Count - 1)
                            smspTemplateModel.ListOfStores += ", ";
                    }
                    return smspTemplateModel;
                });
            });
           
            return model;
        }
        #endregion
        public void UpdateLocales(SmsTemplate domainModel, SmsTemplateModel model)
        {
            foreach (var localized in model.Locales)
            {

                _localizedEntityService.SaveLocalizedValue(domainModel,
                                                           x => x.Message,
                                                           localized.Message,
                                                           localized.LanguageId);
            }
        }
        protected virtual void PrepareStoresMappingModel(SmsTemplateModel model, SmsTemplate smsTemplate, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (!excludeProperties && smsTemplate != null)
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(smsTemplate).ToList();

            var allStores = _storeService.GetAllStores();
            foreach (var store in allStores)
            {
                model.AvailableStores.Add(new SelectListItem
                {
                    Text = store.Name,
                    Value = store.Id.ToString(),
                    Selected = model.SelectedStoreIds.Contains(store.Id)
                });
            }
        }
    }
}
