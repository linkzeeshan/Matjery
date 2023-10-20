using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Models.Catalog
{
    public partial class VendorListModel : BaseNopEntityModel
    {
        public VendorListModel()
        {
            PagingFilteringContext = new CatalogPagingFilteringModel();
            VendorModel = new List<VendorModel>();
        }

        public CatalogPagingFilteringModel PagingFilteringContext { get; set; }

        public IList<VendorModel> VendorModel { get; set; }
    }
}
