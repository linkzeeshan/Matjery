using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Foundations;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Plugin.Matjery.WebApi.Extensions;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Foundations;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Vendors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class VendorPluginService : BasePluginService, IVendorPluginService
    {
        #region initialization
        private readonly VendorSettings _vendorSettings;
        private readonly IVendorRatingService _vendorRatingService;
        private readonly IReviewPluginService _reviewPluginService;
        private readonly IDownloadService _downloadService;
        private readonly INopFileProvider _fileProvider;
        private readonly IFoundationService _foundationService;
        private readonly CommonSettings _commonSettings;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IMessageTokenProvider _messageTokenProvider;

        private readonly ITokenizer _tokenizer;
        #endregion
        #region Ctor
        public VendorPluginService()
        {
            _fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
            _vendorSettings = EngineContext.Current.Resolve<VendorSettings>();
            _vendorRatingService = EngineContext.Current.Resolve<IVendorRatingService>();
            _reviewPluginService = EngineContext.Current.Resolve<IReviewPluginService>();
            _downloadService = EngineContext.Current.Resolve<IDownloadService>();
            _foundationService = EngineContext.Current.Resolve<IFoundationService>();
            _commonSettings = EngineContext.Current.Resolve<CommonSettings>();
            _emailAccountService = EngineContext.Current.Resolve<IEmailAccountService>();
            _emailAccountSettings = EngineContext.Current.Resolve<EmailAccountSettings>();
            _queuedEmailService = EngineContext.Current.Resolve<IQueuedEmailService>();
            _messageTemplateService = EngineContext.Current.Resolve<IMessageTemplateService>();
            _messageTokenProvider = EngineContext.Current.Resolve<IMessageTokenProvider>();
            _tokenizer = EngineContext.Current.Resolve<ITokenizer>();


        }

        #endregion
        #region Method
        public (VendorApplyResult, ApiValidationResultResponse) ApplyFoundation(ParamsModel.VendorApplyParamsModel model)
        {
            var foundationApplyResult = new VendorApplyResult();
            var apiValidationResult = new ApiValidationResultResponse();

            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                var currentFoundation = _foundationService.GetAllFoundations(phoneNumber: model.PhoneNumber);
                if (currentFoundation.TotalCount > 0)
                    apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        fieldName = _localizationService.GetResource("Foundations.ApplyAccount.PhoneNumber"),
                        errorDescription = _localizationService.GetResource("Foundations.ApplyAccount.AlreadyApplied")
                    });
            }

            if (apiValidationResult.fieldValidationResult.Count == 0)
            {
                var description = Core.Html.HtmlHelper.FormatText(model.Description, true, false, true, false,
                    false, false);
                //disabled by default
                var foundation = new Foundation
                {
                    Name = model.Name,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    Description = description,
                    CreatedOnUtc = DateTime.UtcNow
                };
                _foundationService.InsertFoundation(foundation);

                foundationApplyResult.Message = _localizationService.GetResource("Foundations.ApplyAccount.Submitted");
                foundationApplyResult.Submitted = true;
            }
            return (foundationApplyResult, apiValidationResult);
        }

        public (VendorApplyResult, ApiValidationResultResponse) ApplyVendor(ParamsModel.VendorApplyParamsModel model, IFormFile license)
        {

            var vendorApplyResult = new VendorApplyResult();
            var apiValidationResult = new ApiValidationResultResponse();


            var currentVendor = _vendorService.GetAllVendors(name: model.Name, pageIndex: 1, pageSize: 20, showHidden: false);
            if (currentVendor.TotalCount > 0)
                if (currentVendor.Where(x => x.Name.ToLower() == model.Name.ToLower()).Any())
                {
                    apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        fieldName = _localizationService.GetResource("Vendors.ApplyAccount.Name"),
                        errorDescription = _localizationService.GetResource("Vendors.ApplyAccount.Name.AlreadyExist")
                    });
                }

            if (!string.IsNullOrEmpty(model.PhoneNumber))
            {
                currentVendor = _vendorService.GetAllVendors(phoneNumber: model.PhoneNumber, pageIndex: 1, pageSize: 20);
                if (currentVendor.TotalCount > 0)
                    if (currentVendor.Where(x => x.PhoneNumber.ToLower() == model.PhoneNumber.ToLower()).Any())
                    {
                        apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                        {
                            fieldName = _localizationService.GetResource("Vendors.ApplyAccount.PhoneNumber"),
                            errorDescription = _localizationService.GetResource("Vendors.ApplyAccount.Name.AlreadyExist")
                        });
                    }
            }


            int pictureId = 0;
            if (model.Logo != null && !string.IsNullOrEmpty(model.Logo))
            {
                try
                {
                    Image image;
                    byte[] vendorPictureBinary = Convert.FromBase64String(model.Logo);
                    using (MemoryStream m = new MemoryStream(vendorPictureBinary))
                    {
                        image = Image.FromStream(m);
                    }
                    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo codec = codecs.First(c => c.FormatID == image.RawFormat.Guid);
                    Picture picture = _pictureService.InsertPicture(vendorPictureBinary, codec.MimeType, null);
                    if (picture != null)
                        pictureId = picture.Id;
                }
                catch (Exception)
                {
                    apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                    {
                        errorDescription = _localizationService.GetResource("Vendors.ApplyAccount.Picture.ErrorMessage")
                    });
                }
            }


            if (apiValidationResult.fieldValidationResult.Count == 0)
            {
                int TradeLicenseFileId = 0;
                if (license != null)
                {
                    var fileBinary = _downloadService.GetDownloadBits(license);
                    var download = new Download
                    {
                        DownloadGuid = Guid.NewGuid(),
                        UseDownloadUrl = false,
                        DownloadUrl = string.Empty,
                        DownloadBinary = fileBinary,
                        ContentType = license.ContentType,
                        Filename = _fileProvider.GetFileNameWithoutExtension(license.FileName),
                        Extension = _fileProvider.GetFileExtension(license.FileName),
                        IsNew = true
                    };
                    _downloadService.InsertDownload(download);
                    TradeLicenseFileId = download.Id;

                }
                var description = Core.Html.HtmlHelper.FormatText(model.Description, false, false, true, false,
                    false, false);
                //disabled by default
                var vendor = new Vendor
                {
                    Name = model.Name,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    RegisterationType = model.RegisterationType,
                    //some default settings
                    PageSize = 6,
                    AllowCustomersToSelectPageSize = true,
                    PageSizeOptions = _vendorSettings.DefaultVendorPageSizeOptions,
                    PictureId = pictureId,
                    LicenseCopyId = TradeLicenseFileId,
                    LicenseNo = model.TradeLicenseNumber,
                    EmiratesId = model.emiratesId,
                    Facebook = model.facebook,
                    Googleplus = model.googleplus,
                    Instagram = model.instagram,
                    Twitter = model.twitter,
                    WhatsApp = model.whatsApp,
                    BBM = model.bbm,
                    Description = description,
                    CreatedOnUtc = DateTime.UtcNow,
                    CreatedBy = _workContext.CurrentCustomer.Id,
                    UpdatedOnUtc = DateTime.UtcNow
                };
                if (model.RegisterationType == VendorRegisterationType.Foundation)
                    vendor.FoundationTypeId = (int)FoundationTypeStatus.CannotAproveOrRejectRequest;

                _vendorService.InsertVendor(vendor);
                //search engine name (the same as vendor name)
                var seName = _urlRecordService.ValidateSeName(vendor, vendor.Name, vendor.Name, true);
                _urlRecordService.SaveSlug(vendor, seName, 0);

                //associate to the current customer
                //but a store owner will have to manually add this customer role to "Vendors" role
                //if he wants to grant access to admin area

                _workContext.CurrentCustomer.VendorId = vendor.Id;
                var CustomerRole = _customerService.GetCustomerRoleBySystemName(NopCustomerDefaults.VendorsRoleName);
                if (!_customerService.GetCustomerRoleIds(_workContext.CurrentCustomer).Contains(CustomerRole.Id))
                {

                    _workContext.CurrentCustomer.CustomerRoles.Add(CustomerRole);
                    _customerService.AddCustomerRoleMapping(new CustomerCustomerRoleMapping { CustomerId = _workContext.CurrentCustomer.Id, CustomerRoleId = CustomerRole.Id });

                }

                _customerService.UpdateCustomer(_workContext.CurrentCustomer);

                //update picture seo file name
                var picture = _pictureService.GetPictureById(vendor.PictureId);
                if (picture != null)
                    _pictureService.SetSeoFilename(picture.Id, _pictureService.GetPictureSeName(vendor.Name));

                //notify store owner here (email)
                _workflowMessageService.SendNewVendorAccountApplyStoreOwnerNotification(
                    _workContext.CurrentCustomer,
                    vendor, _localizationSettings.DefaultAdminLanguageId);

                vendorApplyResult.Customer = this.GetCustomerInfo(_workContext.CurrentCustomer);
                vendorApplyResult.Status = HttpStatusCode.OK;
                _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.TradeIssueDate, model.IssueDate);

                _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.TradeExpiryDate, model.ExpiryDate);

                _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.TradeCategory, string.Join(",", model.CategoryId));

                vendorApplyResult.Message = _localizationService.GetResource("Vendors.ApplyAccount.Submitted");
                vendorApplyResult.Submitted = true;
            }
            return (vendorApplyResult, apiValidationResult);
        }

        public VendorContactResult ContactVendor(ParamsModel.ContactVendorParamsModel model)
        {
            try
            {
                var vendor = _vendorService.GetVendorById(model.VendorId);

                if (vendor == null)
                    throw new ArgumentNullException(nameof(vendor));

                var store = _storeContext.CurrentStore;
                var body = $"Dear {WebUtility.HtmlEncode(vendor.Name)}, <br /><br /> You have received an inquiry from {WebUtility.HtmlEncode(model.FullName)}. <br /><br />{WebUtility.HtmlEncode(model.Enquiry)}<br /><br /><br />Regards, <br/>Matjery";
                var messageTemplates = GetActiveMessageTemplates(MessageTemplateSystemNames.ContactVendorMessage, store.Id);
                if (!messageTemplates.Any())
                    return new VendorContactResult();

                //tokens
                var commonTokens = new List<Token>
            {
                new Token("ContactUs.SenderEmail", model.Email),
                new Token("ContactUs.SenderName", model.FullName),
                new Token("ContactUs.Body", body, true)
            };

                messageTemplates.Select(messageTemplate =>
                {
                    //email account
                    var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, _workContext.WorkingLanguage.Id);

                    string fromEmail;
                    string fromName;
                    //required for some SMTP servers
                    if (_commonSettings.UseSystemEmailForContactUsForm)
                    {
                        fromEmail = emailAccount.Email;
                        fromName = emailAccount.DisplayName;
                        body = $"<strong>From</strong>: {WebUtility.HtmlEncode(model.FullName)} - {WebUtility.HtmlEncode(model.Email)}<br /><br />{body}";
                    }
                    else
                    {
                        fromEmail = model.Email;
                        fromName = model.FullName;
                    }

                    var tokens = new List<Token>(commonTokens);
                    _messageTokenProvider.AddStoreTokens(tokens, store, emailAccount);

                    //event notification
                    _eventPublisher.MessageTokensAdded(messageTemplate, tokens);

                    var toEmail = vendor.Email;
                    var toName = vendor.Name;

                    return SendNotification(messageTemplate, emailAccount, _workContext.WorkingLanguage.Id, tokens, toEmail, toName,
                        fromEmail: fromEmail,
                        fromName: fromName,
                        subject: model.Subject,
                        replyToEmailAddress: model.Email,
                        replyToName: model.FullName);
                }).ToList();

                var vendorContactResult = new VendorContactResult();
                vendorContactResult.Sent = true;
                vendorContactResult.Result = _localizationService.GetResource("ContactVendor.YourEnquiryHasBeenSent");

                return vendorContactResult;
            }
            catch (Exception ex)
            {

            }
            return new VendorContactResult();
        }
        protected virtual IList<MessageTemplate> GetActiveMessageTemplates(string messageTemplateName, int storeId)
        {
            //get message templates by the name
            var messageTemplates = _messageTemplateService.GetMessageTemplatesByName(messageTemplateName, storeId);

            //no template found
            if (!messageTemplates?.Any() ?? true)
                return new List<MessageTemplate>();

            //filter active templates
            messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();

            return messageTemplates;
        }

        /// <summary>
        /// Get EmailAccount to use with a message templates
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>EmailAccount</returns>
        protected virtual EmailAccount GetEmailAccountOfMessageTemplate(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccountId = _localizationService.GetLocalized(messageTemplate, mt => mt.EmailAccountId, languageId);
            //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
            if (emailAccountId == 0)
                emailAccountId = messageTemplate.EmailAccountId;

            var emailAccount = (_emailAccountService.GetEmailAccountById(emailAccountId) ?? _emailAccountService.GetEmailAccountById(_emailAccountSettings.DefaultEmailAccountId)) ??
                               _emailAccountService.GetAllEmailAccounts().FirstOrDefault();
            return emailAccount;
        }
        /// <summary>
        /// Send notification
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="tokens">Tokens</param>
        /// <param name="toEmailAddress">Recipient email address</param>
        /// <param name="toName">Recipient name</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name</param>
        /// <param name="replyToEmailAddress">"Reply to" email</param>
        /// <param name="replyToName">"Reply to" name</param>
        /// <param name="fromEmail">Sender email. If specified, then it overrides passed "emailAccount" details</param>
        /// <param name="fromName">Sender name. If specified, then it overrides passed "emailAccount" details</param>
        /// <param name="subject">Subject. If specified, then it overrides subject of a message template</param>
        /// <returns>Queued email identifier</returns>
        public virtual int SendNotification(MessageTemplate messageTemplate,
            EmailAccount emailAccount, int languageId, IEnumerable<Token> tokens,
            string toEmailAddress, string toName,
            string attachmentFilePath = null, string attachmentFileName = null,
            string replyToEmailAddress = null, string replyToName = null,
            string fromEmail = null, string fromName = null, string subject = null)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException(nameof(messageTemplate));

            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            //retrieve localized message template data
            var bcc = _localizationService.GetLocalized(messageTemplate, mt => mt.BccEmailAddresses, languageId);
            if (string.IsNullOrEmpty(subject))
                subject = _localizationService.GetLocalized(messageTemplate, mt => mt.Subject, languageId);
            var body = _localizationService.GetLocalized(messageTemplate, mt => mt.Body, languageId);

            //Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            //limit name length
            toName = CommonHelper.EnsureMaximumLength(toName, 300);

            var email = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                From = !string.IsNullOrEmpty(fromEmail) ? fromEmail : emailAccount.Email,
                FromName = !string.IsNullOrEmpty(fromName) ? fromName : emailAccount.DisplayName,
                To = string.IsNullOrEmpty(toEmailAddress) ? "email@notprovided.com" : toEmailAddress,
                ToName = toName,
                ReplyTo = replyToEmailAddress,
                ReplyToName = replyToName,
                CC = null,
                Bcc = bcc,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                AttachmentFilePath = attachmentFilePath,
                AttachmentFileName = attachmentFileName,
                AttachedDownloadId = messageTemplate.AttachedDownloadId,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id,
                DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                    : (DateTime?)(DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
            };

            _queuedEmailService.InsertQueuedEmail(email);
            return email.Id;
        }


        public List<VendorReviewListResultModel> GetAllVendorReviews(int vendorId)
        {
            var vendorReviewList = new List<VendorReviewListResultModel>();
            try
            {
                var vendor = _vendorService.GetVendorById(vendorId);
                if (vendor == null || !vendor.Active || vendor.Deleted)
                    return new List<VendorReviewListResultModel>();


                IPagedList<VendorReview> vendorReviews = _vendorRatingService.GetAllVendorReviews(0,
                    vendor.Id, _storeContext.CurrentStore.Id, true);

                foreach (VendorReview vr in vendorReviews)
                {
                    vr.Customer = _customerService.GetCustomerById(vr.CustomerId);

                    vr.Vendor = _vendorService.GetVendorById(vr.VendorId);
                    if (vr.Customer != null)
                    {
                        var vendorReviewListModel = new VendorReviewListResultModel();
                        vendorReviewListModel.Id = vr.Id;
                        vendorReviewListModel.CustomerId = vr.CustomerId;
                        vendorReviewListModel.CustomerName = _genericAttributeService.GetAttribute<string>(vr.Customer, NopCustomerDefaults.FirstNameAttribute);
                        vendorReviewListModel.AllowViewingProfiles = _customerSettings.AllowViewingProfiles && vr.Customer != null && !_customerService.IsGuest(_workContext.CurrentCustomer);
                        vendorReviewListModel.Title = vr.Title;
                        vendorReviewListModel.ReviewText = vr.ReviewText;
                        vendorReviewListModel.Rating = vr.Rating;
                        DateTime userTime = _dateTimeHelper.ConvertToUserTime(vr.CreatedOnUtc, DateTimeKind.Utc);
                        vendorReviewListModel.CreatedON = userTime.ToShortDateString();
                        vendorReviewList.Add(vendorReviewListModel);
                    }
                }

                //double totalRating = 0;
                //if (vendorReviews.TotalCount != 0)
                //{
                //    double percentage = ((vendorReviews.Sum(vr => vr.Rating) * 100) / vendorReviews.TotalCount) / 5;
                //    //5= maximut rating values
                //    totalRating = percentage * 5 / 100;
                //}

                return vendorReviewList;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public List<VendorResult> GetAllVendors(DateTime? syncDate = null)
        {
            var vendorResult = new List<VendorResult>();
            //we don't allow viewing of vendors if "vendors" block is hidden
            if (_vendorSettings.VendorsBlockItemsToDisplay > 0)
            {
                IPagedList<Vendor> vendors = null;
                if (syncDate.HasValue)
                {
                    DateTime syncDateUtc = _dateTimeHelper.ConvertToUtcTime(syncDate.Value);
                    //check for force sync
                    //var vendorSyncTable = _syncStatusService.GetSyncStatusByTableName("Vendor", 0);
                    //if (vendorSyncTable != null && vendorSyncTable.ForceSync && vendorSyncTable.ForceSyncDate > syncDateUtc)
                    //    vendors = _vendorService.GetAllVendors(showHidden: false);
                    //else
                    //showNotActive must be true, otherwise sync won't work if Active property is changed
                    vendors = _vendorService.GetAllVendors(syncDate: syncDateUtc, showNotActive: true);
                }
                else
                {
                    vendors = _vendorService.GetAllVendors(showHidden: false);
                }
                foreach (var vendor in vendors)
                {
                    var vendorModel = this.PrepareVendorModel(vendor);
                    vendorResult.Add(vendorModel);
                }
            }
            return vendorResult;

        }

        public TradelicenseStatus GetVendorLicenseStatus()
        {
            var lisenseStatus = new TradelicenseStatus();
            var vendor = _workContext.CurrentVendor;
            lisenseStatus = _vendorService.GetVendorLicenseStatus(vendor);
            lisenseStatus.Message = LicenseAlert(lisenseStatus.Id);
            return lisenseStatus;
        }
        public string LicenseAlert(int status)
        {
            string Message = "";
            switch (status)
            {
                case (int)VendorLicenseStatus.Active:
                    Message = _localizationService.GetLocaleStringResourceByName("account.vendorinfo.licenseactive").ResourceValue;
                    break;
                case (int)VendorLicenseStatus.Expired:
                    Message = _localizationService.GetLocaleStringResourceByName("account.register.errors.licenseexpiry").ResourceValue;
                    break;
                case (int)VendorLicenseStatus.AboutToExpire:
                    Message = _localizationService.GetLocaleStringResourceByName("account.vendorinfo.licensenotification").ResourceValue;
                    break;

                default:
                    Message = "";
                    break;
            }
            return Message;
        }

        public VendorResult GetVendorById(int vendorId)
        {
            var vendorModel = new VendorResult();
            //we don't allow viewing of vendors if "vendors" block is hidden
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null || vendor.Deleted || !vendor.Active)
                return vendorModel;

            //Vendor is active?
            if (!vendor.Active)
                return vendorModel;

            vendorModel = this.PrepareVendorModel(vendor);
            CommonHelper.StripHTML(vendorModel.Description);
            CommonHelper.StripHTML(vendorModel.ShortDescription);
            return vendorModel;
        }

        public List<VendorResult> GetVendorSupportedVendors(int vendorId)
        {
            var vendorResultList = new List<VendorResult>();
            //we don't allow viewing of vendors if "vendors" block is hidden
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null || vendor.Deleted || !vendor.Active)
                return vendorResultList;

            //Vendor is active?
            if (!vendor.Active)
                return vendorResultList;

            var supportedVendors = _vendorService.GetAllVendors(showSupportedVendors: true, supportedByFoundationId: vendor.Id);

            foreach (var supportedVendor in supportedVendors.Where(x => x.FoundationAprovalStatusId == (int)FoundationAprovalStatus.Aproved))
            {
                var vendorModel = this.PrepareVendorModel(supportedVendor);
                vendorResultList.Add(vendorModel);
            }
            return vendorResultList;
        }

        public (VendorResult, ApiValidationResultResponse) SaveVendorInfo(ParamsModel.VendorInfoParamsModel model, IFormFile license)
        {
            try
            {


                var apiValidationResult = new ApiValidationResultResponse();
                Vendor vendor = _vendorService.GetVendorById(model.Id);
                int pictureId = 0;
                if (model.ImageUrl != null && !string.IsNullOrEmpty(model.ImageUrl))
                {
                    //add
                    try
                    {
                        Image image;
                        byte[] vendorPictureBinary = Convert.FromBase64String(model.ImageUrl);

                        using (MemoryStream m = new MemoryStream(vendorPictureBinary))
                        {
                            image = Image.FromStream(m);
                        }
                        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                        ImageCodecInfo codec = codecs.First(c => c.FormatID == image.RawFormat.Guid);
                        Picture picture = _pictureService.InsertPicture(vendorPictureBinary, codec.MimeType, null);
                        if (picture != null)
                            pictureId = picture.Id;
                    }
                    catch (Exception)
                    {
                        apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                        {
                            errorDescription =
                                _localizationService.GetResource("Vendors.ApplyAccount.Picture.ErrorMessage")
                        });
                    }
                }
                else
                {
                    //update
                    pictureId = model.PictureId;
                }
                int TradeLicenseFileId = 0;
                if (license != null)
                {
                    var fileBinary = _downloadService.GetDownloadBits(license);
                    var download = new Download
                    {
                        DownloadGuid = Guid.NewGuid(),
                        UseDownloadUrl = false,
                        DownloadUrl = string.Empty,
                        DownloadBinary = fileBinary,
                        ContentType = license.ContentType,
                        Filename = _fileProvider.GetFileNameWithoutExtension(license.FileName),
                        Extension = _fileProvider.GetFileExtension(license.FileName),
                        IsNew = true
                    };
                    _downloadService.InsertDownload(download);
                    TradeLicenseFileId = download.Id;

                }
                else
                {
                    TradeLicenseFileId = model.LicenseId;
                }




                vendor.Name = model.Name;
                vendor.EmiratesId = model.EmiratesId;
                vendor.Email = model.Email;
                vendor.Description = model.Description;
                vendor.PhoneNumber = model.PhoneNumber;
                vendor.PictureId = pictureId;
                vendor.LicenseCopyId = TradeLicenseFileId > 0 ? TradeLicenseFileId : model.LicenseId;
                vendor.LicenseNo = model.TradeLicenseNumber;
                vendor.UpdatedOnUtc = DateTime.UtcNow;
                vendor.WhatsApp = model.WhatsApp;
                vendor.Facebook = model.Facebook;
                vendor.Instagram = model.Instagram;
                vendor.Twitter = model.Twitter;
                vendor.Googleplus = model.Googleplus;
                vendor.BBM = model.BBM;
                vendor.EnrollForTraining = model.EnrollForTraining;

                _vendorService.UpdateVendor(vendor);
                _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.TradeIssueDate, model.IssueDate);

                _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.TradeExpiryDate, model.ExpiryDate);

                _genericAttributeService.SaveAttribute(vendor, NopVendorDefaults.TradeCategory, string.Join(",", model.CategoryId));
                VendorResult vendorResult = this.PrepareVendorModel(vendor);
                return (vendorResult, apiValidationResult);
            }
            catch (Exception ex)
            {

            }
            return (new VendorResult(), new ApiValidationResultResponse());
        }

        public (VendorResult, ApiValidationResultResponse) VendorFollowAndUnfollow(ParamsModel.VendorFollowAddParamsModel model)
        {
            var vendor = _vendorService.GetVendorById(model.VendorId);
            var apiValidationResult = new ApiValidationResultResponse();
            if (apiValidationResult.fieldValidationResult.Count == 0)
            {
                VendorFollower existingFollower = _vendorFollowerService.GetAllFollowers(_workContext.CurrentCustomer.Id,
                    vendor.Id, showUnFollowed: true).FirstOrDefault();
                if (existingFollower != null && !existingFollower.Unfollowed)
                {
                    existingFollower.UnFollowOnUtc = DateTime.UtcNow;
                    existingFollower.Unfollowed = true;
                    // existingFollower.FollowOnUtc = null;
                    _vendorFollowerService.UpdateFollower(existingFollower);
                }
                else
                {
                    if (existingFollower != null && existingFollower.Unfollowed)
                    {
                        existingFollower.UnFollowOnUtc = null;
                        existingFollower.Unfollowed = false;
                        existingFollower.FollowOnUtc = DateTime.UtcNow;
                        _vendorFollowerService.UpdateFollower(existingFollower);
                    }
                    else
                    {
                        var follower = new VendorFollower
                        {
                            CustomerId = _workContext.CurrentCustomer.Id,
                            VendorId = vendor.Id,
                            FollowOnUtc = DateTime.UtcNow
                        };
                        _vendorFollowerService.InsertFollower(follower);
                    }
                }
            }

            var vendorModel = this.PrepareVendorModel(vendor);
            return (vendorModel, apiValidationResult);
        }

        public bool VendorSupportedByAdd(ParamsModel.SupportedByVendorAddParamsModel model)
        {
            var vendor = _vendorService.GetVendorById(model.VendorId);
            if (vendor == null || !vendor.Active || vendor.Deleted)
                return false;
            var foundation = _vendorService.GetFoundationById(model.SupportedByFoundationId);
            if (foundation == null || !foundation.Active || foundation.Deleted)
                return false;

            vendor.SupportedByFoundationId = foundation.Id;
            vendor.FoundationAprovalStatus = FoundationAprovalStatus.Pending;
            _vendorService.UpdateVendor(vendor);

            return true;
        }


        public List<VendorResult> GetAllVendors(int pageIndex = 0, int pageSize = int.MaxValue, bool displayEntities = false)
        {

            var vendorResult = new List<VendorResult>();
            //we don't allow viewing of vendors if "vendors" block is hidden
            if (_vendorSettings.VendorsBlockItemsToDisplay > 0)
            {
                IPagedList<Vendor> vendors = null;
                //if (syncDate.HasValue)
                //{
                // DateTime syncDateUtc = _dateTimeHelper.ConvertToUtcTime(syncDate.Value);
                //check for force sync
                //var vendorSyncTable = _syncStatusService.GetSyncStatusByTableName("Vendor", 0);
                //if (vendorSyncTable != null && vendorSyncTable.ForceSync && vendorSyncTable.ForceSyncDate > syncDateUtc)
                //    vendors = _vendorService.GetAllVendors(showHidden: false);
                //else
                //showNotActive must be true, otherwise sync won't work if Active property is changed
                //vendors = _vendorService.GetAllVendors(syncDate: syncDateUtc, showNotActive: true);
                //}
                //else
                //{
                vendors = Convert.ToBoolean(displayEntities) ? _vendorService.GetAllEntitiesMod(
                vendorRegisterationType: Convert.ToBoolean(displayEntities) ? VendorRegisterationType.Foundation : VendorRegisterationType.Merchant,
                  pageSize: 10,
                  pageIndex: 0) : _vendorService.GetAllVendors(showHidden: false, pageIndex: pageIndex, pageSize: pageSize, customer: _workContext.CurrentCustomer);

                // vendors = _vendorService.GetAllVendors(showHidden: false,pageIndex:pageIndex,pageSize:pageSize, customer:_workContext.CurrentCustomer);
                //}
                foreach (var vendor in vendors)
                {
                    var vendorModel = this.PrepareVendorModel(vendor);
                    vendorResult.Add(vendorModel);
                }
            }
            return vendorResult;
        }


        public (VendorReviewResultModel, ApiValidationResultResponse) VendorReviewAdd(ParamsModel.VendorReviewAddParamsModel model)
        {


            var vendor = _vendorService.GetVendorById(model.VendorId);
            if (vendor == null || !vendor.Active || vendor.Deleted)
                throw new ArgumentException("Not Found");

            var apiValidationResult = new ApiValidationResultResponse();
            if (_customerService.IsGuest(_workContext.CurrentCustomer) && !_vendorReviewSettings.AllowAnonymousUsersToReviewVendor)
            {
                apiValidationResult.fieldValidationResult.Add(new ApiValidationResultEntryResponse
                {
                    errorDescription =
                        _localizationService.GetResource(
                            "Nop.Plugin.NopWarehouse.VendorReview.Reviews.OnlyRegisteredUsersCanWriteReviews")
                });
            }
            var vendorReviewResult = new VendorReviewResultModel();
            if (apiValidationResult.fieldValidationResult.Count == 0)
            {
                int rating = model.Rating;
                if (rating < 1 || rating > 5)
                {
                    rating = _vendorReviewSettings.DefaultVendorRatingValue;
                }
                bool isApproved = !_vendorReviewSettings.VendorReviewsMustBeApproved;
                var vendorReview = new VendorReview
                {
                    VendorId = model.VendorId,
                    CustomerId = _workContext.CurrentCustomer.Id,
                    Title = model.Title,
                    ReviewText = model.ReviewText,
                    Rating = rating,
                    HelpfulYesTotal = 0,
                    HelpfulNoTotal = 0,
                    IsApproved = isApproved,
                    CreatedOnUtc = DateTime.UtcNow,
                    StoreId = _storeContext.CurrentStore.Id
                };
                _vendorRatingService.InsertVendorReview(vendorReview);
                _customerActivityService.InsertActivity("PublicStore.AddVendorReview",
                    _localizationService.GetResource(
                        "Nop.Plugin.NopWarehouse.VendorReview.ActivityLog.PublicStore.AddVendorReview"), vendor);

                vendorReviewResult.Added = true;
                if (isApproved)
                {
                    vendorReviewResult.Result =
                        _localizationService.GetResource(
                            "Nop.Plugin.NopWarehouse.VendorReview.Reviews.SuccessfullyAdded");
                }
                else
                {
                    vendorReviewResult.Result =
                        _localizationService.GetResource(
                            "Nop.Plugin.NopWarehouse.VendorReview.Reviews.SeeAfterApproving");
                }
            }

            return (vendorReviewResult, apiValidationResult);

        }

        protected virtual VendorResult PrepareVendorModel(Vendor vendor)
        {
            string vendorUrl = string.Format("{0}v/{1}", _webHelper.GetStoreLocation(),
            _urlRecordService.GetSeName(vendor, _workContext.WorkingLanguage.Id));

            var vendorModel = new VendorResult
            {
                Id = vendor.Id,
                Name = _localizationService.GetLocalized(vendor, x => x.Name),
                Email = vendor.Email,
                VendorUrl = vendorUrl,
                Description = CommonHelper.StripHTML(_localizationService.GetLocalized(vendor, x => x.Description)),
                SeName = _urlRecordService.GetSeName(vendor),
                RegisterationTypeId = vendor.RegisterationTypeId,
                AllowCustomersToContactVendors = _vendorSettings.AllowCustomersToContactVendors,
                WhatsApp = vendor.WhatsApp,
                EmiratesId = vendor.EmiratesId,
                PictureId = vendor.PictureId,
                PhoneNumber = vendor.PhoneNumber,
                Instagram = vendor.Instagram,
                Twitter = vendor.Twitter,
                Facebook = vendor.Facebook,
                Googleplus = vendor.Googleplus,
                BBM = vendor.BBM,
                EnrollForTraining = vendor.EnrollForTraining,
                FoundationAprovalStatusId = vendor.FoundationAprovalStatusId,
                FoundationTypeId = vendor.FoundationTypeId,
                SupportedByFoundationId = vendor.SupportedByFoundationId,
                DisplayOrder = vendor.DisplayOrder,
                Active = vendor.Active,
                TradeLicenseNumber = vendor.LicenseNo,
                LicenseId = vendor.LicenseCopyId
            };
            if (vendor.FoundationAprovalStatusId.HasValue)
            {
                vendorModel.FoundationAprovalStatusText = _localizationService.GetLocalizedEnum(vendor.FoundationAprovalStatus, _workContext.WorkingLanguage.Id);
            }
            vendorModel.ShortDescription = CommonHelper.StripHTML(vendorModel.Description.TruncateToShort());
            foreach (Language language in _languageService.GetAllLanguages())
            {
                var localeModel = new
                {
                    Name = _localizationService.GetLocalized(vendor, x => x.Name, language.Id),
                    Description = CommonHelper.StripHTML(_localizationService.GetLocalized(vendor, x => x.Description, language.Id)),// vendor.GetLocalized(x => x.Description, language.Id),
                    ShortDescription = CommonHelper.StripHTML(_localizationService.GetLocalized(vendor, x => x.Description, language.Id)).TruncateToShort(), //vendor.GetLocalized(x => x.Description, language.Id).TruncateToShort(),
                    LicensedBy = _localizationService.GetLocalized(vendor, x => x.LicensedBy, language.Id), // vendor.GetLocalized(v => v.LicensedBy, language.Id)
                };
                vendorModel.CustomProperties.Add(language.UniqueSeoCode, localeModel);
            }
            //license

            Download download = _downloadService.GetDownloadById(vendor.LicenseCopyId);
            vendorModel.FileName = download != null ? download.Filename : "";
            vendorModel.TradeLicenseFile = download == null ? "" : _storeContext.CurrentStore.Url + "vendor/DownloadFile?downloadGuid=" + download.DownloadGuid.ToString();

            //prepare picture model
            int pictureSize = _mediaSettings.VendorThumbPictureSize;
            var picture = _pictureService.GetPictureById(vendor.PictureId);
            if (picture != null)
            {
                vendorModel.FullSizeImageUrl = _pictureService.GetPictureUrl(picture.Id, defaultPictureType: PictureType.VendorOrEntity);
                vendorModel.ImageUrl = _pictureService.GetPictureUrl(picture.Id, pictureSize, defaultPictureType: PictureType.VendorOrEntity);
            }
            //vendorModel.WaterMarkImageUrl = _miscWatermarkPictureService.GetPictureUrlWithWatermark(picture, pictureSize, defaultPictureType: PictureType.VendorOrEntity);
            vendorModel.TotalProducts = _productService.GetNumberOfProductsByVendorId(vendor.Id);

            //reviews
            var vendorReviews = _vendorRatingService.GetAllVendorReviews(vendorId: vendor.Id);
            vendorModel.VendorReview.TotalRating = _reviewPluginService.GetVendorRating(vendor.Id);
            //if (vendorReviews.TotalCount != 0)
            //{
            //    double percentage = vendorReviews.Sum(vr => vr.Rating) * 100 / vendorReviews.TotalCount / 5;
            //    //5= maximut rating values
            //    vendorModel.VendorReview.TotalRating = percentage * 5 / 100; 

            //}
            //followers
            var vendorFollowers = _vendorFollowerService.GetAllFollowers(vendorId: vendor.Id);
            foreach (VendorFollower vr in vendorFollowers)
            {
                var vendorfollowerstModel = new VendorResult.VendorFollowerModel.FollowersModel();
                vendorfollowerstModel.CustomerId = vr.CustomerId;
                vendorfollowerstModel.CustomerName = _customerService.FormatUsername(vr.Customer);
                DateTime userTime = _dateTimeHelper.ConvertToUserTime(vr.FollowOnUtc, DateTimeKind.Utc);
                vendorfollowerstModel.FollowOnStr = userTime.ToShortDateString();
                vendorModel.VendorFollower.Followers.Add(vendorfollowerstModel);
            }
            //license
            vendorModel.ExpiryDate = _genericAttributeService.GetAttribute<string>(vendor, NopVendorDefaults.TradeExpiryDate);
            vendorModel.IssueDate = _genericAttributeService.GetAttribute<string>(vendor, NopVendorDefaults.TradeIssueDate);
            string categories = _genericAttributeService.GetAttribute<string>(vendor, NopVendorDefaults.TradeCategory);
            vendorModel.CategoryId = string.IsNullOrEmpty(categories) ? null : categories.Split(",");
            vendorModel.LicenseNo = vendor.LicenseNo;
            if (vendor.LicenseCopyId > 0)
            {
                var licenseCopyDownload = _downloadService.GetDownloadById(vendor.LicenseCopyId);
                if (licenseCopyDownload != null)
                    vendorModel.HasUploadedLicense = true;
            }

            vendorModel.VendorFollower.TotalFollowers = vendorFollowers.TotalCount;



            return vendorModel;
        }
        private CustomerInfoResult GetCustomerInfo(Customer customer)
        {
            var customerInfoResult = new CustomerInfoResult()
            {
                Idpk = customer.Id,
                Id = customer.CustomerGuid.ToString(),
                FirstName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.FirstNameAttribute),
                LastName = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.LastNameAttribute),
                Gender = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.GenderAttribute),
                PhoneNumber = _genericAttributeService.GetAttribute<string>(customer, NopCustomerDefaults.PhoneAttribute),
                FullName = _customerService.GetCustomerFullName(customer),
                FormattedUserName = _customerService.FormatUsername(customer),
                Email = customer.Email,
                IsVendor = _customerService.IsVendor(customer) && this.IsVendorActive(customer.VendorId),
                IsAdmin = _customerService.IsAdmin(customer),
                IsTranslator = _customerService.IsTranslator(customer),
                IsFoundation = _customerService.IsFoundation(customer) && this.IsFoundationActive(customer.VendorId),
                NationalityCountryId = _genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.NationalityCountryId),
                HasAcceptedTermsAndConditions = _genericAttributeService.GetAttribute<bool>(customer, NopCustomerDefaults.HasAcceptedTermsAndConditions),
                VendorId = customer.VendorId
            };
            DateTime? dateOfBirth = _genericAttributeService.GetAttribute<DateTime?>(customer, NopCustomerDefaults.DateOfBirthAttribute);
            if (dateOfBirth.HasValue)
            {
                customerInfoResult.DateOfBirth = new DateTime(dateOfBirth.Value.Year, dateOfBirth.Value.Month,
                    dateOfBirth.Value.Day, 0, 0, 0, DateTimeKind.Utc);
            }
            customerInfoResult.Username = customer.Email;
            if (_customerSettings.UsernamesEnabled)
            {
                customerInfoResult.Username = customer.Username;
            }
            if (_customerSettings.AllowCustomersToUploadAvatars)
            {
                customerInfoResult.AvatarUrl = _pictureService.GetPictureUrl(_genericAttributeService.GetAttribute<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute),
                    _mediaSettings.AvatarPictureSize, storeLocation: _storeContext.CurrentStore.Url);
            }
            var externalProvider = _openAuthenticationService.GetProviderSystemName(customer.Id);
            if (!string.IsNullOrEmpty(externalProvider))
                customerInfoResult.ProviderSystemName = externalProvider;

            return customerInfoResult;
        }
        private bool IsVendorActive(int vendorId)
        {
            var vendor = _vendorService.GetVendorById(vendorId);
            if (vendor == null)
                return false;

            return vendor.Active;
        }
        private bool IsFoundationActive(int vendorId)
        {
            var foundation = _vendorService.GetFoundationById(vendorId);
            if (foundation == null)
                return false;

            return foundation.Active;
        }
        #endregion
    }
}
