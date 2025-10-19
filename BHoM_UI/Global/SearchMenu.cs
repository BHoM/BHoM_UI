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
using BH.Engine.UI;
using BH.oM.Data.Requests;
using BH.oM.UI;
using BH.UI.Base.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace BH.UI.Base.Global
{
    public abstract class SearchMenu
    {
        /*************************************/
        /**** Events                      ****/
        /*************************************/

        public event EventHandler<ComponentRequest> ItemSelected;

        public event EventHandler Disposed;


        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public List<SearchItem> PossibleItems { get; set; } = new List<SearchItem>();

        public int NbHits { get; set; } = 20;

        public bool HitsOnEmptySearch { get; set; } = false;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public SearchMenu()
        {
            PossibleItems = Initialisation.SearchItems.ToList();
            Initialisation.CompletionTime = DateTime.UtcNow;
        }

        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public abstract bool SetParent(object parent);

        protected virtual void RefreshSearchResults(List<SearchItem> hits) { }

        protected virtual void SetSearchText(string searchText) { }

        /*************************************/

        public void ShowResults(string searchText)
        {
            SetSearchText(searchText);
            RefreshSearchResults(PossibleItems.Hits(searchText, NbHits, HitsOnEmptySearch));
        }


        /*************************************/
        /**** Protected Methods           ****/
        /*************************************/

        protected void NotifySelection(SearchItem item)
        {
            NotifySelection(item, null);
        }

        /*************************************/

        protected void NotifySelection(SearchItem item, BH.oM.Geometry.Point location)
        {
            if (item == null)
                ItemSelected?.Invoke(this, null);
            else
            {
                if (item.Item == null && !string.IsNullOrEmpty(item.Json))
                    item.Item = BH.Engine.Serialiser.Convert.FromJson(item.Json);
                ItemSelected?.Invoke(this, new ComponentRequest { CallerType = item.CallerType, SelectedItem = item.Item, Location = location });
            }
                
        }

        /*************************************/

        protected void NotifyDispose()
        {
            Disposed?.Invoke(this, null);
        }

        /*************************************/
    }
}






