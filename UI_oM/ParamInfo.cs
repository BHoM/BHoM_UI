using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.UI
{
    public class ParamInfo : BHoMObject
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public Type DataType { get; set; } = null;

        public string Description { get; set; } = "";

        public ParamKind Kind { get; set; } = ParamKind.Unknown;

        public bool HasDefaultValue { get; set; } = false;

        public object DefaultValue { get; set; } = null;


        /***************************************************/
    }
}
