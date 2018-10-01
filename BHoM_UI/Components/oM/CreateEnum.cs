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
    public class CreateEnumCaller : Templates.MultiChoiceCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.BHoM_Enum;

        public override Guid Id { get; protected set; } = new Guid("F9C81693-CE16-456A-A1C4-AA109B6F56FE");

        public override string Category { get; protected set; } = "oM";

        public override string Name { get; protected set; } = "CreateEnum";

        public override string Description { get; protected set; } = "Creates a selected enum value";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateEnumCaller() : base()
        {
            List<Type> types = Engine.Reflection.Query.BHoMEnumList();
            SetPossibleItems(types);

            OutputParams = new List<ParamInfo>() { new ParamInfo { DataType = typeof(Enum), Kind = ParamKind.Output, Name = "enum", Description = "enum value" } };
        }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        public override bool SetItem(object item)
        {
            Type enumType = item as Type;
            Choices = Enum.GetValues(enumType).Cast<object>().ToList();
            return true;
        }

        /*************************************/

        public override List<string> GetChoiceNames()
        {
            Type enumType = Selector.GetSelectedItem() as Type;
            return Enum.GetNames(enumType).ToList();
        }

        /*************************************/
    }
}
