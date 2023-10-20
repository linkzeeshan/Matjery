using Nop.Core.Domain.Messages;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Messages
{
    public interface IDeviceInfoService
    {
        void DeleteDeviceInfo(DeviceInfo deviceInfo);
        IList<DeviceInfo> GetAllDeviceInfo(int? customerId = null, string registerationId = "", string deviceId = "");
        DeviceInfo GetDeviceInfoById(int deviceInfoId);
        DeviceInfo GetDeviceInfoByCustomerId(int customerId);
        void InsertDeviceInfo(DeviceInfo deviceInfo);
        void UpdateDeviceInfo(DeviceInfo deviceInfo);
    }
}
