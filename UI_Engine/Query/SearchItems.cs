/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static IEnumerable<MethodBase> EngineItems()
        {
            return Engine.Reflection.Query.BHoMMethodList().Where(x => !x.IsNotImplemented() && !x.IsDeprecated());
        }

        /***************************************************/

        public static IEnumerable<MethodBase> CreateItems()
        {
            return Engine.Reflection.Query.BHoMMethodList()
                .Where(x => !x.IsNotImplemented() && !x.IsDeprecated() && x.DeclaringType.Name == "Create");
        }

        /***************************************************/

        public static IEnumerable<MethodBase> ComputeItems()
        {
            return Engine.Reflection.Query.BHoMMethodList()
                .Where(x => !x.IsNotImplemented() && !x.IsDeprecated() && x.DeclaringType.Name == "Compute");
        }

        /***************************************************/

        public static IEnumerable<MethodBase> ConvertItems()
        {
            return Engine.Reflection.Query.BHoMMethodList()
                .Where(x => !x.IsNotImplemented() && !x.IsDeprecated() && x.DeclaringType.Name == "Convert");
        }

        /***************************************************/

        public static IEnumerable<MethodBase> ModifyItems()
        {
            return Engine.Reflection.Query.BHoMMethodList()
                .Where(x => !x.IsNotImplemented() && !x.IsDeprecated() && x.DeclaringType.Name == "Modify");
        }

        /***************************************************/

        public static IEnumerable<MethodBase> QueryItems()
        {
            return Engine.Reflection.Query.BHoMMethodList()
                .Where(x => !x.IsNotImplemented() && !x.IsDeprecated() && x.DeclaringType.Name == "Query");
        }

        /***************************************************/

        public static IEnumerable<MethodBase> AdapterConstructorItems()
        {
            return Engine.Reflection.Query.AdapterTypeList()
                .Where(x => x.IsSubclassOf(typeof(BHoMAdapter)))
                .SelectMany(x => x.GetConstructors())
                .Where(x => !x.IsNotImplemented() && !x.IsDeprecated());
        }

        /***************************************************/

        public static IEnumerable<MethodBase> CreateRequestItems()
        {
            return BH.Engine.Reflection.Query.BHoMMethodList()
                .Where(x => x.DeclaringType.Name == "Create"
                && typeof(BH.oM.Data.Requests.IRequest).IsAssignableFrom(x.ReturnType)
                && !x.IsDeprecated())
                .OrderBy(x => x.Name);
        }

        /***************************************************/

        public static IEnumerable<Type> TypeItems()
        {
            return Engine.Reflection.Query.BHoMTypeList()
                .Concat(Engine.Reflection.Query.BHoMInterfaceList())
                .Concat(new List<Type> { typeof(object), typeof(Type), typeof(Enum), typeof(bool), typeof(byte), typeof(char), typeof(double), typeof(int), typeof(string) })
                .Where(x => !x.IsNotImplemented() && !x.IsDeprecated());
        }

        /***************************************************/

        public static IEnumerable<Type> EnumItems()
        {
            return Engine.Reflection.Query.BHoMEnumList()
                .Where(x => !x.IsNotImplemented() && !x.IsDeprecated());
        }

        /***************************************************/

        public static List<string> LibraryItems()
        {
            return Engine.Library.Query.LibraryNames().ToList();
        }

        /***************************************************/
    }
}
