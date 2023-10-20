using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Sms;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Stores;

namespace Nop.Services
{
    public class SmsTemplateService : ISmsTemplateService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// </remarks>
        private const string SMSTEMPLATES_ALL_KEY = "Nop.smstemplate.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : template name
        /// {1} : store ID
        /// </remarks>
        private const string SMSTEMPLATES_BY_NAME_KEY = "Nop.smstemplate.name-{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string SMSTEMPLATES_PATTERN_KEY = "Nop.smstemplate.";

        #endregion

        private readonly IRepository<SmsTemplate> _smsTemplateRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IStoreMappingService _storeMappingService;

        private readonly CatalogSettings _catalogSettings;

        public SmsTemplateService(IRepository<SmsTemplate> smsTemplateRepository, IStaticCacheManager cacheManager, IStoreMappingService storeMappingService, 
            CatalogSettings catalogSettings, IRepository<StoreMapping> storeMappingRepository)
        {
            _smsTemplateRepository = smsTemplateRepository;
            _cacheManager = cacheManager;
            _storeMappingService = storeMappingService;
            _catalogSettings = catalogSettings;
            _storeMappingRepository = storeMappingRepository;
        }

        #region Methods

        /// <summary>
        /// Delete a sms template
        /// </summary>
        /// <param name="smsTemplate">Sms template</param>
        public virtual void DeleteSmsTemplate(SmsTemplate smsTemplate)
        {
            if (smsTemplate == null)
                throw new ArgumentNullException("smsTemplate");

            _smsTemplateRepository.Delete(smsTemplate);

            _cacheManager.RemoveByPrefix(SMSTEMPLATES_PATTERN_KEY);
        }

        /// <summary>
        /// Inserts a sms template
        /// </summary>
        /// <param name="smsTemplate">Sms template</param>
        public virtual void InsertSmsTemplate(SmsTemplate smsTemplate)
        {
            if (smsTemplate == null)
                throw new ArgumentNullException("smsTemplate");

            _smsTemplateRepository.Insert(smsTemplate);

            _cacheManager.RemoveByPrefix(SMSTEMPLATES_PATTERN_KEY);
        }

        /// <summary>
        /// Updates a sms template
        /// </summary>
        /// <param name="smsTemplate">Sms template</param>
        public virtual void UpdateSmsTemplate(SmsTemplate smsTemplate)
        {
            if (smsTemplate == null)
                throw new ArgumentNullException("smsTemplate");

            _smsTemplateRepository.Update(smsTemplate);

            _cacheManager.RemoveByPrefix(SMSTEMPLATES_PATTERN_KEY);
        }

        /// <summary>
        /// Gets a sms template
        /// </summary>
        /// <param name="smsTemplateId">Sms template identifier</param>
        /// <returns>Sms template</returns>
        public virtual SmsTemplate GetSmsTemplateById(int smsTemplateId)
        {
            if (smsTemplateId == 0)
                return null;

            return _smsTemplateRepository.GetById(smsTemplateId);
        }

        /// <summary>
        /// Gets a sms template
        /// </summary>
        /// <param name="smsTemplateName">Sms template name</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>Sms template</returns>
        public virtual SmsTemplate GetSmsTemplateByName(string smsTemplateName, int storeId)
        {
            if (string.IsNullOrWhiteSpace(smsTemplateName))
                throw new ArgumentException("smsTemplateName");

            CacheKey key = new CacheKey(string.Format(SMSTEMPLATES_BY_NAME_KEY, smsTemplateName, storeId));
            return _cacheManager.Get<SmsTemplate>(key, () =>
            {
                var query = _smsTemplateRepository.Table;
                query = query.Where(t => t.Name == smsTemplateName);
                query = query.OrderBy(t => t.Id);
                var templates = query.ToList();

                //store mapping
                if (storeId > 0)
                {
                    templates = templates
                        .Where(t => _storeMappingService.Authorize<SmsTemplate>(t, storeId))
                        .ToList();
                }

                return templates.FirstOrDefault();
            });

        }

        /// <summary>
        /// Gets all sms templates
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <returns>Sms template list</returns>
        public virtual IList<SmsTemplate> GetAllSmsTemplates(int storeId)
        {
            CacheKey key = new CacheKey(string.Format(SMSTEMPLATES_ALL_KEY, storeId));
            return _cacheManager.Get(key, () =>
            {
                var query = _smsTemplateRepository.Table;
                query = query.OrderBy(t => t.Name);

                //Store mapping
                if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
                {
                    query = from t in query
                            join sm in _storeMappingRepository.Table
                            on new { c1 = t.Id, c2 = "SmsTemplate" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into t_sm
                            from sm in t_sm.DefaultIfEmpty()
                            where !t.LimitedToStores || storeId == sm.StoreId
                            select t;

                    //only distinct items (group by ID)
                    query = from t in query
                            group t by t.Id
                            into tGroup
                            orderby tGroup.Key
                            select tGroup.FirstOrDefault();
                    query = query.OrderBy(t => t.Name);
                }

                return query.ToList();
            });
        }

        #endregion
    }
}
