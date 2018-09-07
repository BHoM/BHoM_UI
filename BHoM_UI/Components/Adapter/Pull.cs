using BH.Adapter;
using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using BH.oM.Reflection.Attributes;
using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Components
{
    public class PullCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Pull;

        public override Guid Id { get; protected set; } = new Guid("B25011DD-5F30-4279-B9D9-0F9C169D6685");


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public PullCaller() : base(typeof(PullCaller).GetMethod("Pull")) { }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        public override string Category()
        {
            return "Adapter";
        }

        /*************************************/

        [Description("Pull objects from the external software")]
        [Input("adapter", "Adapter to the external software")]
        [Input("query", "Filter on the objects to pull (default: get all)")]
        [Input("config", "Pull config")]
        [Input("active", "Execute the pull")]
        [Output("Objects pulled")]
        public static IEnumerable<object> Pull(BHoMAdapter adapter, IQuery query = null, Dictionary<string, object> config = null, bool active = false)
        {
            if (query == null)
                query = new FilterQuery();

            if (active)
                return adapter.Pull(query, config);
            else
                return new List<object>();
        }

        /*************************************/
    }
}
