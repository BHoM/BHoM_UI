/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using BH.Engine.UI;
using BH.oM.Data.Collections;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BH.UI.Base.Menus
{
    public class ItemSelectorMenu_WinForm : ItemSelectorMenu<ToolStripDropDown>
    {
        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public ItemSelectorMenu_WinForm(List<SearchItem> itemList, Tree<object> itemTree) : base(itemList, itemTree) { }


        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        protected override void AddTree(ToolStripDropDown menu, Tree<object> itemTree)
        {
            if (itemTree != null)
                AppendMenuTree(itemTree, menu);
        }

        /*************************************/

        protected override void AddSearchBox(ToolStripDropDown menu, List<SearchItem> itemList)
        {
            m_ItemList = itemList;

            AppendMenuSeparator(menu);
            ToolStripMenuItem label = AppendMenuItem(menu, "Search");
            label.Font = new System.Drawing.Font(label.Font, System.Drawing.FontStyle.Bold);

            m_Menu = menu;
            m_SearchBox = new ToolStripTextBox { Text = "", BorderStyle = BorderStyle.FixedSingle };
            m_SearchBox.TextChanged += Search_TextChanged;
            menu.Items.Add(m_SearchBox);
        }


        /*************************************/
        /**** Protected Methods           ****/
        /*************************************/

        protected void AppendMenuTree(Tree<object> tree, ToolStripDropDown menu)
        {
            if (tree.Children.Count > 0)
            {
                ToolStripMenuItem treeMenu = AppendMenuItem(menu, tree.Name);
                treeMenu.Paint += (s, e) =>
                {
                    if (treeMenu.DropDown.Items.Count == 0)
                    {
                        foreach (Tree<object> childTree in tree.Children.Values.OrderBy(x => x.Name))
                            AppendMenuTree(childTree, treeMenu.DropDown);
                        treeMenu.Invalidate();
                    }
                };
            }
            else
            {
                object method = tree.Value;
                ToolStripMenuItem methodItem = AppendMenuItem(menu, tree.Name, Item_Click);
                m_ItemLinks[methodItem] = tree.Value;
                methodItem.ToolTipText = method.IDescription();
            }
        }

        /*************************************/

        protected ToolStripMenuItem AppendMenuItem(ToolStrip menu, string text, EventHandler click = null, bool enabled = true, bool @checked = false)
        {
            ToolStripMenuItem item;
            if (click == null)
                item = new ToolStripMenuItem(text);
            else
                item = new ToolStripMenuItem(text, null, click);

            item.Enabled = enabled;
            item.Checked = @checked;
            menu.Items.Add(item);
            return item;
        }

        /*************************************/

        protected ToolStripSeparator AppendMenuSeparator(ToolStrip menu)
        {
            if (menu.Items.Count == 0)
                return null;

            ToolStripItem lastItem = menu.Items[menu.Items.Count - 1];
            if (lastItem is ToolStripSeparator)
                return null;

            ToolStripSeparator separator = new ToolStripSeparator();
            menu.Items.Add(separator);
            return separator;
        }

        /*************************************/

        protected void Item_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            if (m_ItemLinks.ContainsKey(item))
                ReturnSelectedItem(m_ItemLinks[item]);
        }

        /*************************************/

        protected void Search_TextChanged(object sender, EventArgs e)
        {
            // Clear the old items
            foreach (ToolStripItem item in m_SearchResultItems)
                item.Dispose();
            m_SearchResultItems.Clear();

            // Add the new ones
            m_SearchResultItems.Add(AppendMenuSeparator(m_Menu));
            foreach (SearchItem item in m_ItemList.Hits(m_SearchBox.Text, 12))
            {
                ToolStripMenuItem methodItem = AppendMenuItem(m_Menu, item.Text, Item_Click);
                methodItem.ToolTipText = item.Item.IDescription();
                m_SearchResultItems.Add(methodItem);
                m_ItemLinks[methodItem] = item.Item;
            }
        }


        /*************************************/
        /**** Protected Fields            ****/
        /*************************************/

        protected ToolStripDropDown m_Menu;
        protected ToolStripTextBox m_SearchBox;
        protected Dictionary<ToolStripMenuItem, object> m_ItemLinks = new Dictionary<ToolStripMenuItem, object>();
        protected List<ToolStripItem> m_SearchResultItems = new List<ToolStripItem>();

        /*************************************/
    }
}





