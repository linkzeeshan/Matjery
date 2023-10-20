using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Messages;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Campaign service
    /// </summary>
    public partial interface ICampaignService
    {
        /// <summary>
        /// Inserts a campaign
        /// </summary>
        /// <param name="campaign">Campaign</param>        
        void InsertCampaign(Campaign campaign);

        /// <summary>
        /// Updates a campaign
        /// </summary>
        /// <param name="campaign">Campaign</param>
        void UpdateCampaign(Campaign campaign);

        /// <summary>
        /// Deleted a queued email
        /// </summary>
        /// <param name="campaign">Campaign</param>
        void DeleteCampaign(Campaign campaign);

        /// <summary>
        /// Gets a campaign by identifier
        /// </summary>
        /// <param name="campaignId">Campaign identifier</param>
        /// <returns>Campaign</returns>
        Campaign GetCampaignById(int campaignId);
        IPagedList<ProductCampaign> GetProductCampaignsByCampaignId(int campaignId, int? vendorId = null, int pageIndex = 0,
           int pageSize = int.MaxValue, bool showHidden = false);
        ProductCampaign GetProductCampaignById(int productCampaignId);
        void InsertProductCampaign(ProductCampaign productCampaign);
        void UpdateProductCampaign(ProductCampaign productCampaign);
        void DeleteProductCampaign(ProductCampaign productCampaign);

        /// <summary>
        /// Gets all campaigns
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <returns>Campaigns</returns>
        IList<Campaign> GetAllCampaigns(int storeId = 0);
        IList<Campaign> GetAllCampaigns(int displayAreaId = 0, int storeId = 0, bool showHidden = true);

        /// <summary>
        /// Sends a campaign to specified emails
        /// </summary>
        /// <param name="campaign">Campaign</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="subscriptions">Subscriptions</param>
        /// <returns>Total emails sent</returns>
        int SendCampaign(Campaign campaign, EmailAccount emailAccount,
            IEnumerable<NewsLetterSubscription> subscriptions);

        /// <summary>
        /// Sends a campaign to specified email
        /// </summary>
        /// <param name="campaign">Campaign</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="email">Email</param>
        void SendCampaign(Campaign campaign, EmailAccount emailAccount, string email);
    }
}
