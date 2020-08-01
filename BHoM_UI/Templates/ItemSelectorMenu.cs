/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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

using BH.oM.Data.Collections;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Menus
{
    public abstract class ItemSelectorMenu<T, M> : IItemSelectorMenu<T>
    {
        /*************************************/
        /**** Public Events               ****/
        /*************************************/

        public event EventHandler<T> ItemSelected;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public ItemSelectorMenu(List<SearchItem> itemList, Tree<T> itemTree)
        {
            m_ItemList = itemList;
            m_ItemTree = itemTree;
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public void FillMenu(M menu)
        {
            AddTree(menu, m_ItemTree);
            AddSearchBox(menu, m_ItemList);
        }

        /*************************************/

        public void FillMenu(object menu)
        {
            if (menu is M)
                FillMenu((M)menu);
        }

        /*************************************/

        public void SetItems(List<SearchItem> itemList, Tree<T> itemTree)
        {
            m_ItemList = itemList;
            m_ItemTree = itemTree;
        }


        /*************************************/
        /**** Protected Methods           ****/
        /*************************************/

        protected abstract void AddTree(M menu, Tree<T> itemTree);

        /*************************************/

        protected abstract void AddSearchBox(M menu, List<SearchItem> itemList);

        /*************************************/

        protected void ReturnSelectedItem(T item)
        {
            if (ItemSelected != null)
                ItemSelected(this, item);
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        protected List<SearchItem> m_ItemList = new List<SearchItem>();

        protected Tree<T> m_ItemTree = new Tree<T>();


        /*************************************/
    }
}

