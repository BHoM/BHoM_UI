/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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
using BH.Engine.Reflection;
using BH.oM.Data.Collections;
using BH.oM.Reflection;
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

        public static Output<List<SearchItem>, Tree<T>> OrganiseItems<T>(this List<T> items)
        {
            if (typeof(T) == typeof(MethodBase))
                return OrganiseMethods(items.Cast<MethodBase>().ToList()) as Output<List<SearchItem>, Tree<T>>;
            if (typeof(T) == typeof(Type))
                return OrganiseTypes(items.Cast<Type>().ToList()) as Output<List<SearchItem>, Tree<T>>;
            else
                return OrganiseOthers(items) as Output<List<SearchItem>, Tree<T>>;
        }

        /*************************************/

        public static Output<List<SearchItem>, Tree<MethodBase>> OrganiseMethods(this List<MethodBase> methods)
        {
            // Create method list
            IEnumerable<string> paths = methods.Select(x => x.ToText(true).Replace("Engine", "oM.NonBHoMObjects"));
            List<SearchItem> list = paths.Zip(methods, (k, v) => new SearchItem { Text = k, Item = v }).ToList();

            //Create method tree
            List<string> toSkip = new List<string> { "Compute", "Convert", "Create", "Modify", "Query" };
            Tree<MethodBase> tree = Data.Create.Tree(methods, paths.Select(x => x.Split('.').Except(toSkip).ToList()).ToList(), "Select a method");
            while (tree.Children.Count == 1 && tree.Children.Values.First().Children.Count > 0)
                tree.Children = tree.Children.Values.First().Children;
            tree = tree.GroupMethodsByName();
            
            return new Output<List<SearchItem>, Tree<MethodBase>> { Item1 = list, Item2 = tree };
        }

        /*************************************/

        public static Output<List<SearchItem>, Tree<Type>> OrganiseTypes(this List<Type> types)
        {
            // Create type list
            IEnumerable<string> paths = types.Select(x => x.ToText(true));
            List<SearchItem> list = paths.Zip(types, (k, v) => new SearchItem { Text = k, Item = v }).ToList();

            //Create type tree
            Tree<Type> tree = Data.Create.Tree(types, paths.Select(x => x.Split('.').ToList()).ToList(), "select a type");
            while (tree.Children.Count == 1 && tree.Children.Values.First().Children.Count > 0)
                tree.Children = tree.Children.Values.First().Children;

            return new Output<List<SearchItem>, Tree<Type>> { Item1 = list, Item2 = tree };
        }

        /*************************************/

        public static Output<List<SearchItem>, Tree<T>> OrganiseOthers<T>(this List<T> items)
        {
            // Create item list
            IEnumerable<string> paths = items.Select(x => x.ToString());
            List<SearchItem> list = paths.Zip(items, (k, v) => new SearchItem { Text = k, Item = v }).ToList();

            //Create ietm tree
            Tree<T> tree = Data.Create.Tree(items, paths.Select(x => x.Split(new char[] { '.', '/', '\\' }).ToList()).ToList(), "select an item");
            while (tree.Children.Count == 1 && tree.Children.Values.First().Children.Count > 0)
                tree.Children = tree.Children.Values.First().Children;

            return new Output<List<SearchItem>, Tree<T>> { Item1 = list, Item2 = tree };
        }

        /*************************************/
    }
}
