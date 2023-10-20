using Nop.Web.Framework.Models;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public partial class LookupResult : BaseNopModel
    {
       
        public int Id { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public int Sequence { get; set; }
    }
}