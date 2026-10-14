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
using BH.oM.Base.Attributes;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description("Build the key identifying a type at runtime, stripping away assembly qualification and generic arguments.")]
        [Input("type", "Type to build the key for.")]
        [Output("key", "Key identifying the type, e.g. 'BH.oM.Geometry.Point' or 'BH.oM.Base.BHoMGroup`1' for a generic type definition.")]
        public static string TypeKey(Type type)
        {
            string key = type.FullName ?? type.Name;
            int cut = key.IndexOfAny(new char[] { ',', '[' });
            if (cut > 0)
                key = key.Substring(0, cut);
            return key;
        }

        /*************************************/

        [Description("Convert the text of a type, as written in the parameters of a method, into the key built by TypeKey(Type) for the equivalent type at runtime.")]
        [Input("typeName", "Text describing a type, as found in a method's parameter list, e.g. 'System.Collections.Generic.List<BH.oM.Geometry.Point>' or 'System.Double[]'.")]
        [Output("key", "Key matching the one TypeKey(Type) would build for the equivalent runtime type, e.g. 'System.Collections.Generic.List`1' or 'System.Double'.")]
        public static string TypeKey(string typeName)
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
    }
}







