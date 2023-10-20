namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Customer registered event
    /// </summary>
    public class CustomerRegisteredEvent
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="customer">customer</param>
        public CustomerRegisteredEvent(Customer customer, RegistrationMethod registrationMethod = RegistrationMethod.Web,
            bool isAproved = true)
        {
            this.Customer = customer;
            this.IsAproved = isAproved;
            this.RegistrationMethod = registrationMethod;
        }

        /// <summary>
        /// Customer
        /// </summary>
        public Customer Customer
        {
            get;private set;
        }
        public bool IsAproved { get; set; }
        public RegistrationMethod RegistrationMethod { get; set; }
    }
}