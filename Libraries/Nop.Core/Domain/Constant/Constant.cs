using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Constant
{
    public partial class Constant : BaseEntity
    {
        public string GroupName { get; set; }
        public string Code { get; set; }
        public string ArabicName { get; set; }
        public string EnglishName { get; set; }
        public string EmirateId { get; set; }
        public string CityId { get; set; }
        public string CountryGroup { get; set; }
        public int? PublicPoBoxNumber { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public bool IsDeleted { get; set; }
    }
}
