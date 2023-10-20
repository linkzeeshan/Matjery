using Nop.Core.Domain.Messages;
using Nop.Data;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace Nop.Services.Messages
{
    public partial class DeviceInfoService : IDeviceInfoService
    {
        private readonly IRepository<DeviceInfo> _deviceInfoRepository;
        private readonly IEventPublisher _eventPublisher;

        public DeviceInfoService(IRepository<DeviceInfo> deviceInfoRepository,
            IEventPublisher eventPublisher)
        {
            this._deviceInfoRepository = deviceInfoRepository;
            this._eventPublisher = eventPublisher;
        }

        /// <summary>
        /// Inserts an deviceInfo account
        /// </summary>
        /// <param name="deviceInfo">Device Info</param>
        public virtual void InsertDeviceInfo(DeviceInfo deviceInfo)
        {
            if (deviceInfo == null)
                throw new ArgumentNullException("deviceInfo");

            _deviceInfoRepository.Insert(deviceInfo);

            //event notification
            _eventPublisher.EntityInserted(deviceInfo);
        }

        /// <summary>
        /// Updates an deviceInfo
        /// </summary>
        /// <param name="deviceInfo">Device Info</param>
        public virtual void UpdateDeviceInfo(DeviceInfo deviceInfo)
        {
            if (deviceInfo == null)
                throw new ArgumentNullException("deviceInfo");

            _deviceInfoRepository.Update(deviceInfo);

            //event notification
            _eventPublisher.EntityUpdated(deviceInfo);
        }

        /// <summary>
        /// Deletes an deviceInfo account
        /// </summary>
        /// <param name="deviceInfo">Device Info</param>
        public virtual void DeleteDeviceInfo(DeviceInfo deviceInfo)
        {
            if (deviceInfo == null)
                throw new ArgumentNullException("deviceInfo");

            _deviceInfoRepository.Delete(deviceInfo);

            //event notification
            _eventPublisher.EntityDeleted(deviceInfo);
        }

        /// <summary>
        /// Gets an deviceInfo account by identifier
        /// </summary>
        /// <param name="deviceInfoId">The deviceInfo account identifier</param>
        /// <returns>Device Info</returns>
        public virtual DeviceInfo GetDeviceInfoById(int deviceInfoId)
        {
            if (deviceInfoId == 0)
                return null;

            return _deviceInfoRepository.GetById(deviceInfoId);
        }
        /// <summary>
        /// Gets an deviceInfo account by identifier
        /// </summary>
        /// <param name="customerId">The deviceInfo account identifier</param>
        /// <returns>Device Info</returns>
        public virtual DeviceInfo GetDeviceInfoByCustomerId(int customerId)
        {
            if (customerId == 0)
                return null;
            var query = from ea in _deviceInfoRepository.Table
                        select ea;
            return query.FirstOrDefault(x => x.CustomerId == customerId);
        }

        /// <summary>
        /// Gets all deviceInfo accounts
        /// </summary>
        /// <returns>Device Infos list</returns>
        public virtual IList<DeviceInfo> GetAllDeviceInfo(int? customerId = null, string registerationId = "", string deviceId = "")
        {
            var query = from ea in _deviceInfoRepository.Table
                        select ea;

            if (customerId.HasValue && customerId.Value > 0)
                query = query.Where(d => d.CustomerId == customerId.Value);

            if (!string.IsNullOrEmpty(registerationId))
                query = query.Where(d => d.RegisterationId == registerationId);

            if (!string.IsNullOrEmpty(deviceId))
                query = query.Where(d => d.DeviceId == deviceId);

            query = query.OrderBy(d => d.CreatedOnUtc);

            var deviceInfos = query.ToList();
            return deviceInfos;
        }
    }
}
