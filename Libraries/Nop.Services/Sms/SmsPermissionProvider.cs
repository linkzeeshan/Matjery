using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Services
{
    public partial class SmsPermissionProvider : IPermissionProvider
    {
        public static readonly PermissionRecord ManageSmsTemplate = new PermissionRecord { Name = "Admin area. Manage SMS Templates", SystemName = "ManageSmsTemplates", Category = "Content Management" };
        public static readonly PermissionRecord ManageSmsQueue = new PermissionRecord { Name = "Admin area. Manage Sms Queue", SystemName = "ManageSmsQueue", Category = "Configuration" };

        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                ManageSmsTemplate,
                ManageSmsQueue
            };
        }

        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            //uncomment code below in order to give appropriate permissions to admin by default
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        ManageSmsTemplate,
                        ManageSmsQueue

                    }
                )
            };
        }
    }
}