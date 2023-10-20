using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Sms;
using Nop.Services;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Areas.Admin.Models.Sms;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class SmsPluginController : BaseAdminController
    {
        #region Fields

        private readonly ISmsTemplateService _smsTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly INotificationService _notificationService;
        private readonly ISmsPluginModelFactory _smsPluginModelFactory;

        #endregion Fields

        #region Constructors

        public SmsPluginController(ISmsTemplateService smsTemplateService,
            IEmailAccountService emailAccountService,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            ICustomerActivityService customerActivityService,
            INotificationService notificationService,
            ISmsPluginModelFactory smsPluginModelFactory
            )
        {
            this._smsTemplateService = smsTemplateService;
            this._emailAccountService = emailAccountService;
            this._languageService = languageService;
            this._localizedEntityService = localizedEntityService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._customerActivityService = customerActivityService;
            this._notificationService = notificationService;
            this._smsPluginModelFactory = smsPluginModelFactory;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected virtual void UpdateLocales(SmsTemplate mt, SmsTemplateModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(mt,
                                                           x => x.Message,
                                                           localized.Message,
                                                           localized.LanguageId);
            }
        }

        [NonAction]
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

        [NonAction]
        protected virtual void SaveStoreMappings(SmsTemplate smsTemplate, SmsTemplateModel model)
        {
            smsTemplate.LimitedToStores = model.SelectedStoreIds.Any();

            var existingStoreMappings = _storeMappingService.GetStoreMappings(smsTemplate);
            var allStores = _storeService.GetAllStores();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        _storeMappingService.InsertStoreMapping(smsTemplate, store.Id);
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        _storeMappingService.DeleteStoreMapping(storeMappingToDelete);
                }
            }
        }

        #endregion

        #region Methods

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            //if (!_permissionService.Authorize(StandardPermissionProvider.ManageSmsTemplates))
            //    return AccessDeniedView();

            var model = _smsPluginModelFactory.PrepareSmsTemplateSearchModel(new SmsTemplateSearchModel());
            return View(model);
        }

        [HttpPost]
        public ActionResult List(SmsTemplateSearchModel model)
        {
            //if (!_permissionService.Authorize(StandardPermissionProvider.ManageSmsTemplates))
            //    return AccessDeniedView();
            var response = _smsPluginModelFactory.PrepareSmsPluginListModel(model);

            return Json(response);
        }

        public ActionResult Edit(int id)
        {
            var model = _smsPluginModelFactory.PrepareEditSmsTemplateModel(id);
            if(model == null)
                return RedirectToAction("List");
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public ActionResult Edit(SmsTemplateModel model, bool continueEditing)
        {
            //if (!_permissionService.Authorize(StandardPermissionProvider.ManageSmsTemplates))
            //    return AccessDeniedView();

            var smsTemplate = _smsTemplateService.GetSmsTemplateById(model.Id);
            if (smsTemplate == null)
                //No sms template found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                smsTemplate = model.ToEntity(smsTemplate);
                _smsTemplateService.UpdateSmsTemplate(smsTemplate);

                //activity log
                _customerActivityService.InsertActivity("EditSmsTemplate", _localizationService.GetResource("ActivityLog.EditSmsTemplate"));

                //Stores
                SaveStoreMappings(smsTemplate, model);
                //locales
                UpdateLocales(smsTemplate, model);

                this._notificationService.SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.SmsTemplates.Updated"));

                if (continueEditing)
                {
                    return RedirectToAction("Edit", new { id = smsTemplate.Id });
                }
                return RedirectToAction("List");
            }

            //Store
            PrepareStoresMappingModel(model, smsTemplate, true);
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            //if (!_permissionService.Authorize(StandardPermissionProvider.ManageSmsTemplates))
            //    return AccessDeniedView();

            var smsTemplate = _smsTemplateService.GetSmsTemplateById(id);
            if (smsTemplate == null)
                //No sms template found with the specified id
                return RedirectToAction("List");

            _smsTemplateService.DeleteSmsTemplate(smsTemplate);

            //activity log
            _customerActivityService.InsertActivity("DeleteSmsTemplate", _localizationService.GetResource("ActivityLog.DeleteSmsTemplate"));

            this._notificationService.SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.SmsTemplates.Deleted"));
            return RedirectToAction("List");
        }

        #endregion
    }
}