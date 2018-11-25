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
    public class UpdatePropertyCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.UpdateProperty;

        public override Guid Id { get; protected set; } = new Guid("33F6744B-AB9C-40B8-8606-479C6E10C2CC");

        public override string Category { get; protected set; } = "Adapter";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public UpdatePropertyCaller() : base(typeof(UpdatePropertyCaller).GetMethod("UpdateProperty")) { }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        [Description("Update a specific property of objects from the external software")]
        [Input("adapter", "Adapter to the external software")]
        [Input("property", "Name of the property to update values from")]
        [Input("newValue", "New value to assign to the property")]
        [Input("filter", "Filters the objects to be updated")]
        [Input("config", "UpdateProperty config")]
        [Input("active", "Execute the update")]
        [Output("#updated", "Number of objects that have been updated")]
        public static int UpdateProperty(BHoMAdapter adapter, string property, object newValue, FilterQuery filter = null, Dictionary<string, object> config = null, bool active = false)
        {
            if (filter == null)
                filter = new FilterQuery();

            if (active)
                return adapter.UpdateProperty(filter, property, newValue, config);
            else
                return 0;
        }

        /*************************************/
    }
}
