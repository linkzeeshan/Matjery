using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Host;
using Nop.Core.Domain.Messages;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Areas.Admin.Models.Notification;
using Nop.Web.Areas.Admin.Models.PushNotification;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class PushNotificationTemplateController : BaseAdminController
    {
        #region Fields

        private readonly IPushNotificationTemplateService _pushNotificationTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IPushNotificationTemplateModelFactory _pushNotificationTemplateModelFactory;
        private readonly Nop.Services.Localization.ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly INotificationService _notificationService;

        #endregion Fields
        #region Constructors
        public PushNotificationTemplateController(IPushNotificationTemplateService pushNotificationTemplateService, IEmailAccountService emailAccountService,
             Nop.Services.Localization.ILanguageService languageService, ILocalizationService localizationService, ILocalizedEntityService localizedEntityService,
             IMessageTokenProvider messageTokenProvider, IPermissionService permissionService, IStoreService storeService, IStoreMappingService storeMappingService,
             IWorkflowMessageService workflowMessageService, EmailAccountSettings emailAccountSettings, INotificationService notificationService,
             IPushNotificationTemplateModelFactory pushNotificationTemplateModelFactory
            )
        {
            _pushNotificationTemplateService = pushNotificationTemplateService;
            _emailAccountService = emailAccountService;
            _languageService = languageService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _messageTokenProvider = messageTokenProvider;
            _permissionService = permissionService;
            _storeService = storeService;
            _storeMappingService = storeMappingService;
            _workflowMessageService = workflowMessageService;
            _emailAccountService = emailAccountService;
            _notificationService = notificationService;
            _pushNotificationTemplateModelFactory = pushNotificationTemplateModelFactory;
        }
        #endregion
        #region Action
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }
        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
                return (ActionResult)AccessDeniedView();

            var response = _pushNotificationTemplateModelFactory.PreparePushNotificationSearchModel(new PushNotificationSearchModel());
            return View(response);
        }
        [HttpPost]
        public IActionResult List(PushNotificationSearchModel command, PushNotificationTemplateListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
                return AccessDeniedView();

            var response = _pushNotificationTemplateModelFactory.PreparePushNotificationTemplateListModel(command, model);

            return Json(response);
        }
        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
                return AccessDeniedView();

            var messageTemplate = _pushNotificationTemplateService.GetPushNotificationTemplateById(id);
            if (messageTemplate == null)
                //No message template found with the specified id
                return RedirectToAction("List");

           // var model = messageTemplate.ToModel<PushNotificationTemplateModel>();
            var model = _pushNotificationTemplateModelFactory.PreparePushNotificationTemplateModel(null, messageTemplate);

            return View(model);

        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Edit(PushNotificationTemplateModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
                return AccessDeniedView();

            var messageTemplate = _pushNotificationTemplateService.GetPushNotificationTemplateById(model.Id);
            if (messageTemplate == null)
                //No message template found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                messageTemplate = model.ToEntity(messageTemplate);
                _pushNotificationTemplateService.UpdatePushNotificationTemplate(messageTemplate);
                //locales
                _pushNotificationTemplateModelFactory.UpdateLocales(messageTemplate, model);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.MessageTemplates.Updated"));

                if (continueEditing)
                {
                    return RedirectToAction("Edit", new { id = messageTemplate.Id });
                }
                return RedirectToAction("List");
            }


            //If we got this far, something failed, redisplay form
            return View(model);
        }
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
                return AccessDeniedView();

            var messageTemplate = _pushNotificationTemplateService.GetPushNotificationTemplateById(id);
            if (messageTemplate == null)
                //No message template found with the specified id
                return RedirectToAction("List");

            _pushNotificationTemplateService.DeletePushNotificationTemplate(messageTemplate);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.MessageTemplates.Deleted"));
            return RedirectToAction("List");
        }
        #endregion

    }
}
