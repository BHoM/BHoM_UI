/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using BH.Adapter;
using BH.Engine.Reflection;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        [Description("Extracts all BHoM methods to be grouped as Engine items in the UI (combined group of all 5 basic Engine classes).")]
        [Output("items", "All BHoM methods to be grouped as Engine items.")]
        public static IEnumerable<MethodBase> EngineItems()
        {
            return Engine.Reflection.Query.BHoMMethodList().Where(x => x.IsExposed());
        }

        /***************************************************/

        [Description("Extracts all BHoM methods to be grouped as Create items in the UI.")]
        [Output("items", "All BHoM methods to be grouped as Create items.")]
        public static IEnumerable<MethodBase> CreateItems()
        {
            return Engine.Reflection.Query.BHoMMethodList()
                .Where(x => !x.IsNotImplemented() && !x.IsDeprecated() && x.DeclaringType.Name == "Create");
        }

        /***************************************************/

        [Description("Extracts all BHoM methods to be grouped as Compute items in the UI.")]
        [Output("items", "All BHoM methods to be grouped as Compute items.")]
        public static IEnumerable<MethodBase> ComputeItems()
        {
            return Engine.Reflection.Query.BHoMMethodList()
                .Where(x => !x.IsNotImplemented() && !x.IsDeprecated() && x.DeclaringType.Name == "Compute");
        }

        /***************************************************/

        [Description("Extracts all BHoM methods to be grouped as Convert items in the UI.")]
        [Output("items", "All BHoM methods to be grouped as Convert items.")]
        public static IEnumerable<MethodBase> ConvertItems()
        {
            return Engine.Reflection.Query.BHoMMethodList()
                .Where(x => !x.IsNotImplemented() && !x.IsDeprecated() && x.DeclaringType.Name == "Convert");
        }

        /***************************************************/

        [Description("Extracts all BHoM methods to be grouped as Modify items in the UI.")]
        [Output("items", "All BHoM methods to be grouped as Modify items.")]
        public static IEnumerable<MethodBase> ModifyItems()
        {
            return Engine.Reflection.Query.BHoMMethodList()
                .Where(x => !x.IsNotImplemented() && !x.IsDeprecated() && x.DeclaringType.Name == "Modify");
        }

        /***************************************************/

        [Description("Extracts all BHoM methods to be grouped as Query items in the UI.")]
        [Output("items", "All BHoM methods to be grouped as Query items.")]
        public static IEnumerable<MethodBase> QueryItems()
        {
            return Engine.Reflection.Query.BHoMMethodList()
                .Where(x => !x.IsNotImplemented() && !x.IsDeprecated() && x.DeclaringType.Name == "Query");
        }

        /***************************************************/

        [Description("Extracts all BHoM type constructors to be grouped as Create Adapter items in the UI.")]
        [Output("items", "All BHoM type constructors to be grouped as Create Adapter items.")]
        public static IEnumerable<MethodBase> AdapterConstructorItems()
        {
            return Engine.Reflection.Query.AdapterTypeList()
                .Where(x => x.IsSubclassOf(typeof(BHoMAdapter)))
                .SelectMany(x => x.GetConstructors())
                .Where(x => !x.IsNotImplemented() && !x.IsDeprecated());
        }

        /***************************************************/

        [Description("Extracts all BHoM type constructors to be grouped as Create Request items in the UI.")]
        [Output("items", "All BHoM type constructors to be grouped as Create Request items.")]
        public static IEnumerable<MethodBase> CreateRequestItems()
        {
            return BH.Engine.Reflection.Query.BHoMMethodList()
                .Where(x => x.DeclaringType.Name == "Create"
                && typeof(BH.oM.Data.Requests.IRequest).IsAssignableFrom(x.ReturnType)
                && !x.IsDeprecated())
                .OrderBy(x => x.Name);
        }

        /***************************************************/

        [Description("Extracts all BHoM types that implement IRequest interface and have a valid public constructor.")]
        [Output("items", "All BHoM types that implement IRequest interface and have a valid public constructor.")]
        public static IEnumerable<Type> ConstructableRequestItems()
        {
            return Engine.Reflection.Query.BHoMTypeList()
                .Where(x => x != null && !x.IsNotImplemented() && !x.IsDeprecated() && !x.IsEnum && !x.IsAbstract)
                .Where(x => typeof(BH.oM.Data.Requests.IRequest).IsAssignableFrom(x) && x.GetConstructors().Where(c => c.GetParameters().Count() > 0).Count() == 0);
        }

        /***************************************************/

        [Description("Extracts all types valid in BHoM.")]
        [Output("items", "All types valid in BHoM.")]
        public static IEnumerable<Type> TypeItems()
        {
            return Engine.Reflection.Query.BHoMTypeList()
                .Concat(Engine.Reflection.Query.BHoMInterfaceTypeList())
                .Concat(new List<Type> { typeof(Type), typeof(Enum),
                    typeof(object), typeof(bool), typeof(byte),
                    typeof(char), typeof(string),
                    typeof(float), typeof(double), typeof(decimal), typeof(short), typeof(int), typeof(long),
                    typeof(DateTime)})
                .Where(x => !x.IsNotImplemented() && !x.IsDeprecated());
        }

        /***************************************************/

        [Description("Extracts all types that have a valid public constructor.")]
        [Output("items", "All types that have a valid public constructor.")]
        public static IEnumerable<Type> ConstructableTypeItems()
        {
            return Engine.Reflection.Query.BHoMTypeList()
                .Where(x => x != null && !x.IsNotImplemented() && !x.IsDeprecated() && x.IsAutoConstructorAllowed() && !x.IsEnum && !x.IsAbstract)
                .Where(x => x.GetConstructors().Where(c => c.GetParameters().Count() > 0).Count() == 0);
        }

        /***************************************************/

        [Description("Extracts all enum types valid in BHoM.")]
        [Output("items", "All enum types valid in BHoM.")]
        public static IEnumerable<Type> EnumItems()
        {
            return Engine.Reflection.Query.BHoMEnumList()
                .Where(x => !x.IsNotImplemented() && !x.IsDeprecated());
        }

        /***************************************************/

        [Description("Extracts names of all BHoM library items.")]
        [Output("items", "Names of all BHoM library items.")]
        public static List<string> LibraryItems()
        {
            return Engine.Library.Query.LibraryNames().ToList();
        }

        /***************************************************/

        [Description("Extracts all external methods in BHoM.")]
        [Output("items", "All external methods in BHoM.")]
        public static List<MethodBase> ExternalItems()
        {
            return Engine.Reflection.Query.ExternalMethodList();
        }

        /***************************************************/
    }
}



