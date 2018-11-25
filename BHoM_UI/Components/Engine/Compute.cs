using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace BH.UI.Components
{
    public class ComputeCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Compute;

        public override Guid Id { get; protected set; } = new Guid("A4EBE086-E659-4273-940B-98FD9BD73436");

        public override string Name { get; protected set; } = "Compute";

        public override string Category { get; protected set; } = "Engine";

        public override string Description { get; protected set; } = "Run a computationally intensive calculations";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public ComputeCaller() : base()
        {
            IEnumerable<MethodBase> methods = BH.Engine.Reflection.Query.BHoMMethodList().Where(x => x.DeclaringType.Name == "Compute");
            SetPossibleItems(methods);
        }

        /*************************************/
    }
}
