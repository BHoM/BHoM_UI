using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.UI
{
    public class SearchItem : BHoMObject
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public Type CallerType { get; set; } = null;

        public object Item { get; set; } = "";

        public Bitmap Icon { get; set; } = null;

        public string Text { get; set; } = "";


        /***************************************************/
    }
}
