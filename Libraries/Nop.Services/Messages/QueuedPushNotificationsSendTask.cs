using Newtonsoft.Json;
using Nop.Core.Domain.Messages;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;
using Nop.Services.QueuedPushNotificationTask;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.Messages
{
    public partial class QueuedPushNotificationsSendTask : IScheduleTask
    {
        private readonly IQueuedPushNotificationService _queuedPushNotificationService;
        private readonly ILogger _logger;

        //public QueuedPushNotificationsSendTask()
        //{
        //   // this._queuedPushNotificationService = queuedPushNotificationService;
        //   // this._logger = logger;
        //   this._queuedPushNotificationService = EngineContext.Current.Resolve<QueuedPushNotificationService>();
        //}

        public QueuedPushNotificationsSendTask(IQueuedPushNotificationService queuedPushNotification, ILogger logger)
        {
            this._queuedPushNotificationService = queuedPushNotification;
            this._logger = logger;
        }

        /// <summary>
        /// Executes a task
        /// </summary>
        public virtual void Execute()
        {
            var maxTries = 3;
            var queuedPushNotifications = _queuedPushNotificationService.SearchPushNotifications(loadNotSentItemsOnly: true,
               maxSendTries: maxTries, loadNewest: false, pageIndex: 0, pageSize: 500);
            string DeviceType = string.Empty;
            foreach (var queuedPushNotification in queuedPushNotifications)
            {
                ResponseModel response = new ResponseModel();
                try
                {
                    response =  _queuedPushNotificationService.SendPushNotification(queuedPushNotification);
                    queuedPushNotification.ResponseLog = JsonConvert.SerializeObject(response);
                    if (response.IsSuccess)
                    {
                       // QueuedPushNotificationResponse result = JsonConvert.DeserializeObject<QueuedPushNotificationResponse>(response);
                        //if (result.Success)
                            queuedPushNotification.SentOnUtc = DateTime.UtcNow;
                    }
                }
                catch (Exception exc)
                {
                  //  _logger.LogError(string.Format("Error sending push notification. {0}", exc.Message), exc);
                    _logger.Error($"PushNotification sending to device. {exc.Message}", exc);
                }
                finally
                {
                    queuedPushNotification.SentTries = queuedPushNotification.SentTries + 1;
                    queuedPushNotification.ResponseLog = response.Message;
                    _queuedPushNotificationService.UpdatePushNotificationQueue(queuedPushNotification);
                }
                //try
                //{
                //    var response =   _queuedPushNotificationService.SendPushNotification(queuedPushNotification);
                //    queuedPushNotification.ResponseLog = JsonConvert.SerializeObject(response);

                //    if (response.IsSuccess)
                //    {
                //        var result = response;
                //        if (result.IsSuccess)
                //            queuedPushNotification.SentOnUtc = DateTime.UtcNow;
                //    }
                //}
                //catch (Exception exc)
                //{
                //    //_logger.LogError(string.Format("Error sending push notification. {0}", exc.Message), exc);
                //}
                //finally
                //{
                //    queuedPushNotification.SentTries = queuedPushNotification.SentTries + 1;
                //    _queuedPushNotificationService.UpdatePushNotificationQueue(queuedPushNotification);
                //}
            }
        }


    }
}
