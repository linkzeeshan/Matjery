using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Notifications;
using Nop.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Notifications;
using Nop.Services.Security;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Areas.Admin.Models.Notification;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Controllers
{
    public class NotificationController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly Services.Messages.INotificationService _notificationService;
        private readonly INotificationModelFactory _notificationModelFactory;
        private readonly IQueuedPushNotificationService _queuedPushNotificationService;
        #endregion
        #region Constructor
        public NotificationController(
            IPermissionService permissionService
            ,Services.Messages.INotificationService notificationService
            , INotificationModelFactory notificationModelFactory
            )
        {
            this._permissionService = permissionService;
            this._notificationService = notificationService;
            this._notificationModelFactory = notificationModelFactory;
        }
        #endregion
        #region Actions
        // GET: PushNotification
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }
        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNotification))
                return AccessDeniedView();
            //var response = _notificationModelFactory.PrepareNotificationListModel();
            var response = _notificationModelFactory.PrepareNotificationSearchModel(new NotificationSearchModel());
            return View(response);
        }
        [HttpPost]
        public IActionResult List(NotificationSearchModel command, NotificationListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNotification))
                return AccessDeniedView();
            var response = _notificationModelFactory.PrepareNotificationListModel(command, model);

            return Json(response);
        }
        [HttpGet]
        public IActionResult Create(int typeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNotification))
                return AccessDeniedView();

            var model = _notificationModelFactory.PrepareCreateNotification(typeId);

            return View(model);

        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(NotificationModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNotification))
                return AccessDeniedView();
            if (ModelState.IsValid)
            {
                var notification = _notificationModelFactory.PrepareCreateNotification(model, continueEditing);

                //_notificationService.SuccessNotification(_localizationService.GetResource("Admin.Promotions.Notification.Added"));
                //_notificationService.SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.MessageTemplates.Updated"));
                return continueEditing ? RedirectToAction("Create", new { typeId = notification.TypeId }) : RedirectToAction("List");
            }

            return View(model);
        }
            #endregion
    }
}
