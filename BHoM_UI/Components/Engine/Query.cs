using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace BH.UI.Components
{
    public class QueryCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Query;

        public override Guid Id { get; protected set; } = new Guid("2E60079C-3921-4C4F-8C44-7052C85FA36B");

        public override string Name { get; protected set; } = "Query";

        public override string Category { get; protected set; } = "Engine";

        public override string Description { get; protected set; } = "Query information about a BHoM object";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public QueryCaller() : base()
        {
            IEnumerable<MethodBase> methods = BH.Engine.Reflection.Query.BHoMMethodList().Where(x => x.DeclaringType.Name == "Query");
            SetPossibleItems(methods);
        }

        /*************************************/
    }
}
