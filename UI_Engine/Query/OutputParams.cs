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

using BH.Engine.Reflection;
using BH.oM.Base;
using BH.oM.UI;
using System;
using System.Collections;
using System.Collections.Generic;
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
            if (typeof(IDictionary).IsAssignableFrom(group.Key) || group.Key == typeof(CustomObject))
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
            List<ParamInfo> outputParams = new List<ParamInfo>() { };
            foreach (KeyValuePair<string, List<Type>> kvp in properties)
            {
                IEnumerable<Type> uniqueTypes = kvp.Value.Distinct();
                Type commonType = uniqueTypes.Count() > 1 ? typeof(object) : uniqueTypes.FirstOrDefault();
                outputParams.Add(new ParamInfo
                {
                    DataType = commonType ?? typeof(object),
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
                else
                {
                    properties["Keys"] = new List<Type> { typeof(List<>).MakeGenericType(new Type[] { types[0] }) };
                    properties["Values"] = new List<Type> { typeof(List<>).MakeGenericType(new Type[] { types[1] }) };
                }
            }
        }

        /*************************************/

        private static void CollectOutputTypes(IEnumerable<CustomObject> objects, ref Dictionary<string, List<Type>> properties)
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

