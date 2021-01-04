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

using BH.Engine.Reflection;
using BH.Engine.UI;
using BH.oM.Data.Collections;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BH.UI.Base.Menus
{
    public class ParamSelectorMenu
    {
        /*************************************/
        /**** Public Events               ****/
        /*************************************/

        public event EventHandler<List<int>> SelectionChanged;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public ParamSelectorMenu(List<ParamInfo> parameters)
        {
            m_Params = parameters.ToList();
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public void AddParamList(ToolStripDropDown menu)
        {
            ToolStripMenuItem listMenu = AppendMenuItem(menu, "Add/Remove " + ParamLabel());
            listMenu.DropDown.Opened += Menu_Opening;
            listMenu.DropDown.Closed += Menu_Closing;

            listMenu.DropDown.Closing += (object sender, ToolStripDropDownClosingEventArgs e) => 
            {
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                    e.Cancel = true;
            };

            menu.Opened += (sender, e) =>
            {
                listMenu.DropDown.Items.Clear();
                foreach (ParamInfo param in m_Params)
                    AppendMenuItem(listMenu.DropDown, param.Name, param.IsSelected, !param.IsRequired);
            };   
        }

        /*************************************/

        public void AddParamList(System.Windows.Controls.ContextMenu menu)
        {
            System.Windows.Controls.MenuItem listMenu = new System.Windows.Controls.MenuItem { Header = "Add/Remove "+ ParamLabel() };
            listMenu.SubmenuOpened += Menu_Opening;
            listMenu.SubmenuClosed += Menu_Closing;
            menu.Items.Add(listMenu);

            menu.Opened += (sender, e) =>
            {
                listMenu.Items.Clear();
                foreach (ParamInfo param in m_Params)
                    AppendMenuItem(listMenu, param.Name, param.IsSelected, !param.IsRequired);
            };
        }

        /*************************************/

        public void AddParamList(object menu)
        {
            if (menu is ToolStripDropDown)
                AddParamList(menu as ToolStripDropDown);
            else if (menu is System.Windows.Controls.ContextMenu)
                AddParamList(menu as System.Windows.Controls.ContextMenu);
        }

        /*************************************/

        public void SetParamCheck(string name, bool @checked)
        {
            int index = m_Params.FindIndex(x => x.Name == name);
            if (index >= 0)
                m_Params[index].IsSelected = @checked;

            if (m_MenuItems.ContainsKey(name))
            {
                object item = m_MenuItems[name];
                if (item is ToolStripMenuItem)
                    ((ToolStripMenuItem)item).Checked = @checked;
                else if (item is System.Windows.Controls.MenuItem)
                    ((System.Windows.Controls.MenuItem)item).IsChecked = @checked;
            }
        }


        /*************************************/
        /**** Protected Methods           ****/
        /*************************************/

        protected ToolStripMenuItem AppendMenuItem(ToolStrip menu, string text, bool @checked = false, bool enabled = true)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            item.Checked = @checked;
            item.Enabled = enabled;
            item.Click += Item_CheckedChanged;
            menu.Items.Add(item);
            m_MenuItems[text] = item;
            return item;
        }

        /*************************************/

        protected System.Windows.Controls.MenuItem AppendMenuItem(System.Windows.Controls.MenuItem menu, string text, bool @checked = false, bool enabled = true)
        {
            System.Windows.Controls.MenuItem item = new System.Windows.Controls.MenuItem
            {
                Header = text,
                IsChecked = @checked,
                IsEnabled = enabled
            };
            item.Click += Item_CheckedChanged;
            menu.Items.Add(item);
            m_MenuItems[text] = item;
            return item;
        }

        /*************************************/

        protected void Item_CheckedChanged(object sender, EventArgs e)
        {
            string text = "";
            bool @checked = false;

            if (sender is ToolStripMenuItem)
            {
                ToolStripMenuItem item = sender as ToolStripMenuItem;
                text = item.Text;
                @checked = !item.Checked;
                item.Checked = @checked;
            }
            else if (sender is System.Windows.Controls.MenuItem)
            {
                System.Windows.Controls.MenuItem item = sender as System.Windows.Controls.MenuItem;
                text = item.Header.ToString();
                @checked = !item.IsChecked;
                item.IsChecked = @checked;
            }

            int index = m_Params.FindIndex(x => x.Name == text);
            if (index >= 0)
                m_Params[index].IsSelected = @checked;
        }

        /*************************************/

        protected void Menu_Opening(object sender, EventArgs e)
        {
            m_OriginalSelection = m_Params.Select((x, i) => new { i, x.IsSelected }).ToDictionary(x => x.i, x => x.IsSelected);
        }

        /*************************************/

        protected void Menu_Closing(object sender, EventArgs e)
        {
            if (m_OriginalSelection.Count > 0)
            {
                List<int> changedIndices = m_OriginalSelection.Where(x => x.Value != m_Params[x.Key].IsSelected).Select(x => x.Key).ToList();
                SelectionChanged?.Invoke(this, changedIndices);
            }
            m_OriginalSelection = new Dictionary<int, bool>();
        }

        /*************************************/

        protected string ParamLabel()
        {
            var groups = m_Params.GroupBy(x => x.Kind);

            if (groups.Count() == 1)
            {
                if (groups.First().Key == ParamKind.Input)
                    return "Inputs";
                else
                    return "Outputs";
            }
            else
                return "Params";
            
        }

        /*************************************/
        /**** Protected Fields            ****/
        /*************************************/

        List<ParamInfo> m_Params = new List<ParamInfo>();
        Dictionary<string, object> m_MenuItems = new Dictionary<string, object>();
        Dictionary<int, bool> m_OriginalSelection = new Dictionary<int, bool>();

        /*************************************/
    }
}


