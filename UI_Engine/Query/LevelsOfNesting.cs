/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        [Description("Obtain how many levels of nesting are involved in an object, if the object is an IEnumerable object (List, array, etc.).")]
        [Input("obj", "Object to check how many levels of nesting it contains.")]
        [Output("levelsOfNesting", "0 if the object is not an IEnumerable or is a char[]. 1 if the object is a flat IEnumerable of objects (e.g. List<in>). 2 or more for greater nesting (e.g. 2 for List<List<int>>).")]
        public static int LevelsOfNesting(object obj)
        {
            var type = obj.GetType();

            int levelsOfNest = 0;
            while (typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string))
            {
                try
                {
                    obj = ((IEnumerable<object>)obj).FirstOrDefault();
                    type = obj.GetType();
                }
                catch
                {
                    break;
                }
                levelsOfNest++;
            }

            return levelsOfNest;
        }
    }
}
