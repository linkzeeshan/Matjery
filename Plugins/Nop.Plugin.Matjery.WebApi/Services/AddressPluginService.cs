using Nop.Core.Domain.Common;
using Nop.Plugin.Matjery.WebApi.Interface;
using Nop.Plugin.Matjery.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Services
{
    public class AddressPluginService : BasePluginService, IAddressPluginService
    {
        public bool AddressAdd(ParamsModel.AddressParamsModel model)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                throw new ApplicationException("Not allowed");

            var customer = _workContext.CurrentCustomer;

            var address = new Address();
            address.Address1 = model.Address1;
            address.Address2 = model.Address2;
            address.FirstName = model.FirstName;
            address.LastName = model.LastName;
            address.City = model.City;
            address.Area = model.Area;
            address.PhoneNumber = model.PhoneNumber;
            address.StateProvinceId = model.StateProvinceId;
            address.CountryId = model.CountryId;
            address.Email = _workContext.CurrentCustomer.Email;
            address.CreatedOnUtc = DateTime.UtcNow;
            address.IsDefault = model.IsDefault;

            //some validation
            if (address.CountryId == 0)
                address.CountryId = null;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;
            if (model.IsDefault)
            {
                var addresss = _customerService.GetAddressesByCustomerId(_workContext.CurrentCustomer.Id).ToList();
                foreach (var add in addresss)
                {
                    add.IsDefault = false;
                    _addressService.UpdateAddress(add);
                }
            }
            _addressService.InsertAddress(address);
            _customerService.InsertCustomerAddress(_workContext.CurrentCustomer, address);
            if (customer.BillingAddressId == null || customer.BillingAddressId == 0)
            {
                customer.BillingAddressId = address.Id;
                customer.ShippingAddressId = address.Id;
                _customerService.UpdateCustomer(customer);
            }
            return true;
          
        }

        public bool AddressDelete(ParamsModel.AddressParamsModel model)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                throw new ApplicationException("Not allowed");

          
            var customer = _workContext.CurrentCustomer;
           
            //if (model.Id == customer.BillingAddressId)
            //{
            //    return false;
            //}

                //find address (ensure that it belongs to the current customer)
                //var address = customer.Addresses.FirstOrDefault(a => a.Id == model.Id);

            var address = _customerService.GetCustomerAddress(_workContext.CurrentCustomer.Id, model.Id);

            if (address.Id==customer.BillingAddressId)
            {
                var newBillingAddRef = _customerService.GetAddressesByCustomerId(_workContext.CurrentCustomer.Id)
                    .Where(x => x.Id != address.Id).OrderByDescending(o => o.CreatedOnUtc).FirstOrDefault();

                if (newBillingAddRef != null)
                {
                    customer.BillingAddressId = newBillingAddRef.Id;
                    customer.ShippingAddressId = newBillingAddRef.Id;
                    _customerService.UpdateCustomer(customer);
                }
                else
                {
                    customer.BillingAddressId = null;
                    customer.ShippingAddressId = null;
                    _customerService.UpdateCustomer(customer);
                }
            }

            if (address != null)
            {
                _customerService.RemoveCustomerAddress(customer,address);
                _customerService.UpdateCustomer(customer);
                //now delete the address record
                _addressService.DeleteAddress(address);
            }
            return true;
        }

        public AddressResult GetAddress(int addressId)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                throw new ApplicationException("Not allowed");

            var address = _customerService.GetCustomerAddress(_workContext.CurrentCustomer.Id, addressId);

            if (address == null)
                //address is not found
                throw new  ArgumentException("Not Found");

            var addressResult = new AddressResult()
            {
                Id = address.Id,
                FirstName = address.FirstName,
                LastName = address.LastName,
                Company = address.Company,
                Address1 = address.Address1,
                Address2 = address.Address2,
                City = address.City,
                Area = address.Area,
                ZipPostalCode = address.ZipPostalCode,
                StateProvinceId = address.StateProvinceId ?? 0,
                IsDefault=address.IsDefault==true?address.IsDefault:false
            };
            addressResult.CountryId = address.CountryId ?? 0;
            if (address.CountryId != null)
            {
                var country = _countryService.GetCountryById((int)address.CountryId);
                addressResult.CountryName = _localizationService.GetLocalized(country, c => c.Name);

            }
            if (address.StateProvinceId != null)
            {
                var state = _stateProvinceService.GetStateProvinceById((int)address.StateProvinceId);
                addressResult.StateProvinceName = _localizationService.GetLocalized(state, x => x.Name);
            }
            addressResult.PhoneNumber = address.PhoneNumber == "NULL" ? null : address.PhoneNumber;
            addressResult.FaxNumber = address.FaxNumber;
            return addressResult;
        }
      
        public List<AddressResult> GetAddresses()
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                throw new ApplicationException("Not allowed");

            var customer = _workContext.CurrentCustomer;

            var addresss = _customerService.GetAddressesByCustomerId(_workContext.CurrentCustomer.Id);

            List<AddressResult> addressResults = new List<AddressResult>();
            foreach (Address address in addresss)
            {
                var addressResult = new AddressResult()
                {
                    Id = address.Id,
                    FirstName = address.FirstName,
                    LastName = address.LastName,
                    Company = address.Company,
                    Address1 = address.Address1,
                    Address2 = address.Address2,
                    Area = address.Area,
                    City = address.City,
                    ZipPostalCode = address.ZipPostalCode,
                    StateProvinceId = address.StateProvinceId ?? 0,
                    IsDefault=address.IsDefault==true? address.IsDefault:false
                };
                addressResult.CountryId = address.CountryId ?? 0;
                if (address.CountryId != null)
                {
                    var country = _countryService.GetCountryById((int)address.CountryId);

                    addressResult.CountryName = _localizationService.GetLocalized(country, c => c.Name);
                }
                if (address.StateProvinceId != null)
                {
                    var state = _stateProvinceService.GetStateProvinceById((int)address.StateProvinceId);

                    addressResult.StateProvinceName = _localizationService.GetLocalized(state, x => x.Name);
                }
                addressResult.PhoneNumber = address.PhoneNumber;
                addressResult.FaxNumber = address.FaxNumber;
                //if (customer != null)
                //{
                //    if (address != null)
                //        addressResult.IsDefault = customer.BillingAddressId == address.Id ? true : false;
                //}
                addressResults.Add(addressResult);

            }
            return addressResults;
        }

        public bool UpdateAddress(ParamsModel.AddressParamsModel model)
        {
            if (!_customerService.IsRegistered(_workContext.CurrentCustomer))
                throw new ApplicationException("Not allowed");

            var customer = _workContext.CurrentCustomer;
            //find address (ensure that it belongs to the current customer)
            //var address = customer.Addresses.FirstOrDefault(a => a.Id == model.Id);
            var address = _customerService.GetCustomerAddress(_workContext.CurrentCustomer.Id, model.Id);


            if (address == null)
                throw new ArgumentException("Not found");
            address.Address1 = model.Address1;
            address.Address2 = model.Address2;
            address.FirstName = model.FirstName;
            address.LastName = model.LastName;
            address.City = model.City;
            address.Area = model.Area;
            address.PhoneNumber = model.PhoneNumber;
            address.StateProvinceId = model.StateProvinceId;
            address.CountryId = model.CountryId;
           

            if (model.IsDefault && address.IsDefault==false)
            {
                var addresss = _customerService.GetAddressesByCustomerId(_workContext.CurrentCustomer.Id).ToList();
                foreach (var add in addresss)
                {
                    add.IsDefault = false;
                    _addressService.UpdateAddress(add);
                }
            }
            address.IsDefault = model.IsDefault;
            _addressService.UpdateAddress(address);
            //if (!model.IsDefault)
            //{
            //    var Newdefaddress = _customerService.GetAddressesByCustomerId(_workContext.CurrentCustomer.Id)
            //        .Where(x => x.Id != address.Id).OrderByDescending(o=>o.CreatedOnUtc).FirstOrDefault();

            //    customer.BillingAddressId = Newdefaddress.Id;
            //    customer.ShippingAddressId = Newdefaddress.Id;
            //    _customerService.UpdateCustomer(customer);
            //}
            if ((customer.BillingAddressId == null || customer.BillingAddressId == 0))
            {
                customer.BillingAddressId = address.Id;
                customer.ShippingAddressId = address.Id;
                _customerService.UpdateCustomer(customer);
            }
            return true;
        }
    }
}
