/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.Engine.UI;
using BH.oM.UI;
using BH.UI.Base.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BH.UI.Base.Global
{
    public static class GlobalSearch
    {
        /*************************************/
        /**** Public Events               ****/
        /*************************************/

        public static event EventHandler<ComponentRequest> ItemSelected;


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static bool Activate(ContentControl container)
        {
            if (m_SearchMenu == null)
            {
                m_SearchMenu = new SearchMenu_Wpf();
                m_SearchMenu.ItemSelected += M_SearchMenu_ItemSelected;
                m_SearchMenu.Disposed += M_SearchMenu_Disposed;
                m_PossibleItems = m_SearchMenu.PossibleItems;
            }

            container.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.B && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                    Open(container);
            };

            return true;
        }

        /*************************************/

        public static bool Activate(System.Windows.Forms.ContainerControl container)
        {
            if (m_SearchMenu == null)
            {
                m_SearchMenu = new SearchMenu_WinForm();
                m_SearchMenu.ItemSelected += M_SearchMenu_ItemSelected;
                m_SearchMenu.Disposed += M_SearchMenu_Disposed;
                m_PossibleItems = m_SearchMenu.PossibleItems;
            }

            container.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == System.Windows.Forms.Keys.B && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                    Open(container);
            };
            return true;
        }

        /*************************************/

        public static void Open(ContentControl container, SearchConfig config = null)
        {
            if (m_SearchMenu == null)
                return;

            m_SearchMenu.SetParent(container.Content);
            ShowWithConstraint(config);
        }

        /*************************************/

        public static void Open(System.Windows.Forms.ContainerControl container, SearchConfig config = null)
        {
            if (m_SearchMenu == null)
                return;

            m_SearchMenu.SetParent(container);
            ShowWithConstraint(config);
        }

        /*************************************/

        public static void AddPossibleItems(List<SearchItem> items)
        {
            if (m_SearchMenu != null)
                m_SearchMenu.PossibleItems.AddRange(items);
        }

        /*************************************/

        public static void RemoveHandler(string declaringType)
        {
            if (ItemSelected != null)
            {
                foreach (EventHandler<ComponentRequest> d in ItemSelected.GetInvocationList())
                {
                    if (d.Method.DeclaringType.FullName == declaringType)
                        ItemSelected -= d;
                }
            }
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private static void M_SearchMenu_ItemSelected(object sender, ComponentRequest request)
        {
            ItemSelected?.Invoke(sender, request);
        }

        /*************************************/

        private static void M_SearchMenu_Disposed(object sender, EventArgs e)
        {
            m_SearchMenu = null;
        }

        /*************************************/

        private static void ShowWithConstraint(SearchConfig config)
        {
            if (config != null)
            {
                m_SearchMenu.PossibleItems = m_PossibleItems.Select(x =>
                {
                    SearchItem withWeight = x.GetShallowClone() as SearchItem;
                    withWeight.Weight = withWeight.Weight(config);
                    return withWeight;
                }).Where(x => x.Weight > 0).ToList();
            }
            else
            {
                m_SearchMenu.PossibleItems = m_PossibleItems;
            }

            m_SearchMenu.HitsOnEmptySearch = config != null;
            m_SearchMenu.ShowResults("");
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private static SearchMenu m_SearchMenu = null;
        private static List<SearchItem> m_PossibleItems = new List<SearchItem>();

        /*************************************/
    }

}


