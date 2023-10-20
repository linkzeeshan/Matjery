using Nop.Core.Domain.BlackPoints;
using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Models.Blackpoint
{

    public partial class BlackPointModel : BaseNopEntityModel
    {
        public BlackPointModel()
        {
        }
        //[NopResourceDisplayName("blackpoint.comment")]
        public string Comment { get; set; }
        public bool SuccessfullyAdded { get; set; }
        public string Result { get; set; }
        public int VendorOrCustomerId { get; set; }
        public int? OrderId { get; set; }
        public string Imgname { get; set; }
        public BlackPointTypeEnum BlackPointType { get; set; }
    }
}
