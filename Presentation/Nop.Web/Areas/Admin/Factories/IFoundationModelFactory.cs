using Nop.Core.Domain.Foundations;
using Nop.Web.Areas.Admin.Models.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Areas.Admin.Factories
{
    public interface IFoundationModelFactory
    {

        FoundationSearchModel PrepareFoundationSearchModel(FoundationSearchModel searchModel);

        FoundationListModel PrepareFoundationListModel(FoundationSearchModel searchModel);

        void UpdateLocales(Foundation vendor, FoundationModel model);

        FoundationModel PrepareFoundationModel(FoundationModel model, Foundation foundation, bool excludeProperties = false);
    }
}
