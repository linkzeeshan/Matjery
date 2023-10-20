using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Models.Customer
{
    public class DeleteCustomerAccountModel
    {
        public DeleteCustomerAccountModel()
        {
            AvailableReasons = new List<SelectListItem>();
        }
        public int ReasonId { get; set; }
        public string Comments { get; set; }

        public bool IsDeleted { get; set; } = false;
        public IList<SelectListItem> AvailableReasons { get; set; }
        public IList<string> TermsnCondition { get; set; }
    }
}
