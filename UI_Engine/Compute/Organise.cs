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

using BH.Engine.Data;
using BH.Engine.Base;
using BH.oM.Data.Collections;
using BH.oM.Base;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Compute
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static Output<List<SearchItem>, Tree<object>> IOrganise(this List<object> items)
        {
            List<Type> types = items.GroupBy(x => x.GetType()).Select(x => x.Key).ToList();

            if (types.Count == 1 && types[0] == typeof(MethodBase))
                return OrganiseMethods(items);
            else if (types.Count == 1 && (types[0] == typeof(Type) || types[0].Name == "RuntimeType"))
                return OrganiseTypes(items);
            else if (types.All(type => typeof(MemberInfo).IsAssignableFrom(type)))
                return OrganiseMembers(items);
            else
                return OrganiseObjects(items);
        }

        /*************************************/

        public static Output<List<SearchItem>, Tree<object>> OrganiseMethods(this List<object> methods)
        {
            // Create method list
            IEnumerable<string> paths = methods.Select(x => x.IToText(true).Replace("Engine", "oM.NonBHoMObjects"));
            List<SearchItem> list = paths.Zip(methods, (k, v) => new SearchItem { Text = k, Item = v }).ToList();

            //Create method tree
            List<string> toSkip = new List<string> { "Compute", "Convert", "Create", "External", "Modify", "Query" };
            Tree<object> tree = Data.Create.Tree(methods, paths.Select(x => x.Split('.').Except(toSkip).ToList()).ToList(), "Select a method");
            while (tree.Children.Count == 1 && tree.Children.Values.First().Children.Count > 0)
                tree.Children = tree.Children.Values.First().Children;
            tree = tree.GroupByName();
            
            return new Output<List<SearchItem>, Tree<object>> { Item1 = list, Item2 = tree };
        }

        /*************************************/

        public static Output<List<SearchItem>, Tree<object>> OrganiseTypes(this List<object> types)
        {
            // Create type list
            IEnumerable<string> paths = types.Select(x => x.IToText(true));
            List<SearchItem> list = paths.Zip(types, (k, v) => new SearchItem { Text = k, Item = v }).ToList();

            //Create type tree
            Tree<object> tree = Data.Create.Tree(types, paths.Select(x => x.Split('.').ToList()).ToList(), "select a type");
            while (tree.Children.Count == 1 && tree.Children.Values.First().Children.Count > 0)
                tree.Children = tree.Children.Values.First().Children;

            return new Output<List<SearchItem>, Tree<object>> { Item1 = list, Item2 = tree };
        }

        /*************************************/

        public static Output<List<SearchItem>, Tree<object>> OrganiseMembers(this List<object> members)
        {
            // Create method list
            IEnumerable<string> paths = members.Select(x => x is Type ? ((Type)x).ConstructorText() : x.IToText(true).Replace("Engine", "oM.NonBHoMObjects"));
            List<SearchItem> list = paths.Zip(members, (k, v) => new SearchItem { Text = k, Item = v }).ToList();

            //Create method tree
            List<string> toSkip = new List<string> { "Compute", "Convert", "Create", "External", "Modify", "Query" };
            Tree<object> tree = Data.Create.Tree(members, paths.Select(x => x.Split('.').Except(toSkip).ToList()).ToList(), "Select an item");
            while (tree.Children.Count == 1 && tree.Children.Values.First().Children.Count > 0)
                tree.Children = tree.Children.Values.First().Children;
            tree = tree.GroupByName();

            return new Output<List<SearchItem>, Tree<object>> { Item1 = list, Item2 = tree };
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private static Output<List<SearchItem>, Tree<object>> OrganiseObjects(this List<object> items)
        {
            // Create item list
            IEnumerable<string> paths = items.Select(x => x.ToString());
            List<SearchItem> list = paths.Zip(items, (k, v) => new SearchItem { Text = k, Item = v }).ToList();

            //Create ietm tree
            Tree<object> tree = Data.Create.Tree(items, paths.Select(x => x.Split(new char[] { '.', '/', '\\' }).ToList()).ToList(), "select an item");
            while (tree.Children.Count == 1 && tree.Children.Values.First().Children.Count > 0)
                tree.Children = tree.Children.Values.First().Children;

            return new Output<List<SearchItem>, Tree<object>> { Item1 = list, Item2 = tree };
        }


        /*************************************/
    }
}






