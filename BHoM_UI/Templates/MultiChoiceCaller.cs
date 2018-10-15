using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Templates
{
    public abstract class MultiChoiceCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public List<object> Choices { get; protected set; } = new List<object>();


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public MultiChoiceCaller() : base()
        {
            InputParams = new List<ParamInfo>() { new ParamInfo { DataType = typeof(int), Kind = ParamKind.Input, Name = "index", Description = "index of the enum value" } };
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public override object Run(object[] inputs)
        {
            if (inputs.Length != 1)
                return null;

            int index = (int)inputs[0];
            if (index >= 0 && index < Choices.Count)
                return Choices[index];
            else
                return null;
        }

        /*************************************/

        public override abstract bool SetItem(object item);

        /*************************************/

        public abstract List<string> GetChoiceNames();

        /*************************************/
    }
}
