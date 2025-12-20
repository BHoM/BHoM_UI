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

using BH.Engine.Reflection;
using BH.oM.Base.Attributes;
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

        [Description("Checks if two parameter lists are matching.")]
        [Input("newList", "The new parameter list.")]
        [Input("oldList", "The old parameter list to compare against.")]
        [Output("matching", "True if the lists are matching, false otherwise.")]
        public static bool AreMatching(this List<ParamInfo> newList, List<ParamInfo> oldList)
        {
            if (newList.Count != oldList.Count)
                return false;
            else
                return newList.Zip(oldList, (a, b) => a.Name == b.Name).All(x => x);

        }

        /*************************************/

        [Description("Checks if a property list matches a parameter list.")]
        [Input("props", "The property list to check.")]
        [Input("oldList", "The parameter list to compare against.")]
        [Output("matching", "True if the lists are matching, false otherwise.")]
        public static bool AreMatching(this List<PropertyInfo> props, List<ParamInfo> oldList)
        {
            return oldList.All(x => props.Exists(p => p.Name == x.Name && p.PropertyType == x.DataType));
        }

        /*************************************/
    }
}







