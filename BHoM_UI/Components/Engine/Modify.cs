using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace BH.UI.Components
{
    public class ModifyCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Modify;

        public override Guid Id { get; protected set; } = new Guid("2B79756E-C774-470B-8F62-0F20C4AE2DC8");

        public override string Name { get; protected set; } = "Modify";

        public override string Category { get; protected set; } = "Engine";

        public override string Description { get; protected set; } = "Modify a BHoM object";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public ModifyCaller() : base()
        {
            IEnumerable<MethodBase> methods = BH.Engine.Reflection.Query.BHoMMethodList().Where(x => x.DeclaringType.Name == "Modify");
            SetPossibleItems(methods);
        }

        /*************************************/
    }
}
