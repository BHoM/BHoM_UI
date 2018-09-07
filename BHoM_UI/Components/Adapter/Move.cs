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
    public class MoveCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Move;

        public override Guid Id { get; protected set; } = new Guid("6D2C7F5B-7F64-47C8-AB69-424E5301582F");


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public MoveCaller() : base(typeof(MoveCaller).GetMethod("Move")) { }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        public override string Category()
        {
            return "Adapter";
        }

        /*************************************/

        [Description("Copy objects from a source adapter to a target adapter")]
        [Input("source", "Adapter the data is copied from")]
        [Input("target", "Adapter the data is copied to")]
        [Input("query", "Filter on the objects to pull (default: get all)")]
        [Input("config", "Move config")]
        [Input("active", "Execute the move")]
        [Output("Confirms the success of the operation")]
        public static bool Move(BHoMAdapter source, BHoMAdapter target, IQuery query = null, Dictionary<string, object> config = null, bool active = false)
        {
            if (query == null)
                query = new FilterQuery();

            if (active)
                return source.PullTo(target, query, config);
            else
                return false;
        }

        /*************************************/
    }
}
