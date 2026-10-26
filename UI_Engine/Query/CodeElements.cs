/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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
using BH.Engine.Serialiser;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Data.Requests;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description("Collect all the code elements that can be used to create UI components from the loaded assemblies.")]
        [Output("codeElements", "All code elements already loaded that can be used in the UI to create components.")]
        public static List<CodeElementRecord> CodeElements()
        {
            return CodeElements((string)null);
        }

        /*************************************/

        [Description("Collect the code elements that can be used to create UI components from the loaded assemblies matching the filter.")]
        [Input("assemblyNameFilter", "Regular expression the name of the assembly defining the element must match for the element to be collected. If null or empty, all loaded assemblies are used.")]
        [Output("codeElements", "Code elements from the matching loaded assemblies that can be used in the UI to create components.")]
        public static List<CodeElementRecord> CodeElements(string assemblyNameFilter)
        {
            return CodeElements(assemblyNameFilter, null);
        }


        /*************************************/
        /**** Internal Methods            ****/
        /*************************************/

        // droppedAssemblies, if provided, receives the names of the assemblies for which an element could not be converted to a record
        internal static List<CodeElementRecord> CodeElements(string assemblyNameFilter, HashSet<string> droppedAssemblies)
        {
            Regex assemblyFilter = string.IsNullOrEmpty(assemblyNameFilter) ? null : new Regex(assemblyNameFilter);
            List<CodeElementRecord> items = new List<CodeElementRecord>();

            /// Types

            // All constructable BHoM objects and requests
            items.AddRange(BH.Engine.UI.Query.ConstructableTypeItems()
                        .Where(x => Matches(x, assemblyFilter))
                        .Select(x => CodeElement(x, GetConstructableType(x), x.ConstructorText(), droppedAssemblies))
                        .Where(x => x != null));

            // All Enums
            items.AddRange(BH.Engine.UI.Query.EnumItems()
                        .Where(x => Matches(x, assemblyFilter))
                        .Select(x => CodeElement(x, CodeElementType.Enum, x.ToText(true), droppedAssemblies))
                        .Where(x => x != null));

            // All Types
            items.AddRange(BH.Engine.UI.Query.TypeItems()
                        .Where(x => Matches(x, assemblyFilter))
                        .Select(x => CodeElement(x, CodeElementType.Type, x.ToText(true), droppedAssemblies))
                        .Where(x => x != null));

            /// Methods

            // All adapter constructors
            items.AddRange(BH.Engine.UI.Query.AdapterConstructorItems()
                        .Where(x => Matches(x, assemblyFilter))
                        .Select(x => CodeElement(x, CodeElementType.AdapterConstructor, x.ToText(true), droppedAssemblies))
                        .Where(x => x != null));

            // All methods for the BHoM Engine (including creators)
            items.AddRange(BH.Engine.Base.Query.BHoMMethodList()
                        .Where(x => Matches(x, assemblyFilter) && x.IsExposed())
                        .Select(x => CodeElement(x, GetMethodType(x), x.ToText(includePath: true, removeIForInterface: false), droppedAssemblies))
                        .Where(x => x != null));

            // All methods from external class
            items.AddRange(BH.Engine.UI.Query.ExternalItems()
                        .Where(x => Matches(x, assemblyFilter))
                        .Select(x => CodeElement(x, CodeElementType.Method_External, x.ToText(true), droppedAssemblies))
                        .Where(x => x != null));

            // Return the list
            return items;
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        private static bool Matches(Type type, Regex assemblyFilter)
        {
            return assemblyFilter == null || assemblyFilter.IsMatch(AssemblyName(type));
        }

        /*************************************/

        private static bool Matches(MethodBase method, Regex assemblyFilter)
        {
            return assemblyFilter == null || assemblyFilter.IsMatch(AssemblyName(method));
        }

        /*************************************/

        private static CodeElementRecord CodeElement(Type type, CodeElementType elementType, string displayText, HashSet<string> droppedAssemblies)
        {
            try
            {
                List<Type> inputTypes = type.GetProperties()
                    .Select(x => x.PropertyType?.UnderlyingType()?.Type)
                    .Where(x => x != null)
                    .Distinct()
                    .ToList();

                return new CodeElementRecord
                {
                    AssemblyName = AssemblyName(type),
                    AssemblyModifiedTime = AssemblyModifiedTime(type),
                    Type = elementType,
                    DisplayText = displayText,
                    Json = type.ToJson(),
                    InputKeys = inputTypes.Select(x => x.ToText(true)).ToList(),
                    OutputKeys = type.UnderlyingType()?.Type.OutputKeys()
                };
            }
            catch (Exception e)
            {
                if (type == null)   //Don't think this will ever happen, but adding as a extra safeguard
                    BH.Engine.Base.Compute.RecordError(e, "Null type found when initialising the UI.");
                else
                {
                    BH.Engine.Base.Compute.RecordError(e, $"Failed to load type {type.FullName} from {type.Assembly.FullName} to the BHoM UI. The type will not be available.");
                    droppedAssemblies?.Add(AssemblyName(type));
                }

                return null;
            }
        }

        /*************************************/

        private static CodeElementRecord CodeElement(MethodBase method, CodeElementType elementType, string displayText, HashSet<string> droppedAssemblies)
        {
            try
            {
                Type outputType = (method is MethodInfo) ? ((MethodInfo)method).ReturnType : method.DeclaringType;
                List<Type> inputTypes = method.GetParameters()
                    .Select(x => x.ParameterType?.UnderlyingType()?.Type)
                    .Where(x => x != null)
                    .Distinct()
                    .ToList();

                return new CodeElementRecord
                {
                    AssemblyName = AssemblyName(method),
                    AssemblyModifiedTime = AssemblyModifiedTime(method),
                    Type = elementType,
                    DisplayText = displayText,
                    Json = method.ToJson(),
                    InputKeys = inputTypes.Select(x => x.ToText(true)).ToList(),
                    OutputKeys = outputType.UnderlyingType()?.Type.OutputKeys()
                };
            }
            catch (Exception e)
            {
                if(method == null)  //Don't think this will ever happen, but adding as a extra safeguard
                    BH.Engine.Base.Compute.RecordError(e, "Null method found when initialising the UI.");
                else
                {
                    BH.Engine.Base.Compute.RecordError(e, $"Failed to load method {method.Name} from {method.DeclaringType.FullName} from assembly {method.DeclaringType.Assembly.FullName} to the BHoM UI. The method will not be available.");
                    droppedAssemblies?.Add(AssemblyName(method));
                }
                
                return null;
            }
        }

        /*************************************/

        /*************************************/

        private static string AssemblyName(MethodBase method)
        {
            return AssemblyName(method.DeclaringType);
        }

        /*************************************/

        private static string AssemblyName(Type type)
        {
            return type.Assembly.GetName().Name;
        }

        /*************************************/

        private static DateTime AssemblyModifiedTime(MethodBase method)
        {
            return AssemblyModifiedTime(method.DeclaringType);
        }

        /*************************************/

        private static DateTime AssemblyModifiedTime(Type type)
        {
            if (string.IsNullOrEmpty(type?.Assembly?.Location))
                return DateTime.MinValue;
            else
                return File.GetLastWriteTimeUtc(type.Assembly.Location);
        }

        /*************************************/

        private static CodeElementType GetConstructableType(Type type)
        {
            if (typeof(IRequest).IsAssignableFrom(type))
                return CodeElementType.ConstructableRequest;
            else
                return CodeElementType.ConstructableObject;
        }

        /*************************************/

        private static CodeElementType GetMethodType(MethodInfo method)
        {
            switch (method.DeclaringType.Name)
            {
                case "Create":
                    if (typeof(IRequest).IsAssignableFrom(method.ReturnType))
                        return CodeElementType.RequestCreator;
                    else
                        return CodeElementType.ObjectCreator;
                case "Compute":
                    return CodeElementType.Method_Compute;
                case "Convert":
                    return CodeElementType.Method_Convert;
                case "Modify":
                    return CodeElementType.Method_Modify;
                case "Query":
                    return CodeElementType.Method_Query;
                default:
                    return CodeElementType.Undefined;
            }
        }

        /*************************************/
    }
}






