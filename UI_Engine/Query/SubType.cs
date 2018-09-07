using BH.Engine.Reflection;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        public static Type SubType(this ParamInfo info)
        {
            return info.DataType.UnderlyingType().Type;
        }
    }
}
