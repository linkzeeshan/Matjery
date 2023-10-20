using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Matjery.WebApi.Models
{
     public class PasswordComplexity
    {

        /// <summary>
        /// Gets or sets a minimum password length
        /// </summary>
        public int PasswordMinLength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether password are have least one lowercase
        /// </summary>
        public bool PasswordRequireLowercase { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether password are have least one uppercase
        /// </summary>
        public bool PasswordRequireUppercase { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether password are have least one non alphanumeric character
        /// </summary>
        public bool PasswordRequireNonAlphanumeric { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether password are have least one digit
        /// </summary>
        public bool PasswordRequireDigit { get; set; }

        public string RegExpUppercase { get; set; }
        public string RegExpLowercase { get; set; }
        public string RegExpDigit { get; set; }
        public string RegExpNonAlphanumeric { get; set; }


    }
}
