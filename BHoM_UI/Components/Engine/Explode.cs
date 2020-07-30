/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

        public List<ParamInfo> PossibleOutputs { get; protected set; } = new List<ParamInfo>();


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
                {
                    if (data[i] != null || !OutputParams[i].DataType.UnderlyingType().Type.IsValueType)
                        m_CompiledSetters[i](DataAccessor, data[i]);
                }
                    
            }
            catch (Exception e)
            {
                RecordError(e, "This component failed to run properly. Output data is calculated but cannot be set.\n");
                return false;
            }

            return true;
        }

        /*************************************/

        public bool RemoveOutput(string name)
        {
            if (name == null)
                return false;

            bool success = OutputParams.RemoveAll(p => p.Name == name) > 0;
            m_OutputSelector.SetParamCheck(name, false);
            CompileOutputSetters();

            return success;
        }

        /*************************************/

        public override bool Read(string json)
        {
            if (json == "")
                return true;

            try
            {
                CustomObject component = BH.Engine.Serialiser.Convert.FromJson(json) as CustomObject;
                if (component == null)
                    return true;

                object outputParams;
                if (component.CustomData.TryGetValue("Outputs", out outputParams))
                {
                    OutputParams = (outputParams as IEnumerable).OfType<ParamInfo>().ToList();
                    CompileOutputSetters();
                }

                object possibleOutputs;
                if (component.CustomData.TryGetValue("PossibleOutputs", out possibleOutputs))
                {
                    PossibleOutputs = (possibleOutputs as IEnumerable).OfType<ParamInfo>().ToList();
                    SetOutputSelectionMenu();
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
            info.CustomData["PossibleOutputs"] = PossibleOutputs;
            return info.ToJson();
        }

        /*************************************/

        public bool IsAllowedToUpdate()
        {
            return OutputParams.Count >= PossibleOutputs.Count;
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public bool CollectOutputTypes(List<object> objects)
        {
            // Do not update if the list of input is empty or if user has manually selected outputs
            if (objects.Count == 0 || !IsAllowedToUpdate())
                return false;

            // Group the objects by type
            var groups = objects.Where(x => x != null).GroupBy(x => x.GetType());

            // Collect the output parameters
            switch (groups.Count())
            {
                case 0:
                    return false;
                case 1:
                    CollectOutputFromSingleGroup(groups.First());
                    break;
                default:
                    CollectOutputFromMultipleGroups(groups);
                    break;
            }
            PossibleOutputs = OutputParams.ToList();

            // Compile the setters
            CompileOutputSetters();

            // Create the output menu
            SetOutputSelectionMenu();

            return true;
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        protected void CollectOutputFromSingleGroup(IGrouping<Type, object> group)
        {
            if (typeof(IDictionary).IsAssignableFrom(group.Key) || group.Key == typeof(CustomObject))
            {
                CollectOutputFromMultipleGroups(new List<IGrouping<Type, object>> { group });
            }
            else
            {
                OutputParams = group.Key.GetProperties().Select(property => new ParamInfo
                {
                    Name = property.Name,
                    DataType = property.PropertyType,
                    Description = property.IDescription(),
                    Kind = ParamKind.Input
                }).ToList();
            }   
        }

        /*************************************/

        protected void CollectOutputFromMultipleGroups(IEnumerable<IGrouping<Type, object>> groups)
        {
            Dictionary<string, List<Type>> properties = new Dictionary<string, List<Type>>();

            // Collect the properties types and names
            foreach (var group in groups)
            {
                if (typeof(IDictionary).IsAssignableFrom(group.Key))
                    CollectOutputTypes(group.Cast<IDictionary>(), ref properties);
                else if (group.Key == typeof(CustomObject))
                    CollectOutputTypes(group.Cast<CustomObject>(), ref properties);
                else
                    CollectOutputTypes(group.Key, ref properties);
            }

            // Create the new output parameters
            OutputParams = new List<ParamInfo>() { };
            foreach (KeyValuePair<string, List<Type>> kvp in properties)
            {
                IEnumerable<Type> uniqueTypes = kvp.Value.Distinct();
                Type commonType = uniqueTypes.Count() > 1 ? typeof(object) : uniqueTypes.FirstOrDefault();
                OutputParams.Add(new ParamInfo
                {
                    DataType = commonType ?? typeof(object),
                    Name = kvp.Key,
                    Kind = ParamKind.Output
                });
            }
        }

        /*************************************/

        protected void CollectOutputTypes(IEnumerable<IDictionary> objects, ref Dictionary<string, List<Type>> properties)
        {
            foreach (IDictionary dic in objects)
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

        /*************************************/

        protected void CollectOutputTypes(IEnumerable<CustomObject> objects, ref Dictionary<string, List<Type>> properties)
        {
            foreach (KeyValuePair<string, object> prop in objects.SelectMany(x => x.CustomData).Distinct())
            {
                if (!properties.ContainsKey(prop.Key))
                    properties[prop.Key] = new List<Type>();
                if (prop.Value != null)
                    properties[prop.Key].Add(prop.Value.GetType() ?? null);
            }
            if (!properties.ContainsKey("Name"))
                properties["Name"] = new List<Type> { typeof(string) };
            if (!properties.ContainsKey("Tags"))
                properties["Tags"] = new List<Type> { typeof(HashSet<string>) };
            if (!properties.ContainsKey("BHoM_Guid"))
                properties["BHoM_Guid"] = new List<Type> { typeof(Guid) };
        }

        /*************************************/

        protected void CollectOutputTypes(Type type, ref Dictionary<string, List<Type>> properties)
        {
            foreach (PropertyInfo prop in type.GetProperties().Where(x => x.CanRead && x.GetMethod.GetParameters().Count() == 0))
            {
                if (properties.ContainsKey(prop.Name))
                    properties[prop.Name].Add(prop.PropertyType);
                else
                    properties[prop.Name] = new List<Type> { prop.PropertyType };
            }
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        /*************************************/
    }
}

