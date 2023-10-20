using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Nop.Services.Media;
using RestSharp;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Email sender
    /// </summary>
    public partial class EmailSender : IEmailSender
    {
        #region Fields

        private readonly IDownloadService _downloadService;
        private readonly INopFileProvider _fileProvider;
        private readonly ISmtpBuilder _smtpBuilder;
        private readonly IEmailClient _emailClient;


        #endregion

        #region Ctor

        public EmailSender(IDownloadService downloadService, INopFileProvider fileProvider
            , ISmtpBuilder smtpBuilder,
            IEmailClient emailClient

            )
        {
            _downloadService = downloadService;
            _fileProvider = fileProvider;
            _smtpBuilder = smtpBuilder;
            _emailClient = emailClient;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Create an file attachment for the specific download object from DB
        /// </summary>
        /// <param name="download">Attachment download (another attachment)</param>
        /// <returns>A leaf-node MIME part that contains an attachment.</returns>
        //protected MimePart CreateMimeAttachment(Download download)
        //{
        //    if (download is null)
        //        throw new ArgumentNullException(nameof(download));

        //    var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : download.Id.ToString();

        //    return CreateMimeAttachment($"{fileName}{download.Extension}", download.DownloadBinary, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow);
        //}

        ///// <summary>
        ///// Create an file attachment for the specific file path
        ///// </summary>
        ///// <param name="filePath">Attachment file path</param>
        ///// <param name="attachmentFileName">Attachment file name</param>
        ///// <returns>A leaf-node MIME part that contains an attachment.</returns>
        //protected MimePart CreateMimeAttachment(string filePath, string attachmentFileName = null)
        //{
        //    if (string.IsNullOrWhiteSpace(filePath))
        //        throw new ArgumentNullException(nameof(filePath));

        //    if (string.IsNullOrWhiteSpace(attachmentFileName))
        //        attachmentFileName = Path.GetFileName(filePath);

        //    return CreateMimeAttachment(
        //            attachmentFileName,
        //            _fileProvider.ReadAllBytes(filePath),
        //            _fileProvider.GetCreationTime(filePath),
        //            _fileProvider.GetLastWriteTime(filePath),
        //            _fileProvider.GetLastAccessTime(filePath));
        //}

        ///// <summary>
        ///// Create an file attachment for the binary data
        ///// </summary>
        ///// <param name="attachmentFileName">Attachment file name</param>
        ///// <param name="binaryContent">The array of unsigned bytes from which to create the attachment stream.</param>
        ///// <param name="cDate">Creation date and time for the specified file or directory</param>
        ///// <param name="mDate">Date and time that the specified file or directory was last written to</param>
        ///// <param name="rDate">Date and time that the specified file or directory was last access to.</param>
        ///// <returns>A leaf-node MIME part that contains an attachment.</returns>
        //protected MimePart CreateMimeAttachment(string attachmentFileName, byte[] binaryContent, DateTime cDate, DateTime mDate, DateTime rDate)
        //{
        //    if (!ContentType.TryParse(MimeTypes.GetMimeType(attachmentFileName), out var mimeContentType))
        //        mimeContentType = new ContentType("application", "octet-stream");

        //    return new MimePart(mimeContentType)
        //    {
        //        FileName = attachmentFileName,
        //        Content = new MimeContent(new MemoryStream(binaryContent), ContentEncoding.Default),
        //        ContentDisposition = new ContentDisposition
        //        {
        //            CreationDate = cDate,
        //            ModificationDate = mDate,
        //            ReadDate = rDate
        //        }
        //    };
        //}

        #endregion

        #region Methods

        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="emailAccount">Email account to use</param>
        /// <param name="subject">Subject</param>
        /// <param name="body">Body</param>
        /// <param name="fromAddress">From address</param>
        /// <param name="fromName">From display name</param>
        /// <param name="toAddress">To address</param>
        /// <param name="toName">To display name</param>
        /// <param name="replyTo">ReplyTo address</param>
        /// <param name="replyToName">ReplyTo display name</param>
        /// <param name="bcc">BCC addresses list</param>
        /// <param name="cc">CC addresses list</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name. If specified, then this file name will be sent to a recipient. Otherwise, "AttachmentFilePath" name will be used.</param>
        /// <param name="attachedDownloadId">Attachment download ID (another attachment)</param>
        /// <param name="headers">Headers</param>
        public virtual void SendEmail(EmailAccount emailAccount, string subject, string body,
            string fromAddress, string fromName, string toAddress, string toName,
            string replyTo = null, string replyToName = null,
            IEnumerable<string> bcc = null, IEnumerable<string> cc = null,
            string attachmentFilePath = null, string attachmentFileName = null,
            int attachedDownloadId = 0, IDictionary<string, string> headers = null)
        {
            var message = new MailMessage();

            message.From= new MailAddress(fromAddress,fromName);
            message.To.Add(new MailAddress(toAddress,toName));

            if (!string.IsNullOrEmpty(replyTo))
            {
                message.ReplyToList.Add(new MailAddress(replyTo,replyToName));
            }

            //BCC
            if (bcc != null)
            {
                foreach (var address in bcc.Where(bccValue => !string.IsNullOrWhiteSpace(bccValue)))
                {
                    message.Bcc.Add(new MailAddress(address.Trim()));
                }
            }

            //CC
            if (cc != null)
            {
                foreach (var address in cc.Where(ccValue => !string.IsNullOrWhiteSpace(ccValue)))
                {
                    message.CC.Add(new MailAddress(address.Trim()));
                }
            }

            //content

            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;


            //headers
            if (headers != null)
                foreach (var header in headers)
                {
                    message.Headers.Add(header.Key, header.Value);
                }

            //create the file attachment for this e-mail message

            if (!String.IsNullOrEmpty(attachmentFilePath) &&
               File.Exists(attachmentFilePath))
            {
                var attachment = new Attachment(attachmentFilePath);
                attachment.ContentDisposition.CreationDate = File.GetCreationTime(attachmentFilePath);
                attachment.ContentDisposition.ModificationDate = File.GetLastWriteTime(attachmentFilePath);
                attachment.ContentDisposition.ReadDate = File.GetLastAccessTime(attachmentFilePath);
                if (!String.IsNullOrEmpty(attachmentFileName))
                {
                    attachment.Name = attachmentFileName;
                }
                message.Attachments.Add(attachment);
            }

            //another attachment?
            if (attachedDownloadId > 0)
            {
                var download = _downloadService.GetDownloadById(attachedDownloadId);
                if (download != null)
                {
                    //we do not support URLs as attachments
                    if (!download.UseDownloadUrl)
                    {
                        string fileName = !String.IsNullOrWhiteSpace(download.Filename) ? download.Filename : download.Id.ToString();
                        fileName += download.Extension;


                        var ms = new MemoryStream(download.DownloadBinary);
                        var attachment = new Attachment(ms, fileName);
                        //string contentType = !String.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : "application/octet-stream";
                        //var attachment = new Attachment(ms, fileName, contentType);
                        attachment.ContentDisposition.CreationDate = DateTime.UtcNow;
                        attachment.ContentDisposition.ModificationDate = DateTime.UtcNow;
                        attachment.ContentDisposition.ReadDate = DateTime.UtcNow;
                        message.Attachments.Add(attachment);
                    }
                }
            }

            //send email
            var commonsettings = EngineContext.Current.Resolve<CommonSettings>();
            if (commonsettings.IsBiztalkServer)
            {
                _emailClient.From = string.Concat(emailAccount.DisplayName,"<", emailAccount.Email,">");//"Matjery<no-reply.test@sys.maqta.ae>";
                _emailClient.Subject = subject;
                _emailClient.EmailNote = body;
                _emailClient.IsHTMLReceived = "1";
                _emailClient.ToList = toAddress;
                _emailClient.BaseUrl = emailAccount.Host;
                _emailClient.ApiMethod = "SendEmailWithAttachments";
                string cclist = string.Empty;
                if (cc != null)
                {
                    foreach (var address in cc.Where(ccValue => !string.IsNullOrWhiteSpace(ccValue)))
                    {
                        cclist += string.Join(";", message.CC);
                    }
                }
                _emailClient.CCList = cclist;

                _emailClient.SendEmail();
            }
            else
            {
                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.UseDefaultCredentials = emailAccount.UseDefaultCredentials;
                    smtpClient.Host = emailAccount.Host;
                    smtpClient.Port = emailAccount.Port;
                    smtpClient.EnableSsl = emailAccount.EnableSsl;
                    smtpClient.Credentials = emailAccount.UseDefaultCredentials ?
                        CredentialCache.DefaultNetworkCredentials :
                        new NetworkCredential(emailAccount.Username, emailAccount.Password);
                    smtpClient.Send(message);
                }

            }
        }
        #endregion
    }
}