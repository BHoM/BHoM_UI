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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using BH.oM.Data.Collections;
using System.Windows;
using BH.oM.UI;
using BH.Engine.UI;

namespace BH.UI.Base.Menus
{
    public class ItemSelectorMenu_Wpf : ItemSelectorMenu<ContextMenu>
    {
        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public ItemSelectorMenu_Wpf(List<SearchItem> itemList, Tree<object> itemTree) : base(itemList, itemTree) { }


        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        protected override void AddTree(ContextMenu menu, Tree<object> itemTree)
        {
            menu.Items.Add(new Separator());

            m_ItemMenu = new MenuItem { Header = itemTree.Name };
            menu.Items.Add(m_ItemMenu);
            foreach (Tree<object> childTree in itemTree.Children.Values.OrderBy(x => x.Name))
                AppendMenuTree(childTree, m_ItemMenu);
        }

        /*************************************/

        protected override void AddSearchBox(ContextMenu menu, List<SearchItem> itemList)
        {
            m_ItemList = itemList;
            menu.SizeChanged += Menu_SizeChanged;
            menu.Items.Add(new Separator());

            m_SearchLabel = CreateMenuItem("Search");
            m_SearchLabel.FontWeight = FontWeights.Bold;
            menu.Items.Add(m_SearchLabel);

            m_Menu = menu;
            m_SearchBox = new TextBox { Text = "", HorizontalAlignment = HorizontalAlignment.Stretch };
            m_SearchBox.TextChanged += Search_TextChanged;
            menu.Items.Add(m_SearchBox);

            menu.Items.Add(new Separator());
        }

        /*************************************/
        /**** Protected Methods           ****/
        /*************************************/

        protected void AppendMenuTree(Tree<object> tree, MenuItem menu)
        {
            if (tree.Children.Count > 0)
            {
                MenuItem treeMenu = CreateMenuItem(tree.Name);
                menu.Items.Add(treeMenu);
                foreach (Tree<object> childTree in tree.Children.Values.OrderBy(x => x.Name))
                    AppendMenuTree(childTree, treeMenu);
            }
            else
            {
                object method = tree.Value;
                MenuItem methodItem = CreateMenuItem(tree.Name, Item_Click);
                menu.Items.Add(methodItem);
                m_ItemLinks[methodItem] = tree.Value;
            }
        }

        /*************************************/

        protected MenuItem CreateMenuItem(string text, EventHandler click = null, bool enabled = true, bool @checked = false)
        {
            MenuItem item = new MenuItem { Header = text, IsCheckable = @checked };
            if (click != null)
                item.Click += Item_Click;

            return item;
        }

        /*************************************/

        protected void Item_Click(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            if (m_ItemLinks.ContainsKey(item))
            {
                object link = m_ItemLinks[item];

                // Clear the old items
                foreach (MenuItem result in m_SearchResultItems)
                    m_Menu.Items.Remove(result);
                m_SearchResultItems.Clear();

                // Clear the selection menu and search bar
                m_Menu.Items.Remove(m_SearchBox);
                m_SearchBox = null;
                m_Menu.Items.Remove(m_ItemMenu);
                m_ItemTree = null;
                m_Menu.Items.Remove(m_SearchLabel);
                m_SearchLabel = null;

                ReturnSelectedItem(link);
            }
                
        }

        /*************************************/

        protected void Search_TextChanged(object sender, EventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box == null) return;

            // Clear the old items
            foreach (MenuItem item in m_SearchResultItems)
                m_Menu.Items.Remove(item);
            m_SearchResultItems.Clear();

            // Add the new ones
            foreach (SearchItem item in m_ItemList.Hits(m_SearchBox.Text, 12))
            {
                MenuItem methodItem = CreateMenuItem(item.Text, Item_Click);
                m_Menu.Items.Add(methodItem);
                m_SearchResultItems.Add(methodItem);
                m_ItemLinks[methodItem] = item.Item;
            }
        }

        /*************************************/

        private void Menu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // This is needed to force the TextBox to fill the menu (streatch property doesn't seem to do anything and I don't want to spend time finding out why)
            if (m_SearchBox != null && (Double.IsNaN(m_SearchBox.Width) || m_SearchBox.Width == 0))
                m_SearchBox.Width = m_Menu.Items.OfType<FrameworkElement>().Where(x => x != null).Select(x => x.ActualWidth).Aggregate((a, b) => Math.Max(a, b));
        }


        /*************************************/
        /**** Protected Fields            ****/
        /*************************************/

        protected ContextMenu m_Menu;
        protected MenuItem m_ItemMenu;
        protected TextBox m_SearchBox;
        protected MenuItem m_SearchLabel;
        protected Dictionary<MenuItem, object> m_ItemLinks = new Dictionary<MenuItem, object>();
        protected List<MenuItem> m_SearchResultItems = new List<MenuItem>();

        /*************************************/
    }
}

