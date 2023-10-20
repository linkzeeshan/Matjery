using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Messages;
using Nop.Data;
using Nop.Services.Caching.Extensions;
using Nop.Services.Customers;
using Nop.Services.Events;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Campaign service
    /// </summary>
    public partial class CampaignService : ICampaignService
    { /// <summary>
      /// Key for caching
      /// </summary>
      /// <remarks>
      /// {0} : show hidden records?
      /// {1} : category ID
      /// {2} : page index
      /// {3} : page size
      /// {4} : current customer ID
      /// {5} : store ID
      /// </remarks>
        private const string PRODUCAMPAIGNS_ALLBYCAMPAIGNID_KEY = "Nop.productcampaign.allbycampaignid-{0}-{1}-{2}-{3}-{4}";
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductCampaign> _productCampaignRepository;
        private readonly IEmailSender _emailSender;
        private readonly IEventPublisher _eventPublisher;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IRepository<Campaign> _campaignRepository;
        private readonly IStoreContext _storeContext;
        private readonly ITokenizer _tokenizer;
        private readonly IWorkContext _workContext;
        private readonly IStaticCacheManager _cacheManager;

        #endregion

        #region Ctor

        public CampaignService(ICustomerService customerService,
            IRepository<Product> productRepository, IRepository<ProductCampaign> productCampaignRepository,
            IEmailSender emailSender,
            IEventPublisher eventPublisher,
            IMessageTokenProvider messageTokenProvider,
            IQueuedEmailService queuedEmailService,
            IRepository<Campaign> campaignRepository,
            IStoreContext storeContext,
            ITokenizer tokenizer,
            IWorkContext workContext, IStaticCacheManager cacheManager)
        {
            _customerService = customerService;
            _emailSender = emailSender;
            _eventPublisher = eventPublisher;
            _messageTokenProvider = messageTokenProvider;
            _queuedEmailService = queuedEmailService;
            _campaignRepository = campaignRepository;
            _storeContext = storeContext;
            _tokenizer = tokenizer;
            _workContext = workContext;
            _cacheManager = cacheManager;
            _productRepository = productRepository;
            _productCampaignRepository = productCampaignRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts a campaign
        /// </summary>
        /// <param name="campaign">Campaign</param>        
        public virtual void InsertCampaign(Campaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException(nameof(campaign));

            _campaignRepository.Insert(campaign);

            //event notification
            _eventPublisher.EntityInserted(campaign);
        }

        /// <summary>
        /// Updates a campaign
        /// </summary>
        /// <param name="campaign">Campaign</param>
        public virtual void UpdateCampaign(Campaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException(nameof(campaign));

            _campaignRepository.Update(campaign);

            //event notification
            _eventPublisher.EntityUpdated(campaign);
        }

        /// <summary>
        /// Deleted a queued email
        /// </summary>
        /// <param name="campaign">Campaign</param>
        public virtual void DeleteCampaign(Campaign campaign)
        {
            if (campaign == null)
                throw new ArgumentNullException(nameof(campaign));

            _campaignRepository.Delete(campaign);

            //event notification
            _eventPublisher.EntityDeleted(campaign);
        }

        /// <summary>
        /// Gets a campaign by identifier
        /// </summary>
        /// <param name="campaignId">Campaign identifier</param>
        /// <returns>Campaign</returns>
        public virtual Campaign GetCampaignById(int campaignId)
        {
            if (campaignId == 0)
                return null;

            return _campaignRepository.ToCachedGetById(campaignId);
        }
        public virtual IPagedList<ProductCampaign> GetProductCampaignsByCampaignId(int campaignId, int? vendorId = null,
         int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            if (campaignId == 0)
                return new PagedList<ProductCampaign>(new List<ProductCampaign>(), pageIndex, pageSize);

            //string key = string.Format(PRODUCAMPAIGNS_ALLBYCAMPAIGNID_KEY, showHidden, campaignId, pageIndex, pageSize, _workContext.CurrentCustomer.Id);
            //return _cacheManager.Get(key, () =>
            //{
            var query = from pc in _productCampaignRepository.Table
                        join p in _productRepository.Table on pc.ProductId equals p.Id
                        where pc.CampaignId == campaignId && !p.Deleted && (showHidden || p.Published)
                        orderby pc.DisplayOrder
                        select pc;

            if (vendorId.HasValue)
                query = query.Where(pc => pc.VendorId == vendorId.Value);

            if (!showHidden)
            {
                //only distinct categories (group by ID)
                query = from c in query
                        group c by new { c.ProductId,c.DisplayOrder,c.Id}
                        into cGroup
                        orderby cGroup.Key
                        select cGroup.FirstOrDefault();
            }
            else
            {

                query = query.OrderBy(pc => pc.Id).ThenBy(pc => pc.Id);
            }

            //query = query.OrderBy(pc => pc.Id).ThenBy(pc => pc.Id);
            var productCampaigns = new PagedList<ProductCampaign>(query, pageIndex, pageSize);
            return productCampaigns;
            //});
        }

        public virtual ProductCampaign GetProductCampaignById(int productCampaignId)
        {
            if (productCampaignId == 0)
                return null;

            return _productCampaignRepository.GetById(productCampaignId);
        }

        public virtual void InsertProductCampaign(ProductCampaign productCampaign)
        {
            if (productCampaign == null)
                throw new ArgumentNullException("productCampaign");

            _productCampaignRepository.Insert(productCampaign);

            //cache
            //_cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            //_cacheManager.RemoveByPattern(PRODUCTCATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(productCampaign);
        }

        public virtual void UpdateProductCampaign(ProductCampaign productCampaign)
        {
            if (productCampaign == null)
                throw new ArgumentNullException("productCampaign");

            _productCampaignRepository.Update(productCampaign);

            //cache
            //_cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            //_cacheManager.RemoveByPattern(PRODUCTCATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(productCampaign);
        }

        public virtual void DeleteProductCampaign(ProductCampaign productCampaign)
        {
            if (productCampaign == null)
                throw new ArgumentNullException("productCampaign");

            _productCampaignRepository.Delete(productCampaign);

            //cache
            //_cacheManager.RemoveByPattern(CATEGORIES_PATTERN_KEY);
            //_cacheManager.RemoveByPattern(PRODUCTCATEGORIES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(productCampaign);
        }
        /// <summary>
        /// Gets all campaigns
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <returns>Campaigns</returns>
        public virtual IList<Campaign> GetAllCampaigns(int storeId = 0)
        {
            var query = _campaignRepository.Table;

            if (storeId > 0)
            {
                query = query.Where(c => c.StoreId == storeId);
            }

            query = query.OrderBy(c => c.CreatedOnUtc);

            var campaigns = query.ToList();

            return campaigns;
        }
        public virtual IList<Campaign> GetAllCampaigns(int displayAreaId = 0, int storeId = 0, bool showHidden = true)
        {

            var query = _campaignRepository.Table;

            if (storeId > 0)
                query = query.Where(c => c.StoreId == storeId);

            if (!showHidden)
                query = query.Where(c => c.Active);

            if (displayAreaId > 0)
                query = query.Where(c => c.DisplayAreaId == displayAreaId);

            query = query.OrderBy(c => c.CreatedOnUtc);

            var campaigns = query.ToList();

            return campaigns;
        }

        /// <summary>
        /// Sends a campaign to specified emails
        /// </summary>
        /// <param name="campaign">Campaign</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="subscriptions">Subscriptions</param>
        /// <returns>Total emails sent</returns>
        public virtual int SendCampaign(Campaign campaign, EmailAccount emailAccount,
            IEnumerable<NewsLetterSubscription> subscriptions)
        {
            if (campaign == null)
                throw new ArgumentNullException(nameof(campaign));

            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            var totalEmailsSent = 0;

            foreach (var subscription in subscriptions)
            {
                var customer = _customerService.GetCustomerByEmail(subscription.Email);
                //ignore deleted or inactive customers when sending newsletter campaigns
                if (customer != null && (!customer.Active || customer.Deleted))
                    continue;

                var tokens = new List<Token>();
                _messageTokenProvider.AddStoreTokens(tokens, _storeContext.CurrentStore, emailAccount);
                _messageTokenProvider.AddNewsLetterSubscriptionTokens(tokens, subscription);
                if (customer != null)
                    _messageTokenProvider.AddCustomerTokens(tokens, customer);

                var subject = _tokenizer.Replace(campaign.Subject, tokens, false);
                var body = _tokenizer.Replace(campaign.Body, tokens, true);

                var email = new QueuedEmail
                {
                    Priority = QueuedEmailPriority.Low,
                    From = emailAccount.Email,
                    FromName = emailAccount.DisplayName,
                    To = subscription.Email,
                    Subject = subject,
                    Body = body,
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailAccountId = emailAccount.Id,
                    DontSendBeforeDateUtc = campaign.DontSendBeforeDateUtc
                };
                _queuedEmailService.InsertQueuedEmail(email);
                totalEmailsSent++;
            }

            return totalEmailsSent;
        }

        /// <summary>
        /// Sends a campaign to specified email
        /// </summary>
        /// <param name="campaign">Campaign</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="email">Email</param>
        public virtual void SendCampaign(Campaign campaign, EmailAccount emailAccount, string email)
        {
            if (campaign == null)
                throw new ArgumentNullException(nameof(campaign));

            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens, _storeContext.CurrentStore, emailAccount);
            var customer = _customerService.GetCustomerByEmail(email);
            if (customer != null)
                _messageTokenProvider.AddCustomerTokens(tokens, customer);

            var subject = _tokenizer.Replace(campaign.Subject, tokens, false);
            var body = _tokenizer.Replace(campaign.Body, tokens, true);

            _emailSender.SendEmail(emailAccount, subject, body, emailAccount.Email, emailAccount.DisplayName, email, null);
        }

        #endregion
    }
}