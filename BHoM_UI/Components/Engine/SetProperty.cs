using BH.oM.Base;
using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Components
{
    public class SetPropertyCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.BHoM_SetProperty;

        public override Guid Id { get; protected set; } = new Guid("A186D4F1-FC80-499B-8BBF-ECDD49BF6E6E");

        public override string Name { get; protected set; } = "SetProperty";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public SetPropertyCaller() : base(typeof(BH.Engine.Reflection.Modify).GetMethod("PropertyValue", new Type[] { typeof(BHoMObject), typeof(string), typeof(object) })) {}


        /*************************************/
    }
}
