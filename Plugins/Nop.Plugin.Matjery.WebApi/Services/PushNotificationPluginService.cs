using Nop.Core.Domain.Messages;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class PushNotificationPluginService : BasePluginService, IPushNotificationPluginService
    {
        public bool MarkAllPushNotificationAsRead(ParamsModel.QueuedPushNotificationParamsModel model)
        {
            //if (model.CustomerId <= 0)
            //    throw new ArgumentException("Empty");

            //var deviceInfo = _deviceInfoService.GetAllDeviceInfo(customerId: model.CustomerId).FirstOrDefault();
            ////don't do anything!
            //if (deviceInfo != null)
            //    return true;

            //var unReadNotifications = _queuedPushNotificationService.SearchPushNotifications(false, Int32.MaxValue, loadNewest: false,
            //    pushNotificationUserStatus: (int)PushNotificationUserStatus.NoYetRead, registerationId: deviceInfo.RegisterationId);
            //foreach (var unReadNotification in unReadNotifications)
            //{
            //    //udpated it
            //    unReadNotification.UserStatusId = (int)PushNotificationUserStatus.Read;
            //    _queuedPushNotificationService.UpdatePushNotificationQueue(unReadNotification);
            //}
            return true;
        }

        public bool MarkPushNotificationAsRead(ParamsModel.QueuedPushNotificationParamsModel model)
        {
            //if (model.QueuedPushNotificationId <= 0)
            //    throw new ArgumentException("Empty");

            //var queuedPushNotification = _queuedPushNotificationService.GetQueuedPushNotificationById(model.QueuedPushNotificationId);
            //if (queuedPushNotification == null)
            //    return true;

            ////udpated it
            //queuedPushNotification.UserStatusId = (int)PushNotificationUserStatus.Read;
            //_queuedPushNotificationService.UpdatePushNotificationQueue(queuedPushNotification);

            return true;
        }

        public bool UpdateRegisterationId(ParamsModel.PushNotificationParamsModel model)
        {
            //if (string.IsNullOrEmpty(model.DeviceId))
            //    throw new ArgumentException("Device Empty");

            //if (string.IsNullOrEmpty(model.RegistrationId))
            //    throw new ArgumentException("Device Empty"); ;

            //var deviceInfo = _deviceInfoService.GetAllDeviceInfo(deviceId: model.DeviceId).FirstOrDefault();
            //if (deviceInfo == null)
            //    return true;

            ////udpated it
            //deviceInfo.CustomerId = _workContext.CurrentCustomer.Id;
            //deviceInfo.RegisterationId = model.RegistrationId;
            //deviceInfo.UpdatedOnUtc = DateTime.UtcNow;

            //_deviceInfoService.UpdateDeviceInfo(deviceInfo);

            ////mark prev notifications as read
            //var unReadNotifications = _queuedPushNotificationService.SearchPushNotifications(false, Int32.MaxValue, loadNewest: false,
            //    pushNotificationUserStatus: (int)PushNotificationUserStatus.NoYetRead, registerationId: deviceInfo.RegisterationId);
            //foreach (var unReadNotification in unReadNotifications)
            //{
            //    //udpated it
            //    unReadNotification.UserStatusId = (int)PushNotificationUserStatus.Read;
            //    _queuedPushNotificationService.UpdatePushNotificationQueue(unReadNotification);
            //}
            return true;
        }
        public bool DeviceDelete(ParamsModel.DeviceInfoParamsModel model)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                throw new ApplicationException("Not allowed");

            var customer = _workContext.CurrentCustomer.Id>0?_workContext.CurrentCustomer:_customerService.GetCustomerByGuid(Guid.Parse(model.CustomerId));


            var device = _deviceInfoService.GetAllDeviceInfo(customer.Id, model.RegisterationId,model.DeviceId).FirstOrDefault();

            if (device != null)
            {
                _deviceInfoService.DeleteDeviceInfo(device);
            }
            return true;
        }
    }
}
