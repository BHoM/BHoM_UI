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

using BH.Adapter;
using BH.Engine.Reflection;
using BH.oM.UI;
using BH.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BH.UI.Global
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
            container.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.B && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    if (m_SearchMenu == null)
                    {
                        m_SearchMenu = new SearchMenu_Wpf();
                        m_SearchMenu.ItemSelected += M_SearchMenu_ItemSelected;
                    }
                    if (Keyboard.IsKeyDown(Key.LeftAlt) && m_ExternalSearchMenu == null)
                    {
                        m_ExternalSearchMenu = new SearchMenuExternal();
                        m_ExternalSearchMenu.ItemSelected += M_SearchMenu_ItemSelected;
                    }
                    m_SearchMenu.SetParent(container.Content);
                    m_ExternalSearchMenu.SetParent(container.Content);
                }
            };

            return true;
        }

        /*************************************/

        public static bool Activate(System.Windows.Forms.ContainerControl container)
        {
            container.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == System.Windows.Forms.Keys.B && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    if (m_SearchMenu == null)
                    {
                        m_SearchMenu = new SearchMenu_WinForm();
                        m_SearchMenu.ItemSelected += M_SearchMenu_ItemSelected;
                    }
                    if (Keyboard.IsKeyDown(Key.LeftAlt) && m_ExternalSearchMenu == null)
                    {
                        m_ExternalSearchMenu = new SearchMenuExternal();
                        m_ExternalSearchMenu.ItemSelected += M_SearchMenu_ItemSelected;
                    }
                    m_SearchMenu.SetParent(container);
                    m_ExternalSearchMenu.SetParent(container);
                }
            };
            return true;
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private static void M_SearchMenu_ItemSelected(object sender, ComponentRequest request)
        {
            if (request != null)
                ItemSelected?.Invoke(sender, request);
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private static SearchMenu m_SearchMenu = null;

        private static SearchMenu m_ExternalSearchMenu = null;


        /*************************************/
    }

}
