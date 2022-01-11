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

using BH.Engine.UI;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using BH.oM.Data.Collections;
using System.Windows.Forms;
using BH.Engine.Reflection;
using BH.oM.Base;

namespace BH.UI.Base.Menus
{
    public class ItemSelector
    {
        /*************************************/
        /**** Events                      ****/
        /*************************************/

        public event EventHandler<object> ItemSelected;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public ItemSelector(IEnumerable<object> possibleItems, string key) 
        {
            m_Key = key;

            if (!m_ItemTreeStore.ContainsKey(key) || !m_ItemListStore.ContainsKey(key))
            {
                List<object> toKeep = possibleItems.Where(x => x.IIsExposed()).ToList();
                Output<List<SearchItem>, Tree<object>> organisedMethod = Engine.UI.Compute.IOrganise(toKeep);
                m_ItemListStore[key] = organisedMethod.Item1;
                m_ItemTreeStore[key] = organisedMethod.Item2;
            }
        }

        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public void AddToMenu(ToolStripDropDown menu)
        {
            if (m_SelectorMenu == null)
                SetSelectorMenu(new ItemSelectorMenu_WinForm(m_ItemListStore[m_Key], m_ItemTreeStore[m_Key]));

            m_SelectorMenu.FillMenu(menu);
        }

        /*************************************/

        public void AddToMenu(System.Windows.Controls.ContextMenu menu)
        {
            if (m_SelectorMenu == null)
                SetSelectorMenu(new ItemSelectorMenu_Wpf(m_ItemListStore[m_Key], m_ItemTreeStore[m_Key]));

            m_SelectorMenu.FillMenu(menu);
        }

        /*************************************/

        public void AddToMenu(object menu)
        {
            if (m_SelectorMenu != null)
                m_SelectorMenu.FillMenu(menu);
        }

        /*************************************/

        public void SetSelectorMenu<M>(ItemSelectorMenu<M> selectorMenu)
        {
            selectorMenu.SetItems(m_ItemListStore[m_Key], m_ItemTreeStore[m_Key]);
            selectorMenu.ItemSelected += M_Menu_ItemSelected;

            m_SelectorMenu = selectorMenu;
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private void M_Menu_ItemSelected(object sender, object e)
        {
            ItemSelected?.Invoke(this, e);
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        protected string m_Key = "";
        protected IItemSelectorMenu m_SelectorMenu = null;

        protected static Dictionary<string, Tree<object>> m_ItemTreeStore = new Dictionary<string, Tree<object>>();
        protected static Dictionary<string, List<SearchItem>> m_ItemListStore = new Dictionary<string, List<SearchItem>>();


        /*************************************/
    }
}



