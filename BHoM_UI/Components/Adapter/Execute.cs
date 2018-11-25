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
    public class ExecuteCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Execute;

        public override Guid Id { get; protected set; } = new Guid("D45AD8E8-CF03-464C-BA89-2122F4C6E4FA");

        public override string Category { get; protected set; } = "Adapter";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public ExecuteCaller() : base(typeof(ExecuteCaller).GetMethod("Execute")) { }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        [Description("Execute command in the external software")]
        [Input("adapter", "Adapter to the external software")]
        [Input("command", "Command to run")]
        [Input("parameters", "Parameters of the command")]
        [Input("config", "Execute config")]
        [Input("active", "Execute the command")]
        [Output("Confirms the success of the operation")]
        public static bool Execute(BHoMAdapter adapter, string command, Dictionary<string, object> parameters = null, Dictionary<string, object> config = null, bool active = false)
        {
            if (active)
                return adapter.Execute(command, parameters, config);
            else
                return false;
        }

        /*************************************/
    }
}
