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
using BH.Engine.Base.Objects;
using BH.Engine.Reflection;
using BH.Engine.UI;
using BH.oM.Base.Attributes;
using BH.oM.Data.Requests;
using BH.oM.UI;
using BH.UI.Base.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BH.UI.Base.Global
{
    public class AssemblyResolver : IAssemblyResolver
    {
        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public AssemblyResolver()
        {
        }

        /*************************************/

        [Description("Create a resolver from the maps between types or extension methods and the names of the assemblies defining them.")]
        public AssemblyResolver(Dictionary<string, List<string>> assemblyNamePerType, Dictionary<string, Dictionary<string, List<string>>> assemblyNamesPerExtensionMethod)
        {
            if (assemblyNamePerType != null)
                m_AssemblyNamePerType = assemblyNamePerType;

            if (assemblyNamesPerExtensionMethod != null)
                m_AssemblyNamesPerExtensionMethod = assemblyNamesPerExtensionMethod;
        }

        /*************************************/

        [Description("Create a resolver from a collection of code elements, mapping types and extension methods to the assemblies that define them.")]
        [Input("codeElements", "Code elements describing the content of the assemblies (typically read from an AssemblyContent.tsv file).")]
        public AssemblyResolver(IEnumerable<CodeElementRecord> codeElements)
        {
            AddCodeElements(codeElements);
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description("Add the content described by the code elements to the maps used by the resolver. Can be called while the resolver is in use.")]
        public void AddCodeElements(IEnumerable<CodeElementRecord> codeElements)
        {
            if (codeElements == null)
                return;

            List<CodeElementRecord> records = codeElements.Where(x => x != null).ToList();

            // Collect the relation between types and the assembly they belong to
            // Types are looked up by the name the serialiser writes (e.g. BaseTagHost`1), not by the display text of the code element (e.g. BaseTagHost<T>)
            Dictionary<string, List<string>> assemblyNamePerType = records
                .Where(x => x.Type == CodeElementType.Type && !string.IsNullOrWhiteSpace(x.DisplayText))
                .GroupBy(x => ToTypeKey(x.DisplayText))
                .ToDictionary(group => group.Key, group => group.Select(x => x.AssemblyName).Distinct().ToList());

            // Collect the relation between extension methods and the assembly they belong to
            Dictionary<string, Dictionary<string, List<string>>> assemblyNamesPerExtensionMethod = BuildExtensionMethodDictionary(records);

            // The maps are replaced rather than modified so that concurrent readers always see a consistent snapshot
            lock (m_Lock)
            {
                m_AssemblyNamePerType = Merge(m_AssemblyNamePerType, assemblyNamePerType);
                m_AssemblyNamesPerExtensionMethod = Merge(m_AssemblyNamesPerExtensionMethod, assemblyNamesPerExtensionMethod);
            }
        }

        /*************************************/

        [Description("Make sure assemblies that contain a type matching the input name are loaded. Return true if any assembly was loaded.")]
        public virtual bool MakeSureAssemblyIsLoadedForType(string type)
        {
            if (string.IsNullOrEmpty(type) || !type.StartsWith("BH."))
                return false;

            string[] parts = type.Split(',');
            List<string> assemblyNames = new List<string>();

            if (parts.Length > 1)
            {
                // Assembly is already in the type
                assemblyNames.Add(parts[1].Trim());
            }
            else if (parts.Length == 1)
            {
                // We don't have the assembly registered in the type so we need to deduce it
                Dictionary<string, List<string>> assemblyNamePerType = m_AssemblyNamePerType;
                if (assemblyNamePerType.ContainsKey(type))
                    assemblyNames.AddRange(assemblyNamePerType[type]);
            }

            return LoadAssemblies(assemblyNames, $"type {type}", true);
        }

        /***************************************************/

        [Description("Make sure assemblies containing extension methods matching the input name and target type are loaded. Returns true if any assembly was loaded.")]
        public virtual bool MakeSureAssemblyIsLoadedForExtensionMethod(string methodName, Type targetType)
        {
            if (string.IsNullOrEmpty(methodName) || targetType == null)
                return false;

            Dictionary<string, Dictionary<string, List<string>>> assemblyNamesPerExtensionMethod = m_AssemblyNamesPerExtensionMethod;
            if (!assemblyNamesPerExtensionMethod.ContainsKey(methodName))
                return false;

            Dictionary<string, List<string>> typeToAssemblies = assemblyNamesPerExtensionMethod[methodName];
            HashSet<string> assembliesToLoad = new HashSet<string>();

            // 1. Exact type match
            string exactTypeName = TypeKey(targetType);
            if (typeToAssemblies.ContainsKey(exactTypeName))
                assembliesToLoad.UnionWith(typeToAssemblies[exactTypeName]);

            // 2. Base types
            Type baseType = targetType.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                string baseTypeName = TypeKey(baseType);
                if (typeToAssemblies.ContainsKey(baseTypeName))
                    assembliesToLoad.UnionWith(typeToAssemblies[baseTypeName]);
                baseType = baseType.BaseType;
            }

            // 3. Interfaces
            foreach (Type interfaceType in targetType.GetInterfaces())
            {
                string interfaceName = TypeKey(interfaceType);
                if (typeToAssemblies.ContainsKey(interfaceName))
                    assembliesToLoad.UnionWith(typeToAssemblies[interfaceName]);
            }

            // 4. Generic type definitions
            if (targetType.IsGenericType)
            {
                Type genericDef = targetType.GetGenericTypeDefinition();
                string genericTypeName = TypeKey(genericDef);
                if (typeToAssemblies.ContainsKey(genericTypeName))
                    assembliesToLoad.UnionWith(typeToAssemblies[genericTypeName]);
            }

            // Load all identified assemblies
            return LoadAssemblies(assembliesToLoad, $"extension method {methodName}", false);
        }


        /*************************************/
        /**** Protected Methods           ****/
        /*************************************/

        [Description("Get the path of the file for an assembly. Can be overridden to change where assemblies are searched for.")]
        protected virtual string AssemblyPath(string assemblyName)
        {
            return BH.Engine.UI.Query.AssemblyPath(assemblyName);
        }

        /*************************************/

        [Description("Convert an assembly name found in a type or in the maps into the name of the assembly to load. Returns the name unchanged by default.")]
        protected virtual string NormaliseAssemblyName(string assemblyName)
        {
            return assemblyName;
        }

        /*************************************/

        [Description("Load the assemblies that are not loaded yet. Returns true if any assembly was loaded.")]
        protected virtual bool LoadAssemblies(IEnumerable<string> assemblyNames, string context, bool missingIsError)
        {
            bool anyLoaded = false;
            foreach (string name in assemblyNames.Where(x => !string.IsNullOrEmpty(x)))
            {
                string assemblyName = NormaliseAssemblyName(name);
                if (string.IsNullOrEmpty(assemblyName) || BH.Engine.Base.Query.IsAssemblyLoaded(assemblyName))
                    continue;

                string assemblyPath = AssemblyPath(assemblyName);
                if (!File.Exists(assemblyPath))
                {
                    if (missingIsError)
                        BH.Engine.Base.Compute.RecordError($"Assembly file not found when trying to load assemblies for {context}: {assemblyPath}");
                    else
                        BH.Engine.Base.Compute.RecordNote($"Assembly not found for {context}: {assemblyPath}");
                    continue;
                }

                try
                {
                    Assembly assembly = BH.Engine.Base.Compute.LoadAssembly(assemblyPath);
                    if (assembly != null)
                        anyLoaded = true;
                    else if (missingIsError)
                        BH.Engine.Base.Compute.RecordError($"Failed to load assembly: {assemblyName}");
                    else
                        BH.Engine.Base.Compute.RecordWarning($"Failed to load assembly: {assemblyName}");
                }
                catch (Exception ex)
                {
                    BH.Engine.Base.Compute.RecordError(ex, $"Exception while loading assembly for {context}: {assemblyPath}.");
                }
            }

            return anyLoaded;
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private static string TypeKey(Type type)
        {
            string key = type.FullName ?? type.Name;
            int cut = key.IndexOfAny(new char[] { ',', '[' });
            if (cut > 0)
                key = key.Substring(0, cut);
            return key;
        }

        /*************************************/

        private static Dictionary<string, List<string>> Merge(Dictionary<string, List<string>> existing, Dictionary<string, List<string>> added)
        {
            Dictionary<string, List<string>> result = existing.ToDictionary(x => x.Key, x => new List<string>(x.Value));
            foreach (KeyValuePair<string, List<string>> pair in added)
            {
                if (!result.ContainsKey(pair.Key))
                    result[pair.Key] = new List<string>();

                foreach (string assembly in pair.Value)
                {
                    if (!result[pair.Key].Contains(assembly))
                        result[pair.Key].Add(assembly);
                }
            }
            return result;
        }

        /*************************************/

        private static Dictionary<string, Dictionary<string, List<string>>> Merge(Dictionary<string, Dictionary<string, List<string>>> existing, Dictionary<string, Dictionary<string, List<string>>> added)
        {
            Dictionary<string, Dictionary<string, List<string>>> result = existing.ToDictionary(x => x.Key, x => Merge(x.Value, new Dictionary<string, List<string>>()));
            foreach (KeyValuePair<string, Dictionary<string, List<string>>> pair in added)
            {
                Dictionary<string, List<string>> current = result.ContainsKey(pair.Key) ? result[pair.Key] : new Dictionary<string, List<string>>();
                result[pair.Key] = Merge(current, pair.Value);
            }
            return result;
        }

        /*************************************/

        private static Dictionary<string, Dictionary<string, List<string>>> BuildExtensionMethodDictionary(IEnumerable<CodeElementRecord> codeElements)
        {
            Dictionary<string, Dictionary<string, List<string>>> result
                = new Dictionary<string, Dictionary<string, List<string>>>();

            foreach (CodeElementRecord record in codeElements.Where(x =>
                x.Type == CodeElementType.Method_Query ||
                x.Type == CodeElementType.Method_Compute ||
                x.Type == CodeElementType.Method_Convert ||
                x.Type == CodeElementType.Method_Modify))
            {
                try
                {
                    // Extract first parameter type from JSON
                    string firstParamTypeName = ExtractFirstParameterType(record.Json);

                    if (!string.IsNullOrEmpty(firstParamTypeName))
                    {
                        // Extract method name from DisplayText
                        string methodName = ExtractMethodName(record.DisplayText);

                        // Build nested dictionary
                        if (!result.ContainsKey(methodName))
                            result[methodName] = new Dictionary<string, List<string>>();

                        if (!result[methodName].ContainsKey(firstParamTypeName))
                            result[methodName][firstParamTypeName] = new List<string>();

                        if (!result[methodName][firstParamTypeName].Contains(record.AssemblyName))
                            result[methodName][firstParamTypeName].Add(record.AssemblyName);
                    }
                }
                catch (Exception ex)
                {
                    BH.Engine.Base.Compute.RecordWarning($"Failed to parse extension method from {record.DisplayText}: {ex.Message}");
                }
            }

            return result;
        }

        /*************************************/

        private static string ExtractFirstParameterType(string json)
        {
            if (string.IsNullOrEmpty(json))
                return null;

            try
            {
                // Find the Parameters array in the JSON
                int parametersIndex = json.IndexOf("\"Parameters\"");
                if (parametersIndex < 0)
                    return null;

                // Find the opening bracket of the Parameters array
                int arrayStartIndex = json.IndexOf('[', parametersIndex);
                if (arrayStartIndex < 0)
                    return null;

                // Find the end of the first parameter (first element in array)
                // The first element starts after [ and is a JSON string itself
                int firstParamStart = arrayStartIndex + 1;

                // Skip whitespace
                while (firstParamStart < json.Length && char.IsWhiteSpace(json[firstParamStart]))
                    firstParamStart++;

                // Check if array is empty
                if (firstParamStart >= json.Length || json[firstParamStart] == ']')
                    return null;

                // Current format: the first parameter is a plain string holding the name of the type, e.g. "BH.oM.Geometry.Vector"
                if (json[firstParamStart] == '"' && (firstParamStart + 1 >= json.Length || json[firstParamStart + 1] != '{'))
                {
                    int plainEnd = json.IndexOf('"', firstParamStart + 1);
                    if (plainEnd < 0)
                        return null;

                    return ToTypeKey(json.Substring(firstParamStart + 1, plainEnd - firstParamStart - 1));
                }

                // Previous format: the first parameter is a JSON string, find "Name" property within it
                // Look for "Name" : "..." pattern in the first parameter
                int nameIndex = json.IndexOf("\\\"Name\\\"", firstParamStart);
                if (nameIndex < 0)
                    return null;

                // Find the opening quote of the Name value
                int nameValueStart = json.IndexOf("\\\"", nameIndex + 8); // Skip past \"Name\"
                if (nameValueStart < 0)
                    return null;

                nameValueStart += 2; // Skip past \"

                // Find the closing quote of the Name value
                int nameValueEnd = json.IndexOf("\\\"", nameValueStart);
                if (nameValueEnd < 0)
                    return null;

                // Extract the type name
                string typeName = json.Substring(nameValueStart, nameValueEnd - nameValueStart);

                // Remove assembly qualification: "Type, Assembly" -> "Type"
                int commaIndex = typeName.IndexOf(',');
                if (commaIndex > 0)
                    typeName = typeName.Substring(0, commaIndex).Trim();

                return typeName;
            }
            catch (Exception ex)
            {
                // Silent fail
                BH.Engine.Base.Compute.RecordNote($"Could not extract parameter type: {ex.Message}");
            }

            return null;
        }

        /*************************************/

        private static string ToTypeKey(string typeName)
        {
            // Converts the text of a type, as written in the parameters of a method, into the key built by TypeKey for a type at run time
            // e.g. "System.Collections.Generic.List<BH.oM.Geometry.Point>" -> "System.Collections.Generic.List`1" and "System.Double[]" -> "System.Double"
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            typeName = typeName.Trim();

            // By-reference and pointer markers
            typeName = typeName.TrimEnd('&', '*');

            // Array ranks, e.g. [] or [,]
            while (typeName.EndsWith("]"))
            {
                int rankStart = typeName.LastIndexOf('[');
                if (rankStart <= 0 || typeName.Substring(rankStart + 1, typeName.Length - rankStart - 2).Any(x => x != ','))
                    break;

                typeName = typeName.Substring(0, rankStart);
            }

            // Generic arguments: only the number of top-level arguments is kept
            int genericStart = typeName.IndexOf('<');
            if (genericStart > 0 && typeName.EndsWith(">"))
            {
                int arguments = 1;
                int depth = 0;
                for (int i = genericStart + 1; i < typeName.Length - 1; i++)
                {
                    char c = typeName[i];
                    if (c == '<')
                        depth++;
                    else if (c == '>')
                        depth--;
                    else if (c == ',' && depth == 0)
                        arguments++;
                }

                typeName = typeName.Substring(0, genericStart) + "`" + arguments;
            }

            return typeName;
        }

        /*************************************/

        private static string ExtractMethodName(string displayText)
        {
            // Format: "BH.Engine.Namespace.Query.MethodName(params)" or with generics
            int openParen = displayText.IndexOf('(');
            if (openParen < 0)
                return displayText;

            string beforeParams = displayText.Substring(0, openParen);

            // Remove generic type parameters if present
            int genericStart = beforeParams.IndexOf('<');
            if (genericStart > 0)
                beforeParams = beforeParams.Substring(0, genericStart);

            // Get last segment after last dot
            int lastDot = beforeParams.LastIndexOf('.');
            if (lastDot >= 0)
                return beforeParams.Substring(lastDot + 1);

            return beforeParams;
        }

        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private readonly object m_Lock = new object();

        private Dictionary<string, List<string>> m_AssemblyNamePerType = new Dictionary<string, List<string>>();

        private Dictionary<string, Dictionary<string, List<string>>> m_AssemblyNamesPerExtensionMethod = new Dictionary<string, Dictionary<string, List<string>>>();

        /*************************************/
    }
}
