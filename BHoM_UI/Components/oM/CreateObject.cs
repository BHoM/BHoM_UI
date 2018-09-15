using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace BH.UI.Components
{
    public class CreateObjectCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.CreateBHoM;

        public override Guid Id { get; protected set; } = new Guid("76221701-C5E7-4A93-8A2B-D34E77ED9CC1");

        public override string Name { get; protected set; } = "CreateObject";

        public override string Category { get; protected set; } = "oM";

        public override string Description { get; protected set; } = "Creates an instance of a selected type of BHoM object";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateObjectCaller() : base()
        {
            IEnumerable<MethodBase> methods = BH.Engine.Reflection.Query.BHoMMethodList().Where(x => x.DeclaringType.Name == "Create");
            SetPossibleItems(methods);
        }

            /*************************************/
        }
}
