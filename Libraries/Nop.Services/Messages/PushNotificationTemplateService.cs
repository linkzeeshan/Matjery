using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Events;
using Nop.Services.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Services.Messages
{
    public partial class PushNotificationTemplateService : IPushNotificationTemplateService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// </remarks>
       // private const string PUSHNOTIFICATIONTEMPLATES_ALL_KEY = "Nop.pushnotificationtemplate.all";
        public static CacheKey PUSHNOTIFICATIONTEMPLATES_ALL_KEY => new CacheKey("Nop.pushnotificationtemplate.all}", CUSTOMERTRANSTLATORMAPPING_ALL_KEY_PREFIX);
        public static string CUSTOMERTRANSTLATORMAPPING_ALL_KEY_PREFIX => "Nop.pushnotificationtemplate.all";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : template name
        /// {1} : store ID
        /// </remarks>
        //private const string PUSHNOTIFICATIONTEMPLATES_BY_NAME_KEY = "Nop.pushnotificationtemplate.name-{0}";

        public static CacheKey PUSHNOTIFICATIONTEMPLATES_BY_NAME_KEY => new CacheKey("Nop.pushnotificationtemplate.name-{0}", CUSTOMERTRANSTLATORMAPPING_ALL_KEY_PREFIX);
        public static string PUSHNOTIFICATIONTEMPLATES_BY_NAME_KEY_PREFIX => "Nop.pushnotificationtemplate";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        //private const string PUSHNOTIFICATIONTEMPLATES_PATTERN_KEY = "Nop.pushnotificationtemplate.";

        public static CacheKey PUSHNOTIFICATIONTEMPLATES_PATTERN_KEY => new CacheKey("Nop.pushnotificationtemplate.name-{0}", PUSHNOTIFICATIONTEMPLATES_PATTERN_KEY_PREFIX);
        public static string PUSHNOTIFICATIONTEMPLATES_PATTERN_KEY_PREFIX => "Nop.pushnotificationtemplate";

        #endregion

        #region Fields

        private readonly IRepository<PushNotificationTemplate> _pushNotificationTemplateRepository;
        private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IStaticCacheManager _cacheManager;

        #endregion

        #region Ctor

        public PushNotificationTemplateService(IStaticCacheManager cacheManager,
            IRepository<StoreMapping> storeMappingRepository,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            IRepository<PushNotificationTemplate> pushNotificationTemplateRepository,
            CatalogSettings catalogSettings,
            IEventPublisher eventPublisher,
            IRepository<LocalizedProperty> localizedPropertyRepository)
        {
            this._cacheManager = cacheManager;
            this._storeMappingRepository = storeMappingRepository;
            this._languageService = languageService;
            this._localizedEntityService = localizedEntityService;
            this._pushNotificationTemplateRepository = pushNotificationTemplateRepository;
            this._catalogSettings = catalogSettings;
            this._eventPublisher = eventPublisher;
            this._localizedPropertyRepository = localizedPropertyRepository;
        }

        #endregion

        #region Methods

        public virtual void DeletePushNotificationTemplate(PushNotificationTemplate pushNotificationTemplate)
        {
            if (pushNotificationTemplate == null)
                throw new ArgumentNullException("pushNotificationTemplate");

            _pushNotificationTemplateRepository.Delete(pushNotificationTemplate);

            _cacheManager.RemoveByPrefix(PUSHNOTIFICATIONTEMPLATES_PATTERN_KEY_PREFIX);

            //event notification
            _eventPublisher.EntityDeleted(pushNotificationTemplate);
        }

        public virtual void InsertPushNotificationTemplate(PushNotificationTemplate pushNotificationTemplate)
        {
            if (pushNotificationTemplate == null)
                throw new ArgumentNullException("pushNotificationTemplate");

            _pushNotificationTemplateRepository.Insert(pushNotificationTemplate);

            _cacheManager.RemoveByPrefix(PUSHNOTIFICATIONTEMPLATES_PATTERN_KEY_PREFIX);

            //event notification
            _eventPublisher.EntityInserted(pushNotificationTemplate);
        }

        public virtual void UpdatePushNotificationTemplate(PushNotificationTemplate pushNotificationTemplate)
        {
            if (pushNotificationTemplate == null)
                throw new ArgumentNullException("pushNotificationTemplate");

            _pushNotificationTemplateRepository.Update(pushNotificationTemplate);

            _cacheManager.RemoveByPrefix(PUSHNOTIFICATIONTEMPLATES_PATTERN_KEY_PREFIX);

            //event notification
            _eventPublisher.EntityUpdated(pushNotificationTemplate);
        }

        public virtual PushNotificationTemplate GetPushNotificationTemplateById(int pushNotificationTemplateId)
        {
            if (pushNotificationTemplateId == 0)
                return null;

            return _pushNotificationTemplateRepository.GetById(pushNotificationTemplateId);
        }

        public virtual PushNotificationTemplate GetPushNotificationTemplateByName(string pushNotificationTemplateName)
        {
            if (string.IsNullOrWhiteSpace(pushNotificationTemplateName))
                throw new ArgumentException("pushNotificationTemplateName");

            //string key = string.Format(PUSHNOTIFICATIONTEMPLATES_BY_NAME_KEY, pushNotificationTemplateName);
            //return _cacheManager.Get(PUSHNOTIFICATIONTEMPLATES_BY_NAME_KEY, () =>
            //{
                var query = _pushNotificationTemplateRepository.Table;
                query = query.Where(t => t.Name == pushNotificationTemplateName);
                query = query.OrderBy(t => t.Id);
                var templates = query.ToList();

                return templates.FirstOrDefault();
            //});

        }

        public  IPagedList<PushNotificationTemplate> GetAllPushNotificationTemplates(string name = "", string title = "",
            int pageIndex = 0, int pageSize = int.MaxValue, bool IsActive = false)
        {
            // string key = string.Format(PUSHNOTIFICATIONTEMPLATES_ALL_KEY);
            var query = _pushNotificationTemplateRepository.Table;

            if (!String.IsNullOrWhiteSpace(name))
            {
                query = from q in query
                        join lpj in (from lp in _localizedPropertyRepository.Table where lp.LocaleKeyGroup == "PushNotificationTemplate" && lp.LocaleKey == "Name" select lp)
                        on q.Id equals lpj.EntityId
                         into join1
                        from j in join1.DefaultIfEmpty()
                        where j.LocaleValue.Contains(name) || q.Name.Contains(name)
                        select q;
            }
            if (!string.IsNullOrEmpty(title))
            {
                title = title.Trim();
                query = query.Where(f => f.Title == title);
            }
            if (IsActive)
            {
                query = query.Where(v => v.IsActive);
            }
            else
            {
                query = query.OrderByDescending(v => v.Name)
                    .ThenBy(v => v.Name);
            }
            var pushNotificationTemplate = new PagedList<PushNotificationTemplate>(query, pageIndex, pageSize);
            return pushNotificationTemplate;
        }
        public virtual IList<PushNotificationTemplate> GetAllPushNotificationTemplates()
        {
            // string key = string.Format(PUSHNOTIFICATIONTEMPLATES_ALL_KEY);
            return _cacheManager.Get(PUSHNOTIFICATIONTEMPLATES_ALL_KEY, () =>
            {
                var query = _pushNotificationTemplateRepository.Table;
                query = query.OrderBy(t => t.Name);

                return query.ToList();
            });
        }

        public virtual PushNotificationTemplate GetActivePushNotificationTemplate(string messageTemplateName)
        {
            var messageTemplate = this.GetPushNotificationTemplateByName(messageTemplateName);

            //no template found
            if (messageTemplate == null)
                return null;

            //ensure it's active
            var isActive = messageTemplate.IsActive;
            if (!isActive)
                return null;

            return messageTemplate;
        }

        #endregion
    }
}
