using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Caching;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Services.Customers
{
    public partial class CustomerTranslatorService : ICustomerTranslatorService
    {
        #region Constants
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : entity ID
        /// {1} : entity name
        /// </remarks>
        /// 

        //private const string CUTOMERTRANSLATIONMAPPING_BY_ENTITYID_NAME_KEY = "Nop.customertranslationmapping.entityid-name-{0}";
        ///// <summary>
        ///// Key for caching
        ///// </summary>
        //private const string CUSTOMERTRANSTLATORMAPPING_ALL_KEY = "Nop.customertranslatormapping.all";
        ///// <summary>
        ///// Key pattern to clear cache
        ///// </summary>
        //private const string CUSTOMERTRANSTLATORMAPPING_PATTERN_KEY = "Nop.customertranslatormapping.";
        ///// <summary>
        ///// Key for caching
        ///// </summary>
        ///// <remarks>
        ///// {0} : customer attribute ID
        ///// </remarks>
        //private const string CUSTOMERTRANSTLATORMAPPING_BY_ID_KEY = "Nop.customertranslatormapping.id-{0}";

        public static CacheKey CUTOMERTRANSLATIONMAPPING_BY_ENTITYID_NAME_KEY => new CacheKey("Nop.customertranslationmapping.entityid-name-{0}", CUTOMERTRANSLATIONMAPPING_BY_ENTITYID_NAME_KEY_PREFIX);
        public static string CUTOMERTRANSLATIONMAPPING_BY_ENTITYID_NAME_KEY_PREFIX => "Nop.customertranslationmapping";

        public static CacheKey CUSTOMERTRANSTLATORMAPPING_ALL_KEY => new CacheKey("Nop.customertranslatormapping.all}", CUSTOMERTRANSTLATORMAPPING_ALL_KEY_PREFIX);
        public static string CUSTOMERTRANSTLATORMAPPING_ALL_KEY_PREFIX => "Nop.customertranslatormapping.all";

        public static CacheKey CUSTOMERTRANSTLATORMAPPING_PATTERN_KEY => new CacheKey("Nop.customertranslatormapping.", CUSTOMERTRANSTLATORMAPPING_PATTERN_KEY_PREFIX);
        public static string CUSTOMERTRANSTLATORMAPPING_PATTERN_KEY_PREFIX => "Nop.customertranslatormapping.";

        public static CacheKey CUSTOMERTRANSTLATORMAPPING_BY_ID_KEY => new CacheKey("Nop.customertranslatormapping.id-{0}", CUSTOMERTRANSTLATORMAPPING_BY_ID_KEY_PREFIX);
        public static string CUSTOMERTRANSTLATORMAPPING_BY_ID_KEY_PREFIX => "Nop.customertranslatormapping.id-{0}";
        #endregion
        #region Fields
        private readonly IRepository<CustomerTranslationMapping> _customerTranslationMappingRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly ICacheKeyService _cacheKeyService;
        #endregion
        #region Ctor
        public CustomerTranslatorService(IStaticCacheManager cacheManager,
           IRepository<CustomerTranslationMapping> customerTranslationMappingRepository,
           IEventPublisher eventPublisher,
           IRepository<Product> productRepository,
           IRepository<LocalizedProperty> localizedPropertyRepository,
           ICacheKeyService cacheKeyService)
        {
            _cacheManager = cacheManager;
            _cacheKeyService = cacheKeyService;
            _customerTranslationMappingRepository = customerTranslationMappingRepository;
            _eventPublisher = eventPublisher;
            _productRepository = productRepository;
            _localizedPropertyRepository = localizedPropertyRepository;
        }
        #endregion
        #region Methods
        /// <summary>
        /// Gets all CustomerTranslationMapping
        /// </summary>
        /// <returns>CustomerTranslationMapping</returns>
        public virtual IList<CustomerTranslationMapping> GetAllCustomerTranslationMapping()
        {
            //string key = CUSTOMERTRANSTLATORMAPPING_ALL_KEY;
            //var key = _cacheKeyService.PrepareKeyForShortTermCache(NopCustomerServicesDefaults.customertr, customer, showHidden);

            return _cacheManager.Get(CUSTOMERTRANSTLATORMAPPING_ALL_KEY, () =>
            {
                var query = _customerTranslationMappingRepository.Table;
                return query.ToList();
            });
        }
        /// <summary>
        /// Gets customer translation mapping records
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>customer translation mapping records</returns>
        public virtual IPagedList<CustomerTranslationMapping> GetAllCustomerTranslationListByMappings<T>(T entity, int customerid, int pageIndex = 0, int pageSize = int.MaxValue) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            var query = from sm in _customerTranslationMappingRepository.Table
                        where sm.EntityName == entityName && sm.CustomerId == customerid
                        select sm;
            query = query.OrderBy(p => p.EntityId);
            var translationmapping = new PagedList<CustomerTranslationMapping>(query, pageIndex, pageSize);
            return translationmapping;
        }
        /// <summary>
        /// Gets customer translation mapping records
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>customer translation mapping records</returns>
        public virtual IPagedList<Product> GetAllProductCustomerTranslationListByMappings<T>(T entity, int customerid, string productname = null,
            int languageid = 0, bool showNotTranslatedOnly = false, int pageIndex = 0, int pageSize = int.MaxValue) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            IQueryable<Product> query = null;
            if (showNotTranslatedOnly)
            {
                query = from sm in _customerTranslationMappingRepository.Table
                        join p in _productRepository.Table on sm.EntityId equals p.Id
                        where sm.EntityName == entityName && sm.CustomerId == customerid
                        && sm.IsTranslated == false
                        orderby sm.Id descending
                        select p;

            }
            else
            {
                query = from sm in _customerTranslationMappingRepository.Table
                        join p in _productRepository.Table on sm.EntityId equals p.Id
                        where sm.EntityName == entityName && sm.CustomerId == customerid
                        orderby sm.Id descending
                        select p;
            }

            if (!string.IsNullOrEmpty(productname))
            {
                query = from q in query
                        where q.Name.Contains(productname)
                        from lp in _localizedPropertyRepository.Table
                        .Where(lp => lp.LocaleKeyGroup == entityName && lp.LocaleKey == "Name"
                            && lp.LanguageId == languageid &&
                            lp.LocaleValue.Contains(productname)
                            && lp.EntityId == q.Id)
                        .DefaultIfEmpty()
                        select q;
            }
            query = query.OrderByDescending(p => p.DisplayOrder);
            var translationmapping = new PagedList<Product>(query, pageIndex, pageSize);
            return translationmapping;
        }
        /// <summary>
        /// Gets customer translation mapping records
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>customer translation mapping records</returns>
        public virtual IList<CustomerTranslationMapping> GetAllCustomerTranslationByMappings<T>(T entity) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            var query = from sm in _customerTranslationMappingRepository.Table
                        where sm.EntityId == entityId &&
                        sm.EntityName == entityName
                        select sm;
            var translationmapping = query.ToList();
            return translationmapping;
        }
        public virtual CustomerTranslationMapping GetCustomerTranslatorByMapping<T>(T entity) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            int entityId = entity.Id;
            string entityName = typeof(T).Name;
            var query = from sm in _customerTranslationMappingRepository.Table
                        where sm.EntityId == entityId &&
                        sm.EntityName == entityName
                        select sm;
            //string key = string.Format(CUSTOMERTRANSTLATORMAPPING_BY_ID_KEY, entityId);
            return _cacheManager.Get(CUSTOMERTRANSTLATORMAPPING_BY_ID_KEY, () => query.FirstOrDefault());
        }
        /// <summary>
        /// Inserts a customer translation mapping record
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="customerId">customer id</param>
        /// <param name="entity">Entity</param>
        public virtual void InsertCustomerTranslationMapping<T>(T entity, int customerid, bool Translated) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (customerid == 0)
                throw new ArgumentOutOfRangeException("storeId");

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            var customertranslatorMapping = new CustomerTranslationMapping
            {
                EntityId = entityId,
                EntityName = entityName,
                CustomerId = customerid,
                IsTranslated = Translated
            };

            InsertCustomerTranslation(customertranslatorMapping);
        }

        /// <summary>
        /// Gets a customer attribute 
        /// </summary>
        /// <param name="CustomerTranslationMappingById">CustomerTranslationMapping identifier</param>
        /// <returns>Customer attribute</returns>
        public virtual CustomerTranslationMapping GetCustomerTranslatorById(int customerTranslatorId)
        {
            if (customerTranslatorId == 0)
                return null;

            //string key = string.Format(CUSTOMERTRANSTLATORMAPPING_BY_ID_KEY, customerTranslatorId);
            return _cacheManager.Get(CUSTOMERTRANSTLATORMAPPING_BY_ID_KEY, () => _customerTranslationMappingRepository.GetById(customerTranslatorId));
        }
        /// <summary>
        /// Deletes a customerTranslator
        /// </summary>
        /// <param name="customerTranslator">Customer customerTranslator</param>
        public virtual void DeleteCustomerTranslationMapping<T>(T entity, int id) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            int entityId = entity.Id;
            var customertranslatorMapping = _customerTranslationMappingRepository.GetById(id);

            _customerTranslationMappingRepository.Delete(customertranslatorMapping);

            _cacheManager.RemoveByPrefix(CUSTOMERTRANSTLATORMAPPING_PATTERN_KEY_PREFIX);
            //event notification
            _eventPublisher.EntityDeleted(customertranslatorMapping);
        }
        /// <summary>
        /// Inserts a customer translation Mapping
        /// </summary>
        /// <param name="customertranslatorMapping">Customer Translation Mapping</param>
        public virtual void InsertCustomerTranslation(CustomerTranslationMapping customerMapping)
        {
            if (customerMapping == null)
                throw new ArgumentNullException("customerAttribute");

            _customerTranslationMappingRepository.Insert(customerMapping);

            _cacheManager.RemoveByPrefix(CUSTOMERTRANSTLATORMAPPING_PATTERN_KEY_PREFIX);

            //event notification
            _eventPublisher.EntityInserted(customerMapping);
        }
        /// <summary>
        /// Updates the customer attribute
        /// </summary>
        /// <param name="customerTranslator">Customer attribute</param>
        public virtual void UpdateCustomerTranslator<T>(T entity, int customerid, int id, bool translated) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            if (customerid == 0)
                throw new ArgumentOutOfRangeException("customerid");

            int entityId = entity.Id;
            string entityName = typeof(T).Name;

            var customertranslatorMapping = _customerTranslationMappingRepository.GetById(id);

            customertranslatorMapping.EntityId = entityId;
            customertranslatorMapping.EntityName = entityName;
            customertranslatorMapping.CustomerId = customerid;
            customertranslatorMapping.IsTranslated = translated;
            _customerTranslationMappingRepository.Update(customertranslatorMapping);

            _cacheManager.RemoveByPrefix(CUSTOMERTRANSTLATORMAPPING_PATTERN_KEY_PREFIX );

            //event notification
            _eventPublisher.EntityUpdated(customertranslatorMapping);
        }
        /// <summary>
        /// Find customer identifiers with granted access (mapped to the entity)
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="entity">Wntity</param>
        /// <returns>Store identifiers</returns>
        public virtual int[] GetMappingEntityIds<T>(T entity, int customerId) where T : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException("entity");

            // int entityId = entity.Id;
            string entityName = typeof(T).Name;

            //string key = string.Format(CUTOMERTRANSLATIONMAPPING_BY_ENTITYID_NAME_KEY, entityName);
            return _cacheManager.Get(CUTOMERTRANSLATIONMAPPING_BY_ENTITYID_NAME_KEY, () =>
            {
                var query = from sm in _customerTranslationMappingRepository.Table
                            where sm.EntityName == entityName && sm.CustomerId == customerId
                            select sm.EntityId;
                return query.ToArray();
            });
        }
        #endregion
    }
}
