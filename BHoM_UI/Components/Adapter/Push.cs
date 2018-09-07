using BH.Adapter;
using BH.oM.Base;
using BH.oM.Reflection;
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
    public class PushCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Push;

        public override Guid Id { get; protected set; } = new Guid("F27E94AD-6939-41AA-B680-094BA245F5C1");


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public PushCaller() : base(typeof(PushCaller).GetMethod("Push")) { }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        public override string Category()
        {
            return "Adapter";
        }

        /*************************************/

        [Description("Push objects to the external software")]
        [Input("adapter", "Adapter to the external software")]
        [Input("objects", "Objects to push")]
        [Input("tag", "Tag to apply to the objects being pushed")]
        [Input("config", "Push config")]
        [Input("active", "Execute the push")]
        [MultiOutput(0, "objects", "Objects that have been pushed(with potentially additional information stored in their CustomData to reflect the push)")]
        [MultiOutput(1, "success", "Define if the push was sucessful")]
        public static Output<List<IObject>, bool> Push(BHoMAdapter adapter, IEnumerable<IObject> objects, string tag = "", Dictionary<string, object> config = null, bool active = false)
        {
            List<IObject> result = new List<IObject>();
            if (active)
                result = adapter.Push(objects, tag, config);

            return BH.Engine.Reflection.Create.Output(result, result.Count() == objects.Count());
        }

        /*************************************/
    }
}
