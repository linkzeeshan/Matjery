using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Messages;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Controllers
{
    public class PushNotificationTemplateController : BasePublicController
    {
        #region Fields

        private readonly IPushNotificationTemplateService _pushNotificationTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly EmailAccountSettings _emailAccountSettings;

        #endregion Fields
        #region Constructors
        public PushNotificationTemplateController(IPushNotificationTemplateService pushNotificationTemplateService, IEmailAccountService emailAccountService,
             ILanguageService languageService, ILocalizationService localizationService, ILocalizedEntityService localizedEntityService,
             IMessageTokenProvider messageTokenProvider, IPermissionService permissionService, IStoreService storeService, IStoreMappingService storeMappingService,
             IWorkflowMessageService workflowMessageService, EmailAccountSettings emailAccountSettings

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
            
        }
        #endregion
        public IActionResult Index()
        {
            return View();
        }

    }
}
