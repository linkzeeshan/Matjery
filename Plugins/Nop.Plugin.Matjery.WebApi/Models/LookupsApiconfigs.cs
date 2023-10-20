using System;
using System.Collections.Generic;
using System.Text;
using static Nop.Plugin.Matjery.WebApi.Models.ParamsModel;

namespace Nop.Plugin.Matjery.WebApi.Models
{
    public class LookupsApiconfigs
    {
        public LookupsApiconfigs()
        {
            MatjeryLookupModel = new List<MatjeryLookupModel>();
        }
        public  List<MatjeryLookupModel> MatjeryLookupModel { get; set; }

    }

    public class TypesModel
    {
        public List<string> types { get; set; }
    }
    public class MatjeryLookupModelGroup
    {
        public string Type { get; set; }
        public MatjeryLookupModel matjeryLookupModel { get; set; }
    }
}
