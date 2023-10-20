using Nop.Core;
using Nop.Core.Domain.Stores;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Domain
{
    public class SyncStatus : BaseEntity, IStoreMappingSupported
    {
        public string TableName { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public bool LimitedToStores { get; set; }
        public bool ForceSync { get; set; }
        public DateTime? ForceSyncDate { get; set; }
    }
}
