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

using BH.oM.Base.Attributes;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description("Builds the relation between extension methods and the assemblies defining them, from a collection of code elements.")]
        [Input("codeElements", "Code elements describing the content of the assemblies (typically read from an AssemblyContent.tsv file).")]
        [Output("assemblyNamesPerExtensionMethod", "For each extension method name, the names of the assemblies defining an overload for each first parameter type.")]
        public static Dictionary<string, Dictionary<string, List<string>>> ExtensionMethodDictionary(this IEnumerable<CodeElementRecord> codeElements)
        {
            Dictionary<string, Dictionary<string, List<string>>> result
                = new Dictionary<string, Dictionary<string, List<string>>>();

            if (codeElements == null)
                return result;

            foreach (CodeElementRecord record in codeElements.Where(x =>
                x != null && (
                x.Type == CodeElementType.Method_Query ||
                x.Type == CodeElementType.Method_Compute ||
                x.Type == CodeElementType.Method_Convert ||
                x.Type == CodeElementType.Method_Modify)))
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
        /**** Private Methods             ****/
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

                    return TypeKey(json.Substring(firstParamStart + 1, plainEnd - firstParamStart - 1));
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
    }
}
