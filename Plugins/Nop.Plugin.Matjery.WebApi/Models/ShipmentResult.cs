using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public class ShipmentResult
    {
        public string AdminComment { get; set; }
        public bool CanDeliver { get; set; }
        public bool CanShip { get; set; }
        public string DeliveryDate { get; set; }
        public DateTime? DeliveryDateUtc { get; set; }
        public int Id { get; set; }
        public string ShippedDate { get; set; }
        public DateTime? ShippedDateUtc { get; set; }
        public string TrackingNumber { get; set; }
    }
}
