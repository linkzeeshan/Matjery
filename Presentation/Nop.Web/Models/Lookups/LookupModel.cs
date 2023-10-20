using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Models.Lookups
{
    public class LookupModel
    {
        public LookupModel()
        {
            AvailableReasons = new List<SelectListItem>();
        }
        public IList<SelectListItem> AvailableReasons { get; set; }
        public int Id { get; set; }
    }
}
