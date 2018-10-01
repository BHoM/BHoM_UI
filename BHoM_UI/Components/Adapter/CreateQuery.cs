using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using BH.Engine.Reflection;

namespace BH.UI.Components
{
    public class CreateQueryCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.QueryAdapter;

        public override Guid Id { get; protected set; } = new Guid("A4C4D4BA-8FB9-4CE5-802E-46A39B89FE5E");

        public override string Name { get; protected set; } = "CreateQuery";

        public override string Category { get; protected set; } = "Adapter";

        public override string Description { get; protected set; } = "Creates an instance of a selected type of adapter query";

        public override int GroupIndex { get; protected set; } = 2;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateQueryCaller() : base()
        {
            Type queryType = typeof(BH.oM.DataManipulation.Queries.IQuery);
            IEnumerable<MethodBase> methods  = BH.Engine.Reflection.Query.BHoMMethodList().Where(x => queryType.IsAssignableFrom(x.ReturnType)).OrderBy(x => x.Name);
            SetPossibleItems(methods);
        }

        /*************************************/
    }
}
