using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Plugin.Matjery.WebApi.Domain;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class SyncStatusService : ISyncStatusService
    {

        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// </remarks>
        public static CacheKey SYNCSTATUSES_ALL_KEY => new CacheKey("Nop.syncstatus.all-{0}");
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : template name
        /// {1} : store ID
        /// </remarks>
        //private const string SYNCSTATUSES_BY_NAME_KEY = "Nop.syncstatus.name-{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string SYNCSTATUSES_PATTERN_KEY = "Nop.syncstatus.";

        public static CacheKey SYNCSTATUSES_BY_NAME_KEY => new CacheKey("Nop.syncstatus.name-{0}-{1}");


        //var cacheKey = this.cacheKeyService.PrepareKeyForDefaultCache(NopCatalogDefaults.CATEGORY_NUMBER_OF_PRODUCTS_MODEL_KEY, string.Join(",", _customerService.GetCustomerRoleIds(_workContext.CurrentCustomer)),
        //               _storeContext.CurrentStore.Id,
        //               category.Id);

        #endregion

        private readonly IRepository<SyncStatus> syncStatusRepository;
        private readonly IStaticCacheManager cacheManager;
        private readonly IStoreMappingService storeMappingService;
        private readonly CatalogSettings catalogSettings;
        private readonly IRepository<StoreMapping> storeMappingRepository;
        private readonly ICacheKeyService cacheKeyService;

        public SyncStatusService(
            IRepository<SyncStatus> syncStatusRepository,
            IStaticCacheManager cacheManager,
            IStoreMappingService storeMappingService,
            CatalogSettings catalogSettings,
            IRepository<StoreMapping> storeMappingRepository,
            ICacheKeyService cacheKeyService)
        {
            this.syncStatusRepository = syncStatusRepository;
            this.cacheManager = cacheManager;
            this.storeMappingService = storeMappingService;
            this.catalogSettings = catalogSettings;
            this.storeMappingRepository = storeMappingRepository;
            this.cacheKeyService = cacheKeyService;
        }
        #region Methods

        /// <summary>
        /// Delete a sync status
        /// </summary>
        /// <param name="syncStatus">Sync status</param>
        public virtual void DeleteSyncStatus(SyncStatus syncStatus)
        {
            if (syncStatus == null)
                throw new ArgumentNullException("syncStatus");

            this.syncStatusRepository.Delete(syncStatus);

            this.cacheManager.RemoveByPrefix(SYNCSTATUSES_PATTERN_KEY);
            this.cacheManager.Remove(SYNCSTATUSES_ALL_KEY);
        }

        /// <summary>
        /// Inserts a sync status
        /// </summary>
        /// <param name="syncStatus">Sync status</param>
        public virtual void InsertSyncStatus(SyncStatus syncStatus)
        {
            if (syncStatus == null)
                throw new ArgumentNullException("syncStatus");

            this.syncStatusRepository.Insert(syncStatus);

            this.cacheManager.RemoveByPrefix(SYNCSTATUSES_PATTERN_KEY);
            this.cacheManager.Remove(SYNCSTATUSES_ALL_KEY);
        }

        /// <summary>
        /// Updates a sync status
        /// </summary>
        /// <param name="syncStatus">Sync status</param>
        public virtual void UpdateSyncStatus(SyncStatus syncStatus)
        {
            if (syncStatus == null)
                throw new ArgumentNullException("syncStatus");

            this.syncStatusRepository.Update(syncStatus);

            this.cacheManager.RemoveByPrefix(SYNCSTATUSES_PATTERN_KEY);
            this.cacheManager.Remove(SYNCSTATUSES_ALL_KEY);
        }

        /// <summary>
        /// Gets a sync status
        /// </summary>
        /// <param name="syncStatusId">Sync status identifier</param>
        /// <returns>Sync status</returns>
        public virtual SyncStatus GetSyncStatusById(int syncStatusId)
        {
            if (syncStatusId == 0)
                return null;

            return this.syncStatusRepository.GetById(syncStatusId);
        }

        /// <summary>
        /// Gets a sync status
        /// </summary>
        /// <param name="syncStatusTableName"></param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Sync status</returns>
        public virtual SyncStatus GetSyncStatusByTableName(string syncStatusTableName, int storeId)
        {
            if (string.IsNullOrWhiteSpace(syncStatusTableName))
                throw new ArgumentException("syncStatusTableName");
            
            var key = this.cacheKeyService.PrepareKeyForShortTermCache(SYNCSTATUSES_BY_NAME_KEY, syncStatusTableName, storeId);

            return this.cacheManager.Get(key, () =>
            {
                var query = this.syncStatusRepository.Table;
                query = query.Where(t => t.TableName.ToLower() == syncStatusTableName.ToLower());
                query = query.OrderBy(t => t.Id);
                var templates = query.ToList();

                //store mapping
                if (storeId > 0)
                {
                    templates = templates
                        .Where(t => this.storeMappingService.Authorize<SyncStatus>(t, storeId))
                        .ToList();
                }

                return templates.FirstOrDefault();
            });

        }

        /// <summary>
        /// Gets all sync statuss
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <returns>Sync status list</returns>
        public virtual IList<SyncStatus> GetAllSyncStatuses(int storeId)
        {
            var key = this.cacheKeyService.PrepareKeyForShortTermCache(SYNCSTATUSES_ALL_KEY, storeId);

            return this.cacheManager.Get(key, () =>
            {
                var query = this.syncStatusRepository.Table;
                query = query.OrderBy(t => t.TableName);

                //Store mapping
                if (storeId > 0 && ! this.catalogSettings.IgnoreStoreLimitations)
                {
                    query = from t in query
                            join sm in this.storeMappingRepository.Table
                            on new { c1 = t.Id, c2 = "SyncStatus" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into t_sm
                            from sm in t_sm.DefaultIfEmpty()
                            where !t.LimitedToStores || storeId == sm.StoreId
                            select t;

                    //only distinct items (group by ID)
                    query = from t in query
                            group t by t.Id
                            into tGroup
                            orderby tGroup.Key
                            select tGroup.FirstOrDefault();
                    query = query.OrderBy(t => t.TableName);
                }

                return query.ToList();
            });
        }

        #endregion
    }
}
