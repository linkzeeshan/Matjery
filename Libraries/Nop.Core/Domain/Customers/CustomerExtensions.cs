﻿using System;
using System.Linq;

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Customer extensions
    /// </summary>
    public static class CustomerExtensions
    {

        #region Customer role

        /// <summary>
        /// Gets a value indicating whether customer is in a certain customer role
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="customerRoleSystemName">Customer role system name</param>
        /// <param name="onlyActiveCustomerRoles">A value indicating whether we should look only in active customer roles</param>
        /// <returns>Result</returns>
        public static bool IsInCustomerRole(this Customer customer,
            string customerRoleSystemName, bool onlyActiveCustomerRoles = true)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (String.IsNullOrEmpty(customerRoleSystemName))
                throw new ArgumentNullException("customerRoleSystemName");

            var result = customer.CustomerRoles
                .FirstOrDefault(cr => (!onlyActiveCustomerRoles || cr.Active) && (cr.SystemName == customerRoleSystemName)) != null;
            return result;


        }

        /// <summary>
        /// Gets a value indicating whether customer a search engine
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public static bool IsSearchEngineAccount(this Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (!customer.IsSystemAccount || String.IsNullOrEmpty(customer.SystemName))
                return false;

            var result = customer.SystemName.Equals(NopCustomerDefaults.SearchEngineCustomerName, StringComparison.InvariantCultureIgnoreCase);
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether the customer is a built-in record for background tasks
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        public static bool IsBackgroundTaskAccount(this Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (!customer.IsSystemAccount || String.IsNullOrEmpty(customer.SystemName))
                return false;

            var result = customer.SystemName.Equals(NopCustomerDefaults.BackgroundTaskCustomerName, StringComparison.InvariantCultureIgnoreCase);
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customer is administrator
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="onlyActiveCustomerRoles">A value indicating whether we should look only in active customer roles</param>
        /// <returns>Result</returns>
        //public static bool IsAdmin(this Customer customer, bool onlyActiveCustomerRoles = true)
        //{
        //    return IsInCustomerRole(customer, NopCustomerDefaults.AdministratorsRoleName, onlyActiveCustomerRoles);
        //}

        /// <summary>
        /// Gets a value indicating whether customer is a forum moderator
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="onlyActiveCustomerRoles">A value indicating whether we should look only in active customer roles</param>
        /// <returns>Result</returns>
        //public static bool IsForumModerator(this Customer customer, bool onlyActiveCustomerRoles = true)
        //{
        //    return IsInCustomerRole(customer, NopCustomerDefaults.ForumModeratorsRoleName, onlyActiveCustomerRoles);
        //}

        /// <summary>
        /// Gets a value indicating whether customer is registered
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="onlyActiveCustomerRoles">A value indicating whether we should look only in active customer roles</param>
        /// <returns>Result</returns>
        //public static bool IsRegistered(this Customer customer, bool onlyActiveCustomerRoles = true)
        //{
        //    return IsInCustomerRole(customer, NopCustomerDefaults.RegisteredRoleName, onlyActiveCustomerRoles);
        //}

        /// <summary>
        /// Gets a value indicating whether customer is guest
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="onlyActiveCustomerRoles">A value indicating whether we should look only in active customer roles</param>
        /// <returns>Result</returns>
        //public static bool IsGuest(this Customer customer, bool onlyActiveCustomerRoles = true)
        //{
        //    return IsInCustomerRole(customer, NopCustomerDefaults.GuestsRoleName, onlyActiveCustomerRoles);
        //}

        /// <summary>
        /// Gets a value indicating whether customer is vendor
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="onlyActiveCustomerRoles">A value indicating whether we should look only in active customer roles</param>
        /// <returns>Result</returns>
        //public static bool IsVendor(this Customer customer, bool onlyActiveCustomerRoles = true)
        //{
        //    return IsInCustomerRole(customer, NopCustomerDefaults.VendorsRoleName, onlyActiveCustomerRoles);
        //}
        //public static bool IsFoundation(this Customer customer, bool onlyActiveCustomerRoles = true)
        //{
        //    return IsInCustomerRole(customer, NopCustomerDefaults.Foundations, onlyActiveCustomerRoles);
        //}
        //public static bool IsTranslator(this Customer customer, bool onlyActiveCustomerRoles = true)
        //{
        //    return IsInCustomerRole(customer, NopCustomerDefaults.Translator, onlyActiveCustomerRoles);
        //}
        #endregion
        /// <summary>
        /// Gets a value indicating whether customer a search engine
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Result</returns>
        //public static bool IsSearchEngineAccount(this Customer customer)
        //{
        //    if (customer == null)
        //        throw new ArgumentNullException(nameof(customer));

        //    if (!customer.IsSystemAccount || string.IsNullOrEmpty(customer.SystemName))
        //        return false;

        //    var result = customer.SystemName.Equals(NopCustomerDefaults.SearchEngineCustomerName, StringComparison.InvariantCultureIgnoreCase);

        //    return result;
        //}

        ///// <summary>
        ///// Gets a value indicating whether the customer is a built-in record for background tasks
        ///// </summary>
        ///// <param name="customer">Customer</param>
        ///// <returns>Result</returns>
        //public static bool IsBackgroundTaskAccount(this Customer customer)
        //{
        //    if (customer == null)
        //        throw new ArgumentNullException(nameof(customer));

        //    if (!customer.IsSystemAccount || string.IsNullOrEmpty(customer.SystemName))
        //        return false;

        //    var result = customer.SystemName.Equals(NopCustomerDefaults.BackgroundTaskCustomerName, StringComparison.InvariantCultureIgnoreCase);

        //    return result;
        //}

        //public static bool IsFoundation(this Customer customer, bool onlyActiveCustomerRoles = true)
        //{

        //    if (customer == null)
        //        throw new ArgumentNullException(nameof(customer));

        //    if (!customer.IsSystemAccount || string.IsNullOrEmpty(customer.SystemName))
        //        return false;

        //    var result = customer.SystemName.Equals(NopCustomerDefaults.Foundations, StringComparison.InvariantCultureIgnoreCase);

        //    return result;
        //}
        //public static bool IsTranslator(this Customer customer, bool onlyActiveCustomerRoles = true)
        //{
        //    if (customer == null)
        //        throw new ArgumentNullException(nameof(customer));

        //    if (!customer.IsSystemAccount || string.IsNullOrEmpty(customer.SystemName))
        //        return false;

        //    var result = customer.SystemName.Equals(NopCustomerDefaults.Translator, StringComparison.InvariantCultureIgnoreCase);

        //    return result;
        //}
        /// <summary>
        /// Gets a value indicating whether customer is guest
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="onlyActiveCustomerRoles">A value indicating whether we should look only in active customer roles</param>
        /// <returns>Result</returns>
        public static bool IsGuest(this Customer customer, bool onlyActiveCustomerRoles = true)
        {
            return IsInCustomerRole(customer, SystemCustomerRoleNames.Guests, onlyActiveCustomerRoles);
        }
    }
}