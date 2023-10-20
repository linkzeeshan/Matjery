using Nop.Plugin.Matjery.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Interface
{
    public interface IAddressPluginService
    {
        List<AddressResult> GetAddresses();
        AddressResult GetAddress(int addressId);
        bool AddressAdd(ParamsModel.AddressParamsModel model);
        bool UpdateAddress(ParamsModel.AddressParamsModel model);
        bool AddressDelete(ParamsModel.AddressParamsModel model);

    }
}
