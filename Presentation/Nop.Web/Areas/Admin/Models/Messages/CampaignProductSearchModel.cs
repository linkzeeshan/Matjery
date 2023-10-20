using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Models.Messages
{
    public class CampaignProductSearchModel: BaseSearchModel
    {
        public int campaignId  { get; set; }
    }
}
