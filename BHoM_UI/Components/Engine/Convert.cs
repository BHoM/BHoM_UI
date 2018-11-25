using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace BH.UI.Components
{
    public class ConvertCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Convert;

        public override Guid Id { get; protected set; } = new Guid("DBB544EB-1EDC-4EF0-A935-EA92FF989CF7");

        public override string Name { get; protected set; } = "Convert";

        public override string Category { get; protected set; } = "Engine";

        public override string Description { get; protected set; } = "Convert to and from a BHoM object";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public ConvertCaller() : base()
        {
            IEnumerable<MethodBase> methods = BH.Engine.Reflection.Query.BHoMMethodList().Where(x => x.DeclaringType.Name == "Convert");
            SetPossibleItems(methods);
        }

        /*************************************/
    }
}
