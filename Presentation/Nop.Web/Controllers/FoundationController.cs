using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Foundations;
using Nop.Core.Domain.Localization;
using Nop.Services.Common;
using Nop.Services.Foundations;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Web.Framework.Controllers;
using Nop.Web.Models.Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Controllers
{
    
        public class FoundationController : BasePublicController
        {
            private IFoundationService _foundationService;
            private ILocalizationService _localizationService;
            private IPictureService _pictureService;
            private IWorkContext _workContext;
            private readonly IWorkflowMessageService _workflowMessageService;
            private readonly LocalizationSettings _localizationSettings;
            private readonly IGenericAttributeService _genericAttributeService;

            public FoundationController(IWorkContext workContext, IPictureService pictureService, ILocalizationService localizationService, IFoundationService foundationService, IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings, IGenericAttributeService genericAttributeService)
            {
                this._workContext = workContext;
                this._pictureService = pictureService;
                this._localizationService = localizationService;
                this._foundationService = foundationService;
                this._workflowMessageService = workflowMessageService;
                this._localizationSettings = localizationSettings;
                _genericAttributeService = genericAttributeService;
            }

            #region Methods

            //[NopHttpsRequirement(SslRequirement.Yes)]
            public ActionResult ApplyFoundation()
            {
                //if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
                //    return RedirectToRoute("HomePage");

                //if (!_workContext.CurrentCustomer.IsRegistered())
                //    return new HttpUnauthorizedResult();

                var model = new ApplyFoundationModel();
                if (_workContext.CurrentCustomer.VendorId > 0)
                {
                    //already applied for vendor account
                    //model.DisableFormInput = true;
                    //model.Result = _localizationService.GetResource("Vendors.ApplyAccount.AlreadyApplied");
                    //return View(model);
                }

                model.Email = _workContext.CurrentCustomer.Email;
                var customer = _workContext.CurrentCustomer;
                model.PhoneNumber =_genericAttributeService.GetAttribute<Customer, string>(customer.Id, NopCustomerDefaults.PhoneAttribute);
                return View(model);
            }
            [HttpPost, ActionName("ApplyFoundation")]
           // [PublicAntiForgery]
            [FormValueRequired("ApplyFoundationSubmit")]
            public ActionResult ApplyFoundationSubmit(ApplyFoundationModel model, IFormFile uploadedFile)
            {
                //if (!_vendorSettings.AllowCustomersToApplyForVendorAccount)
                //    return RedirectToRoute("HomePage");
                int pictureId = 0;

                if (uploadedFile != null && !string.IsNullOrEmpty(uploadedFile.FileName))
                {
                    try
                    {
                        var contentType = uploadedFile.ContentType;
                        var vendorPictureBinary = uploadedFile.Length;
         
                        if (vendorPictureBinary > 0)
                        {
                            using (var ms = new MemoryStream())
                            {
                                uploadedFile.CopyTo(ms);
                                var fileBytes = ms.ToArray();
                                var picture = _pictureService.InsertPicture(fileBytes, contentType, null);
                                if (picture != null)
                                    pictureId = picture.Id;
                            }
                        }
                        
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", _localizationService.GetResource("Vendors.ApplyAccount.Picture.ErrorMessage"));
                    }
                }

                if (ModelState.IsValid)
                {
                    var description = Core.Html.HtmlHelper.FormatText(model.Description, false, false, true, false, false, false);
                    //disabled by default

                    var foundation = new Foundation
                    {
                        Name = model.Name,
                        Email = model.Email,
                        PhoneNumber = model.PhoneNumber,
                        PictureId = pictureId,
                        Description = description,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                    _foundationService.InsertFoundation(foundation);

                    //send notfiy mail to admin
                    _workflowMessageService.SendApplyFoundationSubmitNotificationMessage(foundation, _localizationSettings.DefaultAdminLanguageId);

                    //

                    model.DisableFormInput = true;
                    model.Result = _localizationService.GetResource("Foundation.ApplyAccount.Submitted");
                    return View(model);
                }
                return View(model);
            }
            #endregion
        }
    
}
