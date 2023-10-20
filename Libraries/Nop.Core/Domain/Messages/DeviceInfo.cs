using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Messages
{
    public partial class DeviceInfo : BaseEntity
    {
        public string RegisterationId { get; set; }
        public string Platform { get; set; }
        public string DeviceId { get; set; }
        public string Version { get; set; }
        public string Manufacturer { get; set; }
        public string Serial { get; set; }
        public string Model { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public int CustomerId { get; set; }
    }
}
