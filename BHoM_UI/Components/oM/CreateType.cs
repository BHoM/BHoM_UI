using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using BH.oM.UI;
using BH.oM.DataStructure;
using BH.Engine.Reflection;
using BH.Engine.DataStructure;
using BH.Engine.Serialiser;
using System.Windows.Forms;

namespace BH.UI.Components
{
    public class CreateTypeCaller : Templates.Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Type;

        public override Guid Id { get; protected set; } = new Guid("D51978F0-6BEB-4832-9F65-DB00DE85C3B9");

        public override string Category { get; protected set; } = "oM";

        public override string Name { get; protected set; } = "CreateType";

        public override string Description { get; protected set; } = "Creates a selected type definition";

        public Type SelectedType
        {
            get
            {
                return SelectedItem as Type;
            }
            protected set
            {
                SelectedItem = value;
            }
        }


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateTypeCaller() : base()
        {
            List<Type> types = Engine.Reflection.Query.BHoMTypeList();
            types.AddRange(new List<Type> { typeof(object), typeof(Type), typeof(Enum), typeof(bool), typeof(byte), typeof(char), typeof(double), typeof(int), typeof(string) });
            SetPossibleItems(types);

            InputParams = new List<ParamInfo>();
            OutputParams = new List<ParamInfo>() { new ParamInfo { DataType = typeof(Type), Kind = ParamKind.Output, Name = "type", Description = "type definition" } };
        }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        public override object Run(object[] inputs)
        {
            return SelectedType;
        }

        /*************************************/

        public override void AddToMenu(ToolStripDropDown menu)
        {
            if (Selector != null)
                Selector.AddToMenu(menu);
        }

        /*************************************/
    }
}
