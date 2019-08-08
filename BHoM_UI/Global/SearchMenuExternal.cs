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

using BH.Engine.Reflection;
using BH.oM.UI;
using BH.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace BH.UI.Global
{
    public class SearchMenuExternal : SearchMenu_WinForm
    {
        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        protected override List<SearchItem> GetAllPossibleItems()
        {
            // All methods defined from the BHoM_UI
            // Reflection is pretty slow on this one so better to just do it manually even if less elegant
            List<SearchItem> items = new List<SearchItem>();

            items.AddRange(BH.Engine.UI.Query.ExternalComputeItems()
                .Where(x => x != null)
                .Select(x => new SearchItem { Item = x, CallerType = typeof(ExternalComputeCaller), Icon = Properties.Resources.ExternalCompute, Text = x.Method.ToText(true) }));

            return items;
        }

        /*************************************/
    }
}
