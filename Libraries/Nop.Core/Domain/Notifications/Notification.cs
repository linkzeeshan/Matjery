using Nop.Core.Domain.Localization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Notifications
{
    public partial class Notification : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the Title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }
    }
}
