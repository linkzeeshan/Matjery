using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Sms;
using Nop.Services;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Sms;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class QueuedSmsController : BaseAdminController
    {
        private readonly IQueuedSmsService _queuedSmsService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;

        public QueuedSmsController(IQueuedSmsService queuedSmsService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IWorkContext workContext,
            INotificationService notificationService)
        {
            this._queuedSmsService = queuedSmsService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._workContext = workContext;
            this._notificationService = notificationService;
        }

        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            var model = new QueuedSmsListModel
            {
                //default value
                SearchMaxSentTries = 10
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult QueuedSmsList(QueuedSmsListModel model)
        {
            DateTime? startDateValue = (model.SearchStartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SearchStartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.SearchEndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SearchEndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            var queuedSmss = _queuedSmsService.SearchSms(startDateValue, endDateValue, false, model.SearchMaxSentTries, true,
                model.Page - 1, model.PageSize);
            var gridModel = new
            {
                Data = queuedSmss.Select(x => {
                    var m = x.ToModel();
                    m.PriorityName = _localizationService.GetLocalizedEnum(x.Priority);
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    if (x.SentOnUtc.HasValue)
                        m.SentOn = _dateTimeHelper.ConvertToUserTime(x.SentOnUtc.Value, DateTimeKind.Utc);
                    return m;
                }),
                Total = queuedSmss.TotalCount
            };
            return Json(gridModel);
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-sms-by-number")]
        public IActionResult GoToSmsByNumber(QueuedSmsListModel model)
        {
            var queuedSms = _queuedSmsService.GetQueuedSmsById(model.GoDirectlyToNumber);
            if (queuedSms == null)
                return List();

            return RedirectToAction("Edit", "QueuedSms", new { id = queuedSms.Id });
        }

        public IActionResult Edit(int id)
        {
            var sms = _queuedSmsService.GetQueuedSmsById(id);
            if (sms == null)
                //No sms found with the specified id
                return RedirectToAction("List");

            var model = sms.ToModel();
            model.PriorityName = _localizationService.GetLocalizedEnum(sms.Priority);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(sms.CreatedOnUtc, DateTimeKind.Utc);
            if (sms.SentOnUtc.HasValue)
                model.SentOn = _dateTimeHelper.ConvertToUserTime(sms.SentOnUtc.Value, DateTimeKind.Utc);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public IActionResult Edit(QueuedSmsModel model, bool continueEditing)
        {
            var sms = _queuedSmsService.GetQueuedSmsById(model.Id);
            if (sms == null)
                //No sms found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                sms = model.ToEntity(sms);
                _queuedSmsService.UpdateQueuedSms(sms);

                _notificationService.SuccessNotification(_localizationService.GetResource("Admin.System.QueuedSms.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = sms.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            model.PriorityName = _localizationService.GetLocalizedEnum(sms.Priority);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(sms.CreatedOnUtc, DateTimeKind.Utc);
            if (sms.SentOnUtc.HasValue)
                model.SentOn = _dateTimeHelper.ConvertToUserTime(sms.SentOnUtc.Value, DateTimeKind.Utc);

            return View(model);
        }

        [HttpPost, ActionName("Edit"), FormValueRequired("requeue")]
        public IActionResult Requeue(QueuedSmsModel queuedSmsModel)
        {
            var queuedSms = _queuedSmsService.GetQueuedSmsById(queuedSmsModel.Id);
            if (queuedSms == null)
                //No sms found with the specified id
                return RedirectToAction("List");

            var requeuedSms = new QueuedSms
            {
                PriorityId = queuedSms.PriorityId,
                Message = queuedSms.Message,
                CreatedOnUtc = DateTime.UtcNow,
                IsRtl = queuedSms.IsRtl,
                PhoneNumber = queuedSms.PhoneNumber
            };
            _queuedSmsService.InsertQueuedSms(requeuedSms);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.System.QueuedSms.Requeued"));
            return RedirectToAction("Edit", new { id = requeuedSms.Id });
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var sms = _queuedSmsService.GetQueuedSmsById(id);
            if (sms == null)
                //No sms found with the specified id
                return RedirectToAction("List");

            _queuedSmsService.DeleteQueuedSms(sms);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.System.QueuedSms.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        public IActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (selectedIds != null)
            {
                _queuedSmsService.DeleteQueuedSms(_queuedSmsService.GetQueuedSmsByIds(selectedIds.ToArray()));
            }

            return Json(new { Result = true });
        }

        //[HttpPost, ActionName("List")]
        //[FormValueRequired("delete-all")]
        //public IActionResult DeleteAll()
        //{
        //    _queuedSmsService.DeleteAllSms();

        //    _notificationService.SuccessNotification(_localizationService.GetResource("Admin.System.QueuedSms.DeletedAll"));
        //    return RedirectToAction("List");
        //}
    }
}
