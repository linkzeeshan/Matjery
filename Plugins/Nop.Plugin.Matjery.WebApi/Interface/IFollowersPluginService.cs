using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Interface
{
    public interface IFollowersPluginService
    {
        IEnumerable<object> GetAllFollowedVendors();
    }
}
