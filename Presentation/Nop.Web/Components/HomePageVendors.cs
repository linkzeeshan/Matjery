using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Components
{
    public class HomePageVendorsViewComponent : NopViewComponent
    {
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IVendorModelFactory _vendorModelFactory;
        private readonly IWorkContext _workcontext;
        private readonly ICustomerService _customerService;
        public HomePageVendorsViewComponent(ICatalogModelFactory catalogModelFactory,
            ICustomerService customerService,
            IVendorModelFactory vendorModelFactory, 
            IWorkContext workcontext)
        {
            _catalogModelFactory = catalogModelFactory;
            _vendorModelFactory = vendorModelFactory;
            _workcontext = workcontext;
            _customerService = customerService;
        }
        public IViewComponentResult Invoke(bool? displayEntities)
            {
            if (_customerService.IsRegistered(_workcontext.CurrentCustomer) && displayEntities==false)
            {
                //var model = _catalogModelFactory.PrepareVendor(displayEntities,10);
                //return View(model);
                CatalogPagingFilteringModel cmd = new CatalogPagingFilteringModel();
                cmd.PageSize = 10;
                var model = _vendorModelFactory.PrepareFollowedVendor(cmd);
                var Addistional = _catalogModelFactory.PrepareVendor(displayEntities, 10 - model.VendorModel.Count);
                model.VendorModel = model.VendorModel.Union(Addistional).ToList();
                return View(model.VendorModel);
            }
            else
            {
                var model = _catalogModelFactory.PrepareVendor(displayEntities);
                return View(model);
            }
        }
    }
}
