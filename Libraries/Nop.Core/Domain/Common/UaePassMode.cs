using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Nop.Core.Domain.Common
{
    public enum UaePassMode
    {
        [Description("web")]
        web = 1,
        [Description("Mobile")]
        mobile = 2
    }
}
