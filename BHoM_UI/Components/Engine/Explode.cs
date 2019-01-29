/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

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
using BH.Engine.Serialiser;

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
                if (obj == null)
                    return Enumerable.Repeat<object>(null, m_CompiledSetters.Count).ToList();

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

        public override bool Read(string json)
        {
            if (json == "")
                return true;

            try
            {
                CustomObject info = BH.Engine.Serialiser.Convert.FromJson(json) as CustomObject;

                if (info != null && info.CustomData.ContainsKey("Outputs"))
                {
                    IEnumerable outputs = info.CustomData["Outputs"] as IEnumerable;
                    if (outputs != null)
                    {
                        OutputParams = outputs.OfType<ParamInfo>().ToList();
                        CompileOutputSetters();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /*************************************/

        public override string Write()
        {
            CustomObject info = new CustomObject();
            info.CustomData["Outputs"] = OutputParams;
            return info.ToJson();
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public bool CollectOutputTypes(List<object> objects)
        {
            // Do not update if the list of input is empty
            if (objects.Count == 0)
                return false;

            // Group the objects by type
            Dictionary<string, List<Type>> properties = new Dictionary<string, List<Type>>();

            var groups = objects.Where(x => x != null).GroupBy(x => x.GetType());
            if (groups.Count() == 0)
                return false;

            // Collect the properties types and names
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
                                if (key != null & !properties.ContainsKey(key))
                                    properties[key] = new List<Type>();

                                if (dic[key] != null)
                                    properties[key].Add(dic[key].GetType() ?? null);
                            }
                        }
                        else
                        {
                            properties["Keys"] = new List<Type> { typeof(List<>).MakeGenericType(new Type[] { types[0] }) };
                            properties["Values"] = new List<Type> { typeof(List<>).MakeGenericType(new Type[] { types[1] }) };
                        }
                    }
                }
                else if (group.Key == typeof(CustomObject))
                {
                    foreach (KeyValuePair<string, object> prop in group.Cast<BHoMObject>().SelectMany(x => x.CustomData).Distinct())
                    {
                        if (!properties.ContainsKey(prop.Key))
                            properties[prop.Key] = new List<Type>();
                        if (prop.Value != null)
                            properties[prop.Key].Add(prop.Value.GetType() ?? null);
                    }
                }
                else
                {
                    foreach (PropertyInfo prop in group.Key.GetProperties().Where(x => x.CanRead && x.GetMethod.GetParameters().Count() == 0))
                    {
                        if (properties.ContainsKey(prop.Name) && properties[prop.Name].Contains(prop.PropertyType))
                        {
                            BH.Engine.Reflection.Compute.RecordWarning($"The property with name {prop.Name} is present in more than one object with different types. Type will be set to System.Object");
                            properties[prop.Name] = new List<Type> { typeof(object) };
                        }
                        else
                            properties[prop.Name] = new List<Type> { prop.PropertyType };
                    }
                }
            }

            // Create the new output parameters
            OutputParams = new List<ParamInfo>() { };
            foreach (KeyValuePair<string, List<Type>> kvp in properties)
            {
                OutputParams.Add(new ParamInfo
                {
                    DataType = kvp.Value.Where(t => t != null).FirstOrDefault() ?? typeof(object),
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
