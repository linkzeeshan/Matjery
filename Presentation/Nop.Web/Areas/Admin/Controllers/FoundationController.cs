using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Foundations;
using Nop.Services.Foundations;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Foundation;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Controllers
{
    public class FoundationController : BaseAdminController
    {
        #region flds
        private IPermissionService _permissionService;
        private IPictureService _pictureService;
        private IWorkContext _workContext;
        private IFoundationService _foundationsService;
        private ILocalizationService _localizationService;
        private ILocalizedEntityService _localizedEntityService;
        private IFoundationModelFactory _foundationModelFactory;
        private IDownloadService _downloadService;
        private readonly INotificationService _notificationService;
        #endregion

        #region ctr
        public FoundationController(IWorkContext workContext, 
            IFoundationService _foundationsService,
            IPictureService pictureService, 
            ILocalizationService localizationService, 
            ILocalizedEntityService localizedEntityService,
            IFoundationModelFactory foundationModelFactory,
            IDownloadService downloadService,
            INotificationService notificationService,
            IPermissionService permissionService)
        {
            _workContext = workContext;
            this._foundationsService = _foundationsService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._localizedEntityService = localizedEntityService;
            this._permissionService = permissionService;
            _foundationModelFactory = foundationModelFactory;
            _downloadService = downloadService;
            _notificationService = notificationService;
        }
        #endregion

        #region methods
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageFoundations))
                return AccessDeniedView();
            //prepare model
            var model = _foundationModelFactory.PrepareFoundationSearchModel(new FoundationSearchModel());
            return View(model);
        }
        [HttpPost]
        public IActionResult List(FoundationSearchModel searchmodel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageFoundations))
                return AccessDeniedDataTablesJson();
            //prepare model
            var model = _foundationModelFactory.PrepareFoundationListModel(searchmodel);
            return Json(model);
        }
        public IActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageFoundations))
                return AccessDeniedView();

            var model = new FoundationModel();

            //default values
            model.Deleted = false;
            return View(model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Create(FoundationModel model, IFormFile uploadedFile, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageFoundations))
                return AccessDeniedView();

            int pictureId = 0;
            if (uploadedFile != null && !string.IsNullOrEmpty(uploadedFile.FileName))
            {
                try
                {
                    var contentType = uploadedFile.ContentType;
                    var vendorPictureBinary = _downloadService.GetDownloadBits(uploadedFile); 
                    var picture = _pictureService.InsertPicture(vendorPictureBinary, contentType, null);

                    if (picture != null)
                        pictureId = picture.Id;
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", _localizationService.GetResource("Foundation.ApplyAccount.Picture.ErrorMessage"));
                }
            }
            if (ModelState.IsValid)
            {
                //var foundation = model.ToEntity();
                var foundation = model.ToEntity<Foundation>();
                foundation.PictureId = pictureId;
                foundation.CreatedOnUtc = DateTime.Now;
                _foundationsService.InsertFoundation(foundation);
                //locales
               _foundationModelFactory.UpdateLocales(foundation, model);

                var notificationmessag = $"{_localizationService.GetLocalized(foundation, x => x.Name, _workContext.WorkingLanguage.Id)} {_localizationService.GetResource("Admin.Foundations.Created")}";
                _notificationService.SuccessNotification(notificationmessag);

                if (continueEditing)
                {
                    return RedirectToAction("Edit", new { id = foundation.Id });
                }
                return RedirectToAction("List");
            }
            return View(model);
        }

        public IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageFoundations))
                return AccessDeniedView();
            var foundation = _foundationsService.GetFoundationById(id);
            if (foundation == null)
                return RedirectToAction("List");

            var model = _foundationModelFactory.PrepareFoundationModel(null, foundation);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public IActionResult Edit(FoundationModel model, IFormFile uploadedFile, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageFoundations))
                return AccessDeniedView();

            var foundation = _foundationsService.GetFoundationById(model.Id);
            if (foundation == null)
                //No country found with the specified id
                return RedirectToAction("List");
            int pictureId = 0;

            if (uploadedFile != null && !string.IsNullOrEmpty(uploadedFile.FileName))
            {
                try
                {
                    var contentType = uploadedFile.ContentType;
                    //var vendorPictureBinary = uploadedFile.GetPictureBits();
                    var vendorPictureBinary = _downloadService.GetDownloadBits(uploadedFile);
                    var picture = _pictureService.InsertPicture(vendorPictureBinary, contentType, null);

                    if (picture != null)
                        pictureId = picture.Id;
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", _localizationService.GetResource("Foundation.ApplyAccount.Picture.ErrorMessage"));
                }
            }
            if (ModelState.IsValid)
            {
                foundation = model.ToEntity(foundation);
                _foundationsService.UpdateFoundation(foundation);
                //locales
                _foundationModelFactory.UpdateLocales(foundation, model);

                var notificationmessag = $"{_localizationService.GetLocalized(foundation, x => x.Name, _workContext.WorkingLanguage.Id)} {_localizationService.GetResource("Admin.Foundations.Updated")}";
                _notificationService.SuccessNotification(notificationmessag);


                if (continueEditing)
                {

                    return RedirectToAction("Edit", new { id = foundation.Id });
                }
                return RedirectToAction("List");
            }
            return View(model);
        }

        //delete
        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageFoundations))
                return AccessDeniedView();

            var foundation = _foundationsService.GetFoundationById(id);
            if (foundation == null)
                //No found with the specified id
                return RedirectToAction("List");



            //delete
            _foundationsService.DeleteFoundation(foundation);

            var notificationmessag = $"{_localizationService.GetLocalized(foundation, x => x.Name, _workContext.WorkingLanguage.Id)} {_localizationService.GetResource("Admin.Foundations.Deleted")}";
            _notificationService.SuccessNotification(notificationmessag);

            return RedirectToAction("List");
        }
        #endregion

    }
}
