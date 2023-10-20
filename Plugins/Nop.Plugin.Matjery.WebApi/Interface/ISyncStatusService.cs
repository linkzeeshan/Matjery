using Nop.Plugin.Matjery.WebApi.Domain;
using System.Collections.Generic;

namespace Nop.Plugin.Matjery.WebApi.Interface
{
    public interface ISyncStatusService
    {
        /// <summary>
        /// Delete a sync status
        /// </summary>
        /// <param name="syncStatus">Sync status</param>
        void DeleteSyncStatus(SyncStatus syncStatus);

        /// <summary>
        /// Inserts a sync status
        /// </summary>
        /// <param name="syncStatus">Sync status</param>
        void InsertSyncStatus(SyncStatus syncStatus);

        /// <summary>
        /// Updates a sync status
        /// </summary>
        /// <param name="syncStatus">Sync status</param>
        void UpdateSyncStatus(SyncStatus syncStatus);

        /// <summary>
        /// Gets a sync status
        /// </summary>
        /// <param name="syncStatusId">Sync status identifier</param>
        /// <returns>Sync status</returns>
        SyncStatus GetSyncStatusById(int syncStatusId);

        /// <summary>
        /// Gets a sync status
        /// </summary>
        /// <param name="syncStatusTableName"></param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Sync status</returns>
        SyncStatus GetSyncStatusByTableName(string syncStatusTableName, int storeId);

        /// <summary>
        /// Gets all sync statuss
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <returns>Sync status list</returns>
        IList<SyncStatus> GetAllSyncStatuses(int storeId);
    }
}