using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using BH.oM.Base;
using BH.oM.UI;
using BH.Engine.Reflection;
using System.ComponentModel;

namespace BH.UI.Components
{
    public class CreateCustomCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.CustomObject;

        public override Guid Id { get; protected set; } = new Guid("EB7B72E5-B4D8-4FF6-BCBD-833CDEC5D1A2");

        public override string Name { get; protected set; } = "CreateCustom";

        public override string Category { get; protected set; } = "oM";

        public override string Description { get; protected set; } = "Creates an instance of a selected type of BHoM object by manually defining its properties (default type is CustomObject)";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateCustomCaller() : base()
        {
            SetPossibleItems(Engine.Reflection.Query.BHoMTypeList());

            InputParams = new List<ParamInfo>();
            OutputParams = new List<ParamInfo>() { new ParamInfo { DataType = typeof(IObject), Kind = ParamKind.Output, Name = "object", Description = "New Object with properties set as per the inputs." } };
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public void SetInputs(List<string> names)
        {
            InputParams = names.Select(x => GetParam(x)).ToList();

            CompileInputGetters();
            CompileOutputSetters();
        }


        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        public override object Run(object[] inputs)
        {
            IObject obj = new CustomObject();
            if (m_ForcedType != null)
                obj = Activator.CreateInstance(m_ForcedType) as IObject;
            if (obj == null)
                obj = new CustomObject();

            if (inputs.Length == InputParams.Count)
            {
                for (int i = 0; i < inputs.Length; i++)
                    BH.Engine.Reflection.Modify.SetPropertyValue(obj, InputParams[i].Name, inputs[i]);
            }

            return obj;
        }

        /*************************************/

        public override bool SetItem(object item)
        {
            m_ForcedType = item as Type;

            if (m_ForcedType != null)
            {
                Name = m_ForcedType.Name;
                Description = m_ForcedType.Description();
                InputParams = m_ForcedType.GetProperties().Select(x => GetParam(x)).ToList();
            }
                

            return true;
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        public oM.UI.ParamInfo GetParam(string name)
        {
            Type type = typeof(object);
            if (m_ForcedType != null)
            {
                PropertyInfo info = m_ForcedType.GetProperty(name);
                if (info != null)
                    type = info.PropertyType;
            }

            return new ParamInfo
            {
                Name = name,
                DataType = type,
                Kind = ParamKind.Input
            };
        }

        /*************************************/

        public oM.UI.ParamInfo GetParam(PropertyInfo info)
        {
            return new ParamInfo
            {
                Name = info.Name,
                DataType = info.PropertyType,
                Description = info.IDescription(),
                Kind = ParamKind.Input
            };
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        Type m_ForcedType = null;

        /*************************************/
    }
}
