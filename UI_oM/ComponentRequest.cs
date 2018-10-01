using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.UI
{
    public class ComponentRequest : BHoMObject
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public Type CallerType { get; set; } = null;

        public object SelectedItem { get; set; } = null;

        /***************************************************/
    }
}
