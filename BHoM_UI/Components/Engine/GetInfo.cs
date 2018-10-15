using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;

namespace BH.UI.Components
{
    public class GetInfoCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.GetInfo;

        public override Guid Id { get; protected set; } = new Guid("90D428FE-BC49-4944-8E22-C3180FDD6A96");

        public override int GroupIndex { get; protected set; } = 2;

        public override string Description { get; protected set; } = "Get information about the BHoM, a specific dll or method";

        public override string Category { get; protected set; } = "Engine";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public GetInfoCaller() : base(typeof(GetInfoCaller).GetMethod("GetInfo")) { }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        [Description("Execute command in the external software")]
        [Output("information about the current version of the input element (info aboutthe BHoM if no input)")]
        public static string GetInfo()
        {
            return "This is the BHoM";
        }

        /*************************************/
    }
}
