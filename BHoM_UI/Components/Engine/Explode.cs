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
using System.Collections;
using System.Windows.Controls;
using System.Windows.Forms;

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
            InputParams = new List<ParamInfo>() { new ParamInfo { DataType = typeof(object), Kind = ParamKind.Input, Name = "object", Description = "Object to explode" } };
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

        protected override bool PushOutputs(object result)
        {
            try
            {
                List<object> data = result as List<object>;
                if (m_CompiledSetters.Count != data.Count)
                {
                    RecordError(new Exception(""), "The number of outputs doesn't correspond to the data to push out.");
                    return false;
                }

                for (int i = 0; i < m_CompiledSetters.Count; i++)
                    m_CompiledSetters[i](DataAccessor, data[i]);
            }
            catch (Exception e)
            {
                RecordError(e, "This component failed to run properly. Output data is calculated but cannot be set.\n");
                return false;
            }

            return true;
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public bool CollectOutputTypes(List<object> objects)
        {
            // Collect the properties types and names
            Dictionary<string, Type> properties = new Dictionary<string, Type>();
            var groups = objects.GroupBy(x => x.GetType());
            foreach (var group in groups)
            {
                if (typeof(IDictionary).IsAssignableFrom(group.Key))
                {
                    foreach (IDictionary dic in group)
                    {
                        Type[] types = dic.GetType().GetGenericArguments();
                        if (types.Length != 2)
                            continue;

                        if (types[0] == typeof(string))
                        {
                            foreach (string key in dic.Keys.OfType<string>())
                            {
                                if (!properties.ContainsKey(key))
                                    properties[key] = dic[key].GetType();
                            }
                        }
                        else
                        {
                            properties["Keys"] = typeof(List<>).MakeGenericType(new Type[] { types[0] });
                            properties["Values"] = typeof(List<>).MakeGenericType(new Type[] { types[1] });
                        }
                    }
                }
                else if (group.Key == typeof(CustomObject))
                {
                    foreach (string propName in group.Cast<BHoMObject>().SelectMany(x => x.CustomData.Keys).Distinct())
                    {
                        if (!properties.ContainsKey(propName))
                            properties[propName] = typeof(object);
                    }
                }
                else
                {
                    foreach (PropertyInfo prop in group.Key.GetProperties().Where(x => x.CanRead && x.GetMethod.GetParameters().Count() == 0))
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
