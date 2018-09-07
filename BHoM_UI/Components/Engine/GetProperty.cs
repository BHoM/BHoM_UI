using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Components
{
    public class GetPropertyCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.BHoM_GetProperty;

        public override Guid Id { get; protected set; } = new Guid("C0BCB684-80E5-4A67-BF0E-6B8C2C917312");


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public GetPropertyCaller() : base(typeof(BH.Engine.Reflection.Query).GetMethod("PropertyValue")) {}


        /*************************************/
        /**** Methods Override            ****/
        /*************************************/

        public override string Name()
        {
            return "GetProperty";
        }
    }
}
