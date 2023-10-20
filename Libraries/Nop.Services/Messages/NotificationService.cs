using System;
using System.Collections.Generic;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Logging;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Notification service
    /// </summary>
    public partial class NotificationService : INotificationService
    {
        #region Fields

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger _logger;
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
        private readonly IWorkContext _workContext;
        private readonly INotyfService _toastNotification;

        #endregion

        #region Ctor

        public NotificationService(IHttpContextAccessor httpContextAccessor,
            ILogger logger,
            ITempDataDictionaryFactory tempDataDictionaryFactory,
            IWorkContext workContext,
            INotyfService toastNotification)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
            _workContext = workContext;
            _toastNotification = toastNotification;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Save message into TempData
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <param name="message">Message</param>
        /// <param name="encode">A value indicating whether the message should not be encoded</param>
        protected virtual void PrepareTempData(NotifyType type, string message, bool encode = true, string name=null)
        {
            var context = _httpContextAccessor.HttpContext;
            var tempData = _tempDataDictionaryFactory.GetTempData(context);

            //Messages have stored in a serialized list
            var messages = tempData.ContainsKey(NopMessageDefaults.NotificationListKey)
                ? JsonConvert.DeserializeObject<IList<NotifyData>>(tempData[NopMessageDefaults.NotificationListKey].ToString())
                : new List<NotifyData>();

            messages.Add(new NotifyData
            {
                Message = message,
                Type = type,
                Encode = encode,
                Name= name
            });

            tempData[NopMessageDefaults.NotificationListKey] = JsonConvert.SerializeObject(messages);
        }

        /// <summary>
        /// Log exception
        /// </summary>
        /// <param name="exception">Exception</param>
        protected virtual void LogException(Exception exception)
        {
            if (exception == null)
                return;
            var customer = _workContext.CurrentCustomer;
            _logger.Error(exception.Message, exception, customer);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Display notification
        /// </summary>
        /// <param name="type">Notification type</param>
        /// <param name="message">Message</param>
        /// <param name="encode">A value indicating whether the message should not be encoded</param>
        public virtual void Notification(NotifyType type, string message, bool encode = true, string name=null)
        {
            PrepareTempData(type, message, encode, name);
        }

        /// <summary>
        /// Display success notification
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="encode">A value indicating whether the message should not be encoded</param>
        public virtual void SuccessNotification(string message, bool encode = true, string name =null)
        {
            _toastNotification.Success(message);

            //  PrepareTempData(NotifyType.Success, message, encode, name);

        }

        /// <summary>
        /// Display warning notification
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="encode">A value indicating whether the message should not be encoded</param>
        public virtual void WarningNotification(string message, bool encode = true, string name = null)
        {
            _toastNotification.Warning(message + " " + name);
            //PrepareTempData(NotifyType.Warning, message, encode, name);
        }

        /// <summary>
        /// Display error notification
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="encode">A value indicating whether the message should not be encoded</param>
        public virtual void ErrorNotification(string message, bool encode = true)
        {
            _toastNotification.Error(message);
            //PrepareTempData(NotifyType.Error, message, encode);
        }

        /// <summary>
        /// Display error notification
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="logException">A value indicating whether exception should be logged</param>
        public virtual void ErrorNotification(Exception exception, bool logException = true)
        {
            if (exception == null)
                return;

            if (logException)
                LogException(exception);

            ErrorNotification(exception.Message);
        }

        #endregion
    }
}
