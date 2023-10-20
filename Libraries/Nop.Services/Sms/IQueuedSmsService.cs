using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Sms;

namespace Nop.Services
{
    public interface IQueuedSmsService
    {
        /// <summary>
        /// Inserts a queued sms
        /// </summary>
        /// <param name="queuedSms">Queued sms</param>        
        void InsertQueuedSms(QueuedSms queuedSms);

        /// <summary>
        /// Updates a queued sms
        /// </summary>
        /// <param name="queuedSms">Queued sms</param>
        void UpdateQueuedSms(QueuedSms queuedSms);

        /// <summary>
        /// Deleted a queued sms
        /// </summary>
        /// <param name="queuedSms">Queued sms</param>
        void DeleteQueuedSms(QueuedSms queuedSms);

        /// <summary>
        /// Deleted a queued smss
        /// </summary>
        /// <param name="queuedSmss">Queued smss</param>
        void DeleteQueuedSms(IList<QueuedSms> queuedSmss);

        /// <summary>
        /// Gets a queued sms by identifier
        /// </summary>
        /// <param name="queuedSmsId">Queued sms identifier</param>
        /// <returns>Queued sms</returns>
        QueuedSms GetQueuedSmsById(int queuedSmsId);

        /// <summary>
        /// Get queued smss by identifiers
        /// </summary>
        /// <param name="queuedSmsIds">queued sms identifiers</param>
        /// <returns>Queued smss</returns>
        IList<QueuedSms> GetQueuedSmsByIds(int[] queuedSmsIds);

        /// <summary>
        /// Gets all queued smss
        /// </summary>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="loadNotSentItemsOnly">A value indicating whether to load only not sent smss</param>
        /// <param name="maxSendTries">Maximum send tries</param>
        /// <param name="loadNewest">A value indicating whether we should sort queued sms descending; otherwise, ascending.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Sms item list</returns>
        IPagedList<QueuedSms> SearchSms(DateTime? createdFromUtc, DateTime? createdToUtc, 
            bool loadNotSentItemsOnly, int maxSendTries, bool loadNewest, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Delete all queued smss
        /// </summary>
        //void DeleteAllSms();
    }
}