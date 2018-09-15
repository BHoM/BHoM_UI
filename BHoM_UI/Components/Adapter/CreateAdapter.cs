using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using BH.Adapter;

namespace BH.UI.Components
{
    public class CreateAdapterCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Adapter;

        public override Guid Id { get; protected set; } = new Guid("DD286CB5-2BC6-4C4A-AAC5-542D1D0954B5");

        public override string Name { get; protected set; } = "CreateAdapter";

        public override string Category { get; protected set; } = "Adapter";

        public override string Description { get; protected set; } = "Creates an instance of a selected type of Adapter";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateAdapterCaller() : base()
        {
            Type adapterType = typeof(BHoMAdapter);
            IEnumerable<MethodBase> methods = BH.Engine.Reflection.Query.AdapterTypeList().Where(x => x.IsSubclassOf(adapterType)).OrderBy(x => x.Name).SelectMany(x => x.GetConstructors());
            SetPossibleItems(methods);
        }

        /*************************************/
    }
}
