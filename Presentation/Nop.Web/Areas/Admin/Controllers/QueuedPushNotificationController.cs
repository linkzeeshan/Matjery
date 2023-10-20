using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Messages;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class QueuedPushNotificationController : BaseAdminController
    {
        #region Field
        private readonly IQueuedPushNotificationService _queuedPnService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IQueuedQueuedPushNotificationModelFactory _queuedPushNotificationModelFactory;
        private readonly INotificationService _notificationService;
        #endregion

        public QueuedPushNotificationController(IQueuedPushNotificationService queuedPnService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IWorkContext workContext,
            IQueuedQueuedPushNotificationModelFactory queuedPushNotificationModelFactory,
            INotificationService notificationService)
        {
            this._queuedPnService = queuedPnService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._workContext = workContext;
            this._queuedPushNotificationModelFactory = queuedPushNotificationModelFactory;
            _notificationService = notificationService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }
        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
                return (ActionResult)AccessDeniedView();
                
            var response = _queuedPushNotificationModelFactory.PrepareQueuedPushNotificationSearchModel(new QueuedPushNotificationSearchModel());
            return View(response);
        }
        [HttpPost]
        public IActionResult List(QueuedPushNotificationSearchModel command, QueuedPushNotificationListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
                return AccessDeniedView();

            var response = _queuedPushNotificationModelFactory.PrepareQueuedPushNotificationListModel(command, model);

            return Json(response);
        }
        public ActionResult Edit(int id)
        {
            var sms = _queuedPnService.GetQueuedPushNotificationById(id);
            if (sms == null)
                //No sms found with the specified id
                return RedirectToAction("List");

            var model = _queuedPushNotificationModelFactory.PrepareEditQueuedPushNotificationListModel(sms);
          
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Edit(QueuedPushNotificationModel model, bool continueEditing)
        {
            var sms = _queuedPnService.GetQueuedPushNotificationById(model.Id);
            if (sms == null)
                //No sms found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                sms = model.ToEntity(sms);
                _queuedPnService.UpdatePushNotificationQueue(sms);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.System.QueuedPushNotification.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = sms.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
             model = _queuedPushNotificationModelFactory.PrepareEditQueuedPushNotificationListModel(sms);

            return View(model);
        }

        [HttpPost, ActionName("Edit"), FormValueRequired("requeue")]
        public IActionResult Requeue(QueuedPushNotificationModel queuedSmsModel)
        {
            var queuedSms = _queuedPnService.GetQueuedPushNotificationById(queuedSmsModel.Id);
            if (queuedSms == null)
                //No sms found with the specified id
                return RedirectToAction("List");

            var requeuedSms = new QueuedPushNotification
            {
                Title = queuedSms.Title,
                SentOnUtc = queuedSms.SentOnUtc,
                SentTries = queuedSms.SentTries,
                UserStatusId = queuedSms.UserStatusId,
                PriorityId = queuedSms.PriorityId,
                Message = queuedSms.Message,
                CreatedOnUtc = DateTime.UtcNow,
                RegisterationId = queuedSms.RegisterationId
            };
            _queuedPnService.InsertPushNotificationQueue(requeuedSms);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.System.QueuedPushNotification.Requeued"));
            return RedirectToAction("Edit", new { id = requeuedSms.Id });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
                return AccessDeniedView();

            var sms = _queuedPnService.GetQueuedPushNotificationById(id);
            if (sms == null)
                //No message template found with the specified id
                return RedirectToAction("List");

            _queuedPnService.DeleteQueuedPushNotification(sms);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.System.QueuedPushNotification.Deleted"));
            return RedirectToAction("List");
        }
        [HttpPost]
        public IActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (selectedIds != null)
            {
                _queuedPnService.DeleteQueuedPushNotification(_queuedPnService.GetQueuedPushNotificationByIds(selectedIds.ToArray()));
            }

            return Json(new { Result = true });
        }
        [HttpPost, ActionName("List")]
        [FormValueRequired("delete-all")]
        public IActionResult DeleteAll()
        {
            _queuedPnService.DeleteAllPushNotifications();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.System.QueuedPushNotification.DeletedAll"));
            return RedirectToAction("List");
        }
    }
}
