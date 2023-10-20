using Nop.Core.Domain.Lookups;
using Nop.Core.Domain.News;
using Nop.Services.Caching;
using Nop.Services.Lookups;

namespace Nop.Services.News.Caching
{
    /// <summary>
    /// Represents a news item cache event consumer
    /// </summary>
    public partial class LookupCacheEventConsumer : CacheEventConsumer<Lookup>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        protected override void ClearCache(Lookup entity)
        {
            var prefix = _cacheKeyService.PrepareKeyPrefix(NopLookupDefaults.lookupsPrefixCacheKey, entity);
            RemoveByPrefix(prefix);
        }
    }
}