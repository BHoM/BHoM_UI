/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

using BH.Engine.Base;
using BH.Engine.Reflection;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Base.Reflection;
using BH.oM.Quantities.Attributes;
using BH.oM.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static List<ParamInfo> OutputParams(List<object> objects)
        {
            if (objects.Count == 0 )
                return new List<ParamInfo>();

            // Group the objects by type
            var groups = objects.Where(x => x != null).GroupBy(x => x.GetType());

            // Collect the output parameters
            switch (groups.Count())
            {
                case 0:
                    return new List<ParamInfo>();
                case 1:
                    return OutputFromSingleGroup(groups.First());
                default:
                    return OutputFromMultipleGroups(groups);
            }
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private static List<ParamInfo> OutputFromSingleGroup(IGrouping<Type, object> group)
        {
            if (typeof(IDictionary).IsAssignableFrom(group.Key) 
                || typeof(IDynamicObject).IsAssignableFrom(group.Key))
            {
                return OutputFromMultipleGroups(new List<IGrouping<Type, object>> { group });
            }
            else
            {
                return group.Key.GetProperties().Select(property => new ParamInfo
                {
                    Name = property.Name,
                    DataType = property.PropertyType,
                    Description = property.IDescription(),
                    Kind = ParamKind.Output
                }).ToList();
            }
        }

        /*************************************/

        private static List<ParamInfo> OutputFromMultipleGroups(IEnumerable<IGrouping<Type, object>> groups)
        {
            Dictionary<string, List<Type>> properties = new Dictionary<string, List<Type>>();
            Dictionary<string, string> descriptions = new Dictionary<string, string>();

            // Collect the properties types and names
            foreach (var group in groups)
            {
                if (typeof(IDictionary).IsAssignableFrom(group.Key))
                    CollectOutputTypes(group.Cast<IDictionary>(), ref properties);
                else if (typeof(IDynamicObject).IsAssignableFrom(group.Key))
                    CollectOutputTypes(group.Cast<IDynamicObject>(), ref properties, ref descriptions);
                else
                    CollectOutputTypes(group.Key, ref properties);
            }

            // Create the new output parameters
            List<ParamInfo> outputParams = new List<ParamInfo>() { };
            foreach (KeyValuePair<string, List<Type>> kvp in properties)
            {
                IEnumerable<Type> uniqueTypes = kvp.Value.Distinct();
                Type commonType = uniqueTypes.Count() > 1 ? typeof(object) : uniqueTypes.FirstOrDefault();
                string description = descriptions.ContainsKey(kvp.Key) ? descriptions[kvp.Key] : commonType.IDescription();

                outputParams.Add(new ParamInfo
                {
                    DataType = commonType ?? typeof(object),
                    Description = description,
                    Name = kvp.Key,
                    Kind = ParamKind.Output
                });
            }
            return outputParams;
        }

        /*************************************/

        private static void CollectOutputTypes(IEnumerable<IDictionary> objects, ref Dictionary<string, List<Type>> properties)
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
                else if (types[0].IsEnum)
                {
                    foreach (Enum key in dic.Keys.OfType<Enum>())
                    {
                        string stringKey = key.IToText();
                        if (key != null & !properties.ContainsKey(stringKey))
                            properties[stringKey] = new List<Type>();

                        if (dic[key] != null)
                            properties[stringKey].Add(dic[key].GetType() ?? null);
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

        private static void CollectOutputTypes(IEnumerable<IDynamicObject> objects, ref Dictionary<string, List<Type>> properties, ref Dictionary<string, string> descriptions)
        {
            foreach (IDynamicObject obj in objects)
            {
                object result;

                if (obj is IDynamicPropertyProvider)
                {
                    bool success = BH.Engine.Base.Compute.TryRunExtensionMethod(obj, "GetProperties", new object[] { }, out result);

                    if (success && result is List<Property>)
                    {
                        foreach (var prop in result as List<Property>)
                        {
                            if (!properties.ContainsKey(prop.Name))
                            {
                                properties[prop.Name] = new List<Type>();
                                descriptions[prop.Name] = prop.Description;
                            }
                            
                            properties[prop.Name].Add(prop.Type);
                        }
                    }
                }
                else
                {
                    foreach (PropertyInfo prop in obj.GetType().GetProperties())
                    {
                        if (prop.GetCustomAttribute<DynamicPropertyAttribute>() != null 
                            && typeof(IDictionary).IsAssignableFrom(prop.PropertyType) 
                            && prop.PropertyType.GenericTypeArguments.First().IsEnum)
                        {
                            IDictionary dic = prop.GetValue(obj) as IDictionary;

                            string typeDescription = prop.GetCustomAttribute<QuantityAttribute>()?.Description();
                            if (string.IsNullOrEmpty(typeDescription))
                                typeDescription = dic.GetType().GenericTypeArguments[1].Description();

                            foreach (Enum key in dic.Keys.OfType<Enum>().OrderBy(x => x))
                            {
                                string stringKey = key.IToText();
                                if (key != null & !properties.ContainsKey(stringKey))
                                {
                                    properties[stringKey] = new List<Type>();
                                    descriptions[stringKey] = key.IDescription() + "\n" + typeDescription;
                                }
                                    
                                if (dic[key] != null)
                                    properties[stringKey].Add(dic[key].GetType() ?? null);
                            }
                        }
                        else
                        {
                            if (!properties.ContainsKey(prop.Name))
                            {
                                properties[prop.Name] = new List<Type>();
                                descriptions[prop.Name] = prop.Description();
                            }

                            properties[prop.Name].Add(prop.PropertyType);
                        }    
                    }
                }
            }

        }

        /*************************************/

        private static void CollectOutputTypes(Type type, ref Dictionary<string, List<Type>> properties)
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
    }
}






