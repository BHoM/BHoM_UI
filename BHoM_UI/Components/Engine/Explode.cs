using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using BH.oM.UI;
using BH.oM.Base;
using BH.Engine.Reflection;

namespace BH.UI.Components
{
    public class ExplodeCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Explode;

        public override Guid Id { get; protected set; } = new Guid("3647C48A-3322-476F-8B34-4011540AB916");

        public override string Name { get; protected set; } = "Explode";

        public override string Category { get; protected set; } = "Engine";

        public override string Description { get; protected set; } = "Explode an object into its properties";

        public override int GroupIndex { get; protected set; } = 2;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public ExplodeCaller() : base()
        {
            InputParams = new List<ParamInfo>() { new ParamInfo { DataType = typeof(IObject), Kind = ParamKind.Input, Name = "object", Description = "Object to explode" } };
            OutputParams = new List<ParamInfo>() { };
        }


        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        public override object Run(object[] inputs)
        {
            if (inputs.Length != 1)
            {
                BH.Engine.Reflection.Compute.RecordError("The number of inputs is invalid.");
                return null;
            }
            else
            {
                object obj = inputs[0];
                return OutputParams.Select(x => obj.PropertyValue(x.Name)).ToList();
            }
        }

        /*************************************/

        public bool CollectOutputTypes(List<object> objects)
        {
            // Collect the properties types and names
            Dictionary<string, Type> properties = new Dictionary<string, Type>();
            var groups = objects.GroupBy(x => x.GetType());
            foreach (var group in groups)
            {
                foreach(PropertyInfo prop in group.Key.GetProperties().Where(x => x.CanRead && x.GetMethod.GetParameters().Count() == 0))
                {
                    if (properties.ContainsKey(prop.Name) && properties[prop.Name] != prop.PropertyType)
                    {
                        BH.Engine.Reflection.Compute.RecordError("Some object have properties with the same name but with different property types.");
                        return false;
                    }  
                    else
                        properties[prop.Name] = prop.PropertyType;
                }
            }

            //Collect the custom data
            foreach (var group in groups.Where(x => x.Key == typeof(CustomObject))) // Just remove the Where condition if you want all the object to expose directly their custom data
            {
                foreach (string propName in group.OfType<BHoMObject>().SelectMany(x => x.CustomData.Keys).Distinct())
                {
                    if (!properties.ContainsKey(propName))
                        properties[propName] = typeof(object);
                }
            }

            // Create the new output parameters
            OutputParams = new List<ParamInfo>() { };
            foreach (KeyValuePair<string,Type> kvp in properties)
            {
                OutputParams.Add(new ParamInfo
                {
                    DataType = kvp.Value,
                    Name = kvp.Key,
                    Kind = ParamKind.Output
                });
            }

            // Compile the setters
            CompileOutputSetters();

            return true;
        }

        /*************************************/
    }
}
