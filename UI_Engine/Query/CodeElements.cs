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
using BH.Engine.Serialiser;
using BH.oM.Base.Attributes;
using BH.oM.Data.Requests;
using BH.oM.UI;
using System;
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

        [Description("Collect all the code elements that can be used to create UI components from the loaded assemblies.")]
        [Output("codeElements", "All code elements already loaded that can be used in the UI to create components")]
        public static List<CodeElementRecord> CodeElements()
        {
            List<CodeElementRecord> items = new List<CodeElementRecord>();

            // All adapter constructors
            items.AddRange(BH.Engine.UI.Query.AdapterConstructorItems()
                .Select(x => new CodeElementRecord { AssemblyName = AssemblyName(x), Type = CodeElementType.AdapterConstructor, DisplayText = x.ToText(true), Json = x.ToJson() }));

            // All constructable BHoM objects and requests
            items.AddRange(BH.Engine.UI.Query.ConstructableTypeItems()
                .Select(x => new CodeElementRecord { AssemblyName = AssemblyName(x), Type = GetConstructableType(x), DisplayText = x.ConstructorText(), Json = x.ToJson() }));

            // All Enums
            items.AddRange(BH.Engine.UI.Query.EnumItems()
                .Select(x => new CodeElementRecord { AssemblyName = AssemblyName(x), Type = CodeElementType.Enum, DisplayText = x.ToText(true), Json = x.ToJson() }));

            // All methods for the BHoM Engine (including creators)
            items.AddRange(BH.Engine.Base.Query.BHoMMethodList()
                        .Where(x => x.IsExposed())
                        .Select(x => new CodeElementRecord { AssemblyName = AssemblyName(x), Type = GetMethodType(x), DisplayText = x.ToText(true), Json = x.ToJson() }));

            // All methods from external class
            items.AddRange(BH.Engine.UI.Query.ExternalItems()
                .Select(x => new CodeElementRecord { AssemblyName = AssemblyName(x), Type = CodeElementType.Method_External, DisplayText = x.ToText(true), Json = x.ToJson() }));

            // All Types
            items.AddRange(BH.Engine.UI.Query.TypeItems()
                .Select(x => new CodeElementRecord { AssemblyName = AssemblyName(x), Type = CodeElementType.Type, DisplayText = x.ToText(true), Json = x.ToJson() }));

            // Return the list
            return items;
        }


        /*************************************/
        /**** Public Methods              ****/
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






