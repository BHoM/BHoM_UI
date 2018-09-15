using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Components
{
    public class FromJsonCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.FromJson;

        public override Guid Id { get; protected set; } = new Guid("D5D0EC6D-394B-4781-AC33-C278E1A77009");


        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        public FromJsonCaller() : base(typeof(BH.Engine.Serialiser.Convert).GetMethod("FromJson")) {}

        /*************************************/
    }
}
