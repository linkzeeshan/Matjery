using Nop.Core.Caching;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Lookups
{
    public static partial class NopLookupDefaults
    {
        #region Caching defaults
        /// <summary>
        /// Key for number of lookup comments
        /// </summary>
        /// <remarks>
        /// {0} : lookup item ID
        /// </remarks>
        public static CacheKey lookupCommentsNumberCacheKey => new CacheKey("Nop.lookup-{0}", lookupsPrefixCacheKey);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : lookup item ID
        /// </remarks>
        public static string lookupsPrefixCacheKey => "Nop.lookup-{0}";
        #endregion
    }
}
