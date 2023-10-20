using Microsoft.AspNetCore.Mvc;
using Nop.Web.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Customer;
using System.Linq;

namespace Nop.Web.Components
{
    public class CustomerNavigationViewComponent : NopViewComponent
    {
        private readonly ICustomerModelFactory _customerModelFactory;

        public CustomerNavigationViewComponent(ICustomerModelFactory customerModelFactory)
        {
            _customerModelFactory = customerModelFactory;
        }

        public IViewComponentResult Invoke(int selectedTabId = 0)
        {
            var model = _customerModelFactory.PrepareCustomerNavigationModel(selectedTabId);
            if(model.SelectedTab!= CustomerNavigationEnum.Info)
            {
                CustomerNavigationItemModel  itemToRemove = model.CustomerNavigationItems.FirstOrDefault(item => item.RouteName == "DeleteAccount");
                if (itemToRemove != null)
                {
                    model.CustomerNavigationItems.Remove(itemToRemove);
                }
            }
            return View(model);
        }
    }
}
