using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;

using Nop.Core.Domain.Common;
using Nop.Core.Domain.Sms;
using Nop.Data;
using Nop.Services.Events;

namespace Nop.Services
{
    public partial class QueuedSmsService : IQueuedSmsService
    {
        private readonly IRepository<QueuedSms> _queuedSmsRepository;
        private readonly CommonSettings _commonSettings;
        private readonly IEventPublisher _eventPublisher;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="queuedSmsRepository">Queued sms repository</param>
        /// <param name="eventPublisher">Event published</param>
        /// <param name="dbContext">DB context</param>
        /// <param name="dataProvider">WeData provider</param>
        /// <param name="commonSettings">Common settings</param>
        public QueuedSmsService(IRepository<QueuedSms> queuedSmsRepository,
            IEventPublisher eventPublisher,
            CommonSettings commonSettings)
        {
            _queuedSmsRepository = queuedSmsRepository;
            _eventPublisher = eventPublisher;
            this._commonSettings = commonSettings;
        }

        /// <summary>
        /// Inserts a queued sms
        /// </summary>
        /// <param name="queuedSms">Queued sms</param>        
        public virtual void InsertQueuedSms(QueuedSms queuedSms)
        {
            if (queuedSms == null)
                throw new ArgumentNullException("queuedSms");

            _queuedSmsRepository.Insert(queuedSms);

            //event notification
            _eventPublisher.EntityInserted(queuedSms);
        }

        /// <summary>
        /// Updates a queued sms
        /// </summary>
        /// <param name="queuedSms">Queued sms</param>
        public virtual void UpdateQueuedSms(QueuedSms queuedSms)
        {
            if (queuedSms == null)
                throw new ArgumentNullException("queuedSms");

            _queuedSmsRepository.Update(queuedSms);

            //event notification
            _eventPublisher.EntityUpdated(queuedSms);
        }

        /// <summary>
        /// Deleted a queued sms
        /// </summary>
        /// <param name="queuedSms">Queued sms</param>
        public virtual void DeleteQueuedSms(QueuedSms queuedSms)
        {
            if (queuedSms == null)
                throw new ArgumentNullException("queuedSms");

            _queuedSmsRepository.Delete(queuedSms);

            //event notification
            _eventPublisher.EntityDeleted(queuedSms);
        }

        /// <summary>
        /// Deleted a queued smss
        /// </summary>
        /// <param name="queuedSmss">Queued smss</param>
        public virtual void DeleteQueuedSms(IList<QueuedSms> queuedSmss)
        {
            if (queuedSmss == null)
                throw new ArgumentNullException("queuedSmss");

            _queuedSmsRepository.Delete(queuedSmss);

            //event notification
            foreach (var queuedSms in queuedSmss)
            {
                _eventPublisher.EntityDeleted(queuedSms);
            }
        }

        /// <summary>
        /// Gets a queued sms by identifier
        /// </summary>
        /// <param name="queuedSmsId">Queued sms identifier</param>
        /// <returns>Queued sms</returns>
        public virtual QueuedSms GetQueuedSmsById(int queuedSmsId)
        {
            if (queuedSmsId == 0)
                return null;

            return _queuedSmsRepository.GetById(queuedSmsId);

        }

        /// <summary>
        /// Get queued smss by identifiers
        /// </summary>
        /// <param name="queuedSmsIds">queued sms identifiers</param>
        /// <returns>Queued smss</returns>
        public virtual IList<QueuedSms> GetQueuedSmsByIds(int[] queuedSmsIds)
        {
            if (queuedSmsIds == null || queuedSmsIds.Length == 0)
                return new List<QueuedSms>();

            var query = from qe in _queuedSmsRepository.Table
                        where queuedSmsIds.Contains(qe.Id)
                        select qe;
            var queuedSmss = query.ToList();
            //sort by passed identifiers
            var sortedQueuedSmss = new List<QueuedSms>();
            foreach (int id in queuedSmsIds)
            {
                var queuedSms = queuedSmss.Find(x => x.Id == id);
                if (queuedSms != null)
                    sortedQueuedSmss.Add(queuedSms);
            }
            return sortedQueuedSmss;
        }

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
        public virtual IPagedList<QueuedSms> SearchSms(DateTime? createdFromUtc, DateTime? createdToUtc, 
            bool loadNotSentItemsOnly, int maxSendTries, bool loadNewest, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _queuedSmsRepository.Table;
            if (createdFromUtc.HasValue)
                query = query.Where(qe => qe.CreatedOnUtc >= createdFromUtc);
            if (createdToUtc.HasValue)
                query = query.Where(qe => qe.CreatedOnUtc <= createdToUtc);
            if (loadNotSentItemsOnly)
                query = query.Where(qe => !qe.SentOnUtc.HasValue);

            query = query.Where(qe => qe.SentTries < maxSendTries);
            query = loadNewest ?
                //load the newest records
                query.OrderByDescending(qe => qe.CreatedOnUtc) :
                //load by priority
                query.OrderByDescending(qe => qe.PriorityId).ThenBy(qe => qe.CreatedOnUtc);

            var queuedSmss = new PagedList<QueuedSms>(query, pageIndex, pageSize);
            return queuedSmss;
        }

        /// <summary>
        /// Delete all queued smss
        /// </summary>
        //public virtual void DeleteAllSms()
        //{
        //    if (_commonSettings.UseStoredProceduresIfSupported && _dataProvider.StoredProceduredSupported)
        //    {
        //        //although it's not a stored procedure we use it to ensure that a database supports them
        //        //we cannot wait until EF team has it implemented - http://data.uservoice.com/forums/72025-entity-framework-feature-suggestions/suggestions/1015357-batch-cud-support


        //        //do all databases support "Truncate command"?
        //        string queuedSmsTableName = _dbContext.GetTableName<QueuedSms>();
        //        _dbContext.ExecuteSqlCommand(String.Format("TRUNCATE TABLE [{0}]", queuedSmsTableName));
        //    }
        //    else
        //    {
        //        var queuedSmss = _queuedSmsRepository.Table.ToList();
        //        foreach (var qe in queuedSmss)
        //            _queuedSmsRepository.Delete(qe);
        //    }
        //}
    }
}
