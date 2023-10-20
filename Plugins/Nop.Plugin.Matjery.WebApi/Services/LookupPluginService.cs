using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Lookups;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class LookupPluginService : BasePluginService, ILookupPluginService
    {
        public List<Lookup> GetAllLookups(string[] types)
        {
             var lookupList = new List<Lookup>();
            try
            {

                //load by store
                 lookupList = _lookupService.GetAllLookups(types, _workContext.WorkingLanguage.Id);
                
                return lookupList;
            }
            catch (Exception ex)
            {
                throw;
            }

            return lookupList;
        }

    }
}
