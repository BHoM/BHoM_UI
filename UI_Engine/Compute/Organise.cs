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
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Data.Collections;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace BH.Engine.UI
{
    public static partial class Compute
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description("Organizes a list of search items into a tree structure.")]
        [Input("items", "The list of search items to organize.")]
        [Output("tree", "The tree structure of the organized code elements.")]
        public static Tree<SearchItem> Organise(this IEnumerable<SearchItem> items)
        {
            // Create the search item list
            List<SearchItem> list = items.ToList();

            //Create the element tree
            List<string> toSkip = new List<string> { "Compute", "Convert", "Create", "External", "Modify", "Query", "oM", "Engine" };
            Tree<SearchItem> tree = Data.Create.Tree(items.ToList(), items.Select(x => x.Text.Split('.').Except(toSkip).ToList()).ToList(), "Select an item");
            while (tree.Children.Count == 1 && tree.Children.Values.First().Children.Count > 0)
                tree.Children = tree.Children.Values.First().Children;
            tree = tree.GroupByName();

            return tree;
        }

        /*************************************/
    }
}






