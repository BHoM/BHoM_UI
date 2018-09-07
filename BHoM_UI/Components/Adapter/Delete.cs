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
    public class DeleteCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Delete;

        public override Guid Id { get; protected set; } = new Guid("BF39598E-A021-4C52-8D65-20BC491B0BBD");


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public DeleteCaller() : base(typeof(DeleteCaller).GetMethod("Delete")) { }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        public override string Category()
        {
            return "Adapter";
        }

        /*************************************/

        [Description("Delete objects in the external software")]
        [Input("adapter", "Adapter to the external software")]
        [Input("filter", "Filters the objects to be deleted")]
        [Input("config", "Delete config")]
        [Input("active", "Execute the delete")]
        [Output("#deleted", "Number of objects that have been deleted")]
        public static int Delete(BHoMAdapter adapter, FilterQuery filter = null, Dictionary<string, object> config = null, bool active = false)
        {
            if (filter == null)
                filter = new FilterQuery();

            if (active)
                return adapter.Delete(filter, config);
            else
                return 0;
        }

        /*************************************/
    }
}
