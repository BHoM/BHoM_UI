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
using BH.oM.Base;
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
                .GroupBy(x => BH.Engine.UI.Query.TypeKey(x.DisplayText))
                .ToDictionary(group => group.Key, group => group.Select(x => x.AssemblyName).Distinct().ToList());

            // Collect the relation between extension methods and the assembly they belong to
            Dictionary<string, Dictionary<string, List<string>>> assemblyNamesPerExtensionMethod = records.ExtensionMethodDictionary();

            // The maps are replaced rather than modified so that concurrent readers always see a consistent snapshot
            lock (m_Lock)
            {
                m_AssemblyNamePerType = BH.Engine.UI.Compute.Merge(m_AssemblyNamePerType, assemblyNamePerType);
                m_AssemblyNamesPerExtensionMethod = BH.Engine.UI.Compute.Merge(m_AssemblyNamesPerExtensionMethod, assemblyNamesPerExtensionMethod);
            }
        }

        /*************************************/

        [Description("Tell the resolver about types that are already loaded, mapping each one to the assembly that defines it. " +
                     "Useful for types that are not listed in the assembly content files, such as the ones defined by a UI itself. Can be called while the resolver is in use.")]
        [Input("types", "Loaded types to register. Each one is mapped, by its full name, to the name of its assembly.")]
        public void AddLoadedTypes(IEnumerable<Type> types)
        {
            if (types == null)
                return;

            Dictionary<string, List<string>> assemblyNamePerType = types
                .Where(x => x != null && !string.IsNullOrEmpty(x.FullName))
                .GroupBy(x => x.FullName)
                .ToDictionary(group => group.Key, group => group.Select(x => x.Assembly.GetName().Name).Distinct().ToList());

            lock (m_Lock)
            {
                m_AssemblyNamePerType = BH.Engine.UI.Compute.Merge(m_AssemblyNamePerType, assemblyNamePerType);
            }
        }

        /*************************************/

        [Description("Make sure assemblies that contain a type matching the input name are loaded. Return true if any assembly was loaded.")]
        public virtual bool MakeSureAssemblyIsLoadedForType(string type)
        {
            // The type can be a generic type with arguments from other assemblies, so BH. can be anywhere in the text
            if (string.IsNullOrEmpty(type) || !type.Contains("BH."))
                return false;

            Output<List<string>, List<string>> parsedType = BH.Engine.UI.Query.AssemblyNamesInType(type);
            List<string> assemblyNames = new List<string>(parsedType.Item1);

            // The types that were not assembly-qualified still need to be resolved using the maps collected by this resolver
            foreach (string typeName in parsedType.Item2)
            {
                if (m_AssemblyNamePerType.ContainsKey(typeName))
                    assemblyNames.AddRange(m_AssemblyNamePerType[typeName]);
                else
                    BH.Engine.Base.Compute.RecordWarning($"No assembly was found for this type: {typeName}.");
            }

            return LoadAssemblies(assemblyNames.Distinct(), $"type {type}", true);
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
            string exactTypeName = BH.Engine.UI.Query.TypeKey(targetType);
            if (typeToAssemblies.ContainsKey(exactTypeName))
                assembliesToLoad.UnionWith(typeToAssemblies[exactTypeName]);

            // 2. Base types
            Type baseType = targetType.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                string baseTypeName = BH.Engine.UI.Query.TypeKey(baseType);
                if (typeToAssemblies.ContainsKey(baseTypeName))
                    assembliesToLoad.UnionWith(typeToAssemblies[baseTypeName]);
                baseType = baseType.BaseType;
            }

            // 3. Interfaces
            foreach (Type interfaceType in targetType.GetInterfaces())
            {
                string interfaceName = BH.Engine.UI.Query.TypeKey(interfaceType);
                if (typeToAssemblies.ContainsKey(interfaceName))
                    assembliesToLoad.UnionWith(typeToAssemblies[interfaceName]);
            }

            // 4. Generic type definitions
            if (targetType.IsGenericType)
            {
                Type genericDef = targetType.GetGenericTypeDefinition();
                string genericTypeName = BH.Engine.UI.Query.TypeKey(genericDef);
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
        /**** Private Fields              ****/
        /*************************************/

        private readonly object m_Lock = new object();

        private Dictionary<string, List<string>> m_AssemblyNamePerType = new Dictionary<string, List<string>>();

        private Dictionary<string, Dictionary<string, List<string>>> m_AssemblyNamesPerExtensionMethod = new Dictionary<string, Dictionary<string, List<string>>>();

        /*************************************/
    }
}
