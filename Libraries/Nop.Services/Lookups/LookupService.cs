using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Lookups;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Caching;
using Nop.Services.Caching.Extensions;
using Nop.Services.Events;
using Nop.Services.lookup;

namespace Nop.Services.News
{
    /// <summary>
    /// News service
    /// </summary>
    public partial class LookupService : ILookupService
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly ICacheKeyService _cacheKeyService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<Lookup> _lookupsRepository;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public LookupService(CatalogSettings catalogSettings,
            ICacheKeyService cacheKeyService,
            IEventPublisher eventPublisher,
            IRepository<NewsComment> newsCommentRepository,
            IRepository<Lookup> lookupsRepository, IWorkContext workContext)
        {
            _catalogSettings = catalogSettings;
            _cacheKeyService = cacheKeyService;
            _eventPublisher = eventPublisher;
            _lookupsRepository = lookupsRepository;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        #region Lookup

        /// <summary>
        /// Deletes a Lookup
        /// </summary>
        /// <param name="lookup">News item</param>
        public virtual void Deletelookup(Lookup lookup)
        {
            if (lookup == null)
                throw new ArgumentNullException(nameof(lookup));

            _lookupsRepository.Delete(lookup);

            //event notification
            _eventPublisher.EntityDeleted(lookup);
        }

        /// <summary>
        /// Gets a lookup
        /// </summary>
        /// <param name="lookupId">The news identifier</param>
        /// <returns>News</returns>
        public virtual Lookup GetlookupById(int lookupId, int languageId = 0, string type= "")
        {
            Lookup response = new Lookup();
            if (lookupId == 0)
                return null;
            if(type == "")
             response = _lookupsRepository.ToCachedGetById(lookupId, languageId);

            if(response.Id == 0)
            {
                var query = _lookupsRepository.Table;
                if (type != "")
                    return query.FirstOrDefault(x => x.Id == lookupId && x.LanguageId == languageId && x.Type == type);
                else
                    return query.FirstOrDefault(x => x.Id == lookupId && x.LanguageId == languageId);
            }

            return response;
        }

        /// <summary>
        /// Gets lookups
        /// </summary>
        /// <param name="lookupIds">The news identifiers</param>
        /// <returns>News</returns>
        public virtual IList<Lookup> GetlookupByIds(int[] lookupIds, int language = 0)
        {
            var query = _lookupsRepository.Table;
            return query.Where(p => lookupIds.Contains(p.Id)).ToList();
        }

        /// <summary>
        /// Gets lookups
        /// </summary>
        /// <returns>News</returns>
        public virtual List<Lookup> GetAllLookups(string[] types, int languageId = 2)
        {
            var query = _lookupsRepository.Table;
            return query.Where(p => types.Contains(p.Type) && p.LanguageId == languageId).ToList();
        }


        /// <summary>
        /// Gets all lookup
        /// </summary>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="title">Filter by news item title</param>
        /// <returns>News items</returns>
        public virtual IPagedList<Lookup> GetAllLookup(int languageId = 2,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string type = null)
        {
            var query = _lookupsRepository.Table;
            if (languageId > 0)
                query = query.Where(n => languageId == n.LanguageId);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(n => n.Type.Contains(type));

            if (!showHidden)
            {
                query = query.Where(n => n.IsActive);
            }

            query = query.OrderByDescending(n => n.Sequence);

            

            var lookups = new PagedList<Lookup>(query, pageIndex, pageSize);

            return lookups;
        }

        /// <summary>
        /// Inserts a lookup item
        /// </summary>
        /// <param name="lookup">News item</param>
        public virtual void Insertlookup(Lookup lookup)
        {
            if (lookup == null)
                throw new ArgumentNullException(nameof(lookup));

            _lookupsRepository.Insert(lookup);

            //event notification
            _eventPublisher.EntityInserted(lookup);
        }

        /// <summary>
        /// Updates the lookup item
        /// </summary>
        /// <param name="lookup">News item</param>
        public virtual void Updatelookup(Lookup lookup)
        {
            if (lookup == null)
                throw new ArgumentNullException(nameof(lookup));

            _lookupsRepository.Update(lookup);

            //event notification
            _eventPublisher.EntityUpdated(lookup);
        }

        public IList<Lookup> GetlookupByLanguageId(int langugaeId)
        {
            var query = _lookupsRepository.Table;
            return query.Where(p => p.LanguageId == langugaeId).ToList();
        }

        public IList<Lookup> GetAllLookupsBytype(string[] types, int languageId = 2)
        {
            var query = _lookupsRepository.Table;
            return query.Where(p => types.Contains(p.Type) && p.LanguageId == languageId).ToList();
        }

        #endregion


        #endregion
    }
}