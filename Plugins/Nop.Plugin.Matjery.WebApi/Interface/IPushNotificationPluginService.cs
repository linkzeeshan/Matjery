using Nop.Plugin.Matjery.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Interface
{
   public interface IPushNotificationPluginService
    {
      bool  UpdateRegisterationId(ParamsModel.PushNotificationParamsModel model);
      bool  MarkPushNotificationAsRead(ParamsModel.QueuedPushNotificationParamsModel model);
      bool  MarkAllPushNotificationAsRead(ParamsModel.QueuedPushNotificationParamsModel model);
        bool DeviceDelete(ParamsModel.DeviceInfoParamsModel model);

    }
}
