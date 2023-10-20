using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Lookups
{
    public partial class Lookup : BaseEntity
    {
        /// <summary>
        /// Type of lookup
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Type value 
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// Language id
        /// </summary>
        public int LanguageId { get; set; }
        /// <summary>
        /// sorting order
        /// </summary>
        public int Sequence { get; set; }
        
        public bool IsActive { get; set; }
    }
}
