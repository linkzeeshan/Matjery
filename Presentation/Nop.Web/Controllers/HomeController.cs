using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Vendors;
using System.Linq;

namespace Nop.Web.Controllers
{
    public partial class HomeController : BasePublicController
    {
        private readonly ICustomerService _customerService;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly ILanguageService _languageService;
        private readonly IGenericAttributeService _genericAttributeService;
        public HomeController(ICustomerService customerService, IVendorService vendorService, IWorkContext workContext, IStoreContext storeContext, IGenericAttributeService genericAttributeService, ILanguageService languageService)
        {
            _customerService = customerService;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _languageService = languageService;
            _vendorService = vendorService;
        }
       
        public virtual IActionResult Index()
        {
            if (TempData["WorkingLanguage"] != null)
            {
                int langID =!string.IsNullOrEmpty(TempData["WorkingLanguage"].ToString())? (int)TempData["WorkingLanguage"]:_storeContext.CurrentStore.DefaultLanguageId;
                if (langID > 0)
                {
                    var currlang = _languageService.GetLanguageById(langID);
                    _workContext.WorkingLanguage = currlang;
                    _genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, NopCustomerDefaults.LanguageIdAttribute, currlang.Id, _storeContext.CurrentStore.Id);
                }
                TempData["WorkingLanguage"] = null;
            }
            if (_customerService.IsAdmin(_workContext.CurrentCustomer) || _customerService.IsVendor(_workContext.CurrentCustomer))
            {
                if (Request.Query["store"].Any())
                {
                    string store = Request.Query["store"].ToString();
                    if (store.ToLower() == "public")
                    {
                        return View();
                    }
                }
                if (_customerService.IsVendor(_workContext.CurrentCustomer))
                {
                    var vendor= _vendorService.GetVendorById(_workContext.CurrentCustomer.VendorId);
                    if (vendor != null)
                    {
                        if (vendor.Active && !vendor.Deleted)
                        {
                            return RedirectToAction("Index", "Home", new { area = "Admin" });
                        }
                        else
                        {
                            return View();
                        }
                    }
                }
                return  RedirectToAction("Index", "Home", new { area ="Admin" });
            }
            else
            {
                return View();
            }
           
        }
    }
}