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

using BH.oM.Base;
using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description("Recursively parses a type name as serialised by BHoM, including the arguments of closed generic types, and splits the BH. type references it contains between those that are already assembly-qualified and those that are not and still need to be resolved to an assembly name by the caller.")]
        [Input("type", "Text describing a type, in one of the forms written by the BHoM serialiser, e.g. 'BH.oM.Geometry.Point', the assembly-qualified 'BH.oM.Geometry.Point, BHoM_Geometry, Version=...', or the closed generic 'BH.oM.Base.BHoMGroup`1[[BH.oM.Geometry.Point, BHoM_Geometry]]'.")]
        [MultiOutput(0, "assemblyNames", "Names of the assemblies already found qualifying a type in the input text.")]
        [MultiOutput(1, "typeNames", "BH. type names found in the input text that are not assembly-qualified and still need to be resolved to an assembly name.")]
        public static Output<List<string>, List<string>> AssemblyNamesInType(string type)
        {
            List<string> assemblyNames = new List<string>();
            List<string> typeNames = new List<string>();

            CollectTypeNames(type, assemblyNames, typeNames);

            return BH.Engine.Base.Create.Output(assemblyNames, typeNames);
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private static void CollectTypeNames(string type, List<string> assemblyNames, List<string> typeNames)
        {
            // Handles the forms a type can be written in:
            //   Name                                       e.g. BH.oM.Geometry.Point or BH.oM.Base.BHoMGroup`1
            //   Name, Assembly, Version=...                (assembly qualified, previous serialisation)
            //   Name`N[[Argument],[Argument]]              (closed generic, arguments with the same forms)
            // Commas can only be used to separate the name from the assembly when they are not inside the brackets of the generic arguments.
            if (string.IsNullOrWhiteSpace(type))
                return;

            string name = type.Trim();
            string assembly = null;

            int depth = 0;
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (c == '[')
                    depth++;
                else if (c == ']')
                    depth--;
                else if (c == ',' && depth == 0)
                {
                    // The assembly is the text between the first comma and the next one
                    string[] qualification = name.Substring(i + 1).Split(',');
                    assembly = qualification[0].Trim();
                    name = name.Substring(0, i).Trim();
                    break;
                }
            }

            // Separate the generic definition from its arguments
            int argumentsStart = name.IndexOf("[[");
            string definition = argumentsStart >= 0 ? name.Substring(0, argumentsStart) : name;

            if (definition.StartsWith("BH."))
            {
                if (!string.IsNullOrEmpty(assembly))
                    assemblyNames.Add(assembly);   // Assembly is already in the type
                else
                    typeNames.Add(definition);     // Assembly is not known here and will need to be resolved by the caller
            }

            if (argumentsStart >= 0)
            {
                foreach (string argument in GenericArguments(name.Substring(argumentsStart)))
                    CollectTypeNames(argument, assemblyNames, typeNames);
            }
        }

        /*************************************/

        private static List<string> GenericArguments(string arguments)
        {
            // Splits "[[A],[B, Assembly]]" into "A" and "B, Assembly". Arguments that are generic types themselves are kept whole.
            List<string> result = new List<string>();

            int depth = 0;
            int start = 0;
            for (int i = 0; i < arguments.Length; i++)
            {
                char c = arguments[i];
                if (c == '[')
                {
                    depth++;
                    if (depth == 2)
                        start = i + 1;
                }
                else if (c == ']')
                {
                    if (depth == 2)
                        result.Add(arguments.Substring(start, i - start));

                    depth--;
                    if (depth == 0)
                        break;
                }
            }

            return result;
        }

        /*************************************/
    }
}
