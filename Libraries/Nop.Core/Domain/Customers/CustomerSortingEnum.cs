using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Customers
{
    public enum CustomerSortingEnum
    {
        /// <summary>
        /// Position (display order)
        /// </summary>
        Position = 0,
        /// <summary>
        /// Name: A to Z
        /// </summary>
        UserNameAsc = 5,
        /// <summary>
        /// Name: Z to A
        /// </summary>
        UserNameDesc = 6,
        /// Name: A to Z
        /// </summary>
        NameAsc = 7,
        /// <summary>
        /// Name: Z to A
        /// </summary>
        NameDesc = 8,
        /// <summary>
        /// Email: A to Z
        /// </summary>
        EmailAsc = 10,
        /// <summary>
        /// Email: Z to A
        EmailDesc = 11,
        /// <summary>
        ///creation date Asc
        /// </summary>
        CreatedOnAsc = 15,
        /// <summary>
        ///  creation date Desc
        /// </summary>
        CreatedOnDesc = 16,
        /// <summary>
        /// Active Asc
        /// </summary>
        StatusAsc = 17,
        /// <summary>
        /// Active Desc
        /// </summary>
        StatusDesc = 18,
        /// <summary>
        /// Phone Asc
        /// </summary>
        /// 
        PhoneAsc = 19,
        /// <summary>
        ///  Phone Desc
        /// </summary>
        PhoneDesc = 20,

    }
}
