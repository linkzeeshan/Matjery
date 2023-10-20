using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Matjery.WebApi.Domain;
using Nop.Plugin.Matjery.WebApi.Extensions;
using Nop.Plugin.Matjery.WebApi.Infrastructure;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Services.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SyncController : BaseController
    {
        private readonly IStaticCacheManager cacheManager;
        private readonly IPictureService pictureService;
        private readonly ISettingService settingService;
        private readonly ISyncStatusService syncStatusService;
        private readonly IDateTimeHelper dateTimeHelper;
        private readonly IDeviceInfoService deviceInfoService;

        public SyncController(IStaticCacheManager cacheManager, IPictureService pictureService,
            ISettingService settingService, ISyncStatusService syncStatusService, IDateTimeHelper dateTimeHelper
            , IDeviceInfoService deviceInfoService)
        {
            this.cacheManager = cacheManager;
            this.pictureService = pictureService;
            this.settingService = settingService;
            this.syncStatusService = syncStatusService;
            this.dateTimeHelper = dateTimeHelper;
            this.deviceInfoService = deviceInfoService;
        }


        /// <summary>
        /// To Save device information
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("DeviceRegister", Name = "Device Registration")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult DeviceRegister(DeviceInfo model)
        {
            DeviceInfo deviceInfo = new DeviceInfo();
            try
            {
                if (string.IsNullOrWhiteSpace(model.RegisterationId))
                {
                    throw new ArgumentNullException(nameof(model.RegisterationId));
                }
                if (string.IsNullOrWhiteSpace(model.Platform))
                {
                    throw new ArgumentNullException(nameof(model.Platform));
                }
                var oldappversion = new Version("4.0.0");
                if (!string.IsNullOrWhiteSpace(model.Version))
                {
                    var storeInformationSettings = this.settingService.LoadSetting<StoreInformationSettings>();

                    var latestAppVersion = new Version(storeInformationSettings.AppVersion);
                    var currentAppVersion = new Version(model.Version);
                    if (latestAppVersion > currentAppVersion)
                    {
                        if(currentAppVersion<= oldappversion)
                        {
                            return Ok(base.PrepareStringResponse(new { requiresUpdate = true }));
                        }
                        else
                        {
                            return Ok(base.Prepare(new { requiresUpdate = true }));
                        }
                     
                    }
                }
                DateTime date =  new DateTime(2000, 1, 1);
                //give 5min more margin
                //date = date.AddMinutes(-5);
                //date = this.dateTimeHelper.ConvertToUtcTime(date);
                //IList<SyncStatus> statusList = this.syncStatusService.GetAllSyncStatuses(base._storeContext.CurrentStore.Id);

                //var tablesToUpdate = new List<string>();
                //IEnumerable<SyncStatus> tables = statusList.Where(o => o.LastUpdateDate > date || (o.ForceSync && o.ForceSyncDate > date)).ToList();
                //int totalTables = tables.Count();
                //if (totalTables > 0)
                //{
                //    foreach (SyncStatus syncStatus in tables)
                //    {
                //        tablesToUpdate.Add(syncStatus.TableName);
                //    }
                //}

                //store device information
                if (!string.IsNullOrEmpty(model.Platform))
                {
                    try
                    {
                         deviceInfo = this.deviceInfoService.GetAllDeviceInfo(deviceId: model.DeviceId).LastOrDefault();
                        if (deviceInfo != null && deviceInfo.Platform.ToLower() == model.Platform.ToLower())
                        {
                            //udpated it
                            deviceInfo.CustomerId = _workContext.CurrentCustomer.Id;
                            if (!string.IsNullOrEmpty(model.RegisterationId))
                                deviceInfo.RegisterationId = model.RegisterationId;

                            this.deviceInfoService.UpdateDeviceInfo(deviceInfo);

                            var devresult = base.Prepare(deviceInfo);
                            return Ok(devresult);
                        }
                        else
                        {
                            deviceInfo = new DeviceInfo
                            {
                                CreatedOnUtc = DateTime.UtcNow,
                                UpdatedOnUtc = DateTime.UtcNow,
                                Platform = model.Platform,
                                DeviceId = model.DeviceId,
                                Version = model.Version,
                                Manufacturer = model.Manufacturer,
                                Serial = model.Serial,
                                RegisterationId = model.RegisterationId,
                                Model = model.Model,
                                CustomerId = _workContext.CurrentCustomer.Id
                            };
                            this.deviceInfoService.InsertDeviceInfo(deviceInfo);

                            var devresult = base.Prepare(deviceInfo);
                            return Ok(devresult);
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Failed to register device: " + ex.Message, ex, _workContext.CurrentCustomer);
                    }
                }
                else
                {
                    throw new ArgumentNullException(nameof(model.Platform));
                }
                
                ApiResultResponse result = base.Prepare(deviceInfo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, "Server Error");
            }

        }

        /// <summary>
        /// To Get Top menu Category
        /// </summary>
        /// <param name="rootCategoryId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Get", Name = "Get Sync Service")]
        [ProducesResponseType(typeof(IEnumerable), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ValidatePermissionFilter()]
        public IActionResult Get(string lastUpdateDate, string appVersionNo = null,
            string platform = "", string deviceId = "", string version = ""
            , string manufacturer = "", string serial = "", string model = "")
        {
            try
            {
                var oldappversion = new Version("3.3.5");
                if (!string.IsNullOrWhiteSpace(appVersionNo))
                {
                    var storeInformationSettings = this.settingService.LoadSetting<StoreInformationSettings>();

                    var latestAppVersion = new Version(storeInformationSettings.AppVersion);
                    var currentAppVersion = new Version(appVersionNo);
                    if (latestAppVersion > currentAppVersion)
                    {
                        if (currentAppVersion <= oldappversion)
                        {
                            return Ok(base.PrepareStringResponse(new { requiresUpdate = true }));
                        }
                        else
                        {
                            return Ok(base.Prepare(new { requiresUpdate = true }));
                        }

                    }
                }
                DateTime date = lastUpdateDate.ToDateTimeNullable() ?? new DateTime(2000, 1, 1);
                //give 5min more margin
                date = date.AddMinutes(-5);
                date = this.dateTimeHelper.ConvertToUtcTime(date);
                IList<SyncStatus> statusList = this.syncStatusService.GetAllSyncStatuses(base._storeContext.CurrentStore.Id);

                var tablesToUpdate = new List<string>();
                IEnumerable<SyncStatus> tables = statusList.Where(o => o.LastUpdateDate > date || (o.ForceSync && o.ForceSyncDate > date)).ToList();
                int totalTables = tables.Count();
                if (totalTables > 0)
                {
                    foreach (SyncStatus syncStatus in tables)
                    {
                        tablesToUpdate.Add(syncStatus.TableName);
                    }
                }

                //store device information
                if (!string.IsNullOrEmpty(platform))
                {
                    try
                    {
                        var deviceInfo = this.deviceInfoService.GetAllDeviceInfo(deviceId: deviceId).LastOrDefault();
                        if (deviceInfo != null)
                        {
                            //udpated it
                            deviceInfo.CustomerId = _workContext.CurrentCustomer.Id;
                            this.deviceInfoService.UpdateDeviceInfo(deviceInfo);
                        }
                        else
                        {
                            deviceInfo = new DeviceInfo
                            {
                                CreatedOnUtc = DateTime.UtcNow,
                                UpdatedOnUtc = DateTime.UtcNow,
                                Platform = platform,
                                DeviceId = deviceId,
                                Version = version,
                                Manufacturer = manufacturer,
                                Serial = serial,
                                RegisterationId = string.Empty,
                                Model = model,
                                CustomerId = _workContext.CurrentCustomer.Id
                            };
                            this.deviceInfoService.InsertDeviceInfo(deviceInfo);
                        }

                    }
                    catch (Exception ex)
                    {
                        //_logger.LogError("Failed to register device: " + ex.Message, ex, _workContext.CurrentCustomer);
                    }
                }

                ApiResultResponse result = base.Prepare(new { total = totalTables, tables = tablesToUpdate });
                return Ok(result);
                //var currentUserAppVersion = new Version(appVersionNo);
                //if (currentUserAppVersion <= oldappversion)
                //{
                //    ApiResult result = base.PrepareStringResponse(new { total = totalTables, tables = tablesToUpdate });
                //    return Ok(result);
                //}
                //else
                //{
                //    ApiResultResponse result = base.Prepare(new { total = totalTables, tables = tablesToUpdate });
                //    return Ok(result);
                //}
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex.Message, ex, _workContext.CurrentCustomer);
                return StatusCode(StatusCodes.Status500InternalServerError, "Server Error");
            }

        }


    }
}
