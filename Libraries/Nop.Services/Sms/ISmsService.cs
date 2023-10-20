using Nop.Core.Domain.Customers;

namespace Nop.Services
{
    public interface ISmsService
    {
        int SendSms(string message, Customer customer);
    }
}
