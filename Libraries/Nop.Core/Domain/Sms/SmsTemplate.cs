using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;

namespace Nop.Core.Domain.Sms
{
    public class SmsTemplate : BaseEntity, IStoreMappingSupported, ILocalizedEntity
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public bool IsActive { get; set; }
        public bool LimitedToStores { get; set; }
    }
}
