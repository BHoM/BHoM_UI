using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Components
{
    public class ToJsonCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.ToJson;

        public override Guid Id { get; protected set; } = new Guid("FE8024D0-6DB7-46FE-8785-75B25267FBE6");

        public override int GroupIndex { get; protected set; } = 3;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public ToJsonCaller() : base(typeof(BH.Engine.Serialiser.Convert).GetMethod("ToJson")) {}


        /*************************************/
    }
}
