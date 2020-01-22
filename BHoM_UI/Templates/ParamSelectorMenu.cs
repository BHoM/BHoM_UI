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

namespace BH.UI.Templates
{
    public class ParamSelectorMenu
    {
        /*************************************/
        /**** Public Events               ****/
        /*************************************/

        public event EventHandler<Tuple<ParamInfo, bool>> ParamToggled;

        public event EventHandler<List<Tuple<ParamInfo, bool>>> NewSelection;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public ParamSelectorMenu(List<Tuple<ParamInfo, bool>> parameters)
        {
            m_Params = parameters;
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public void AddParamList(ToolStripDropDown menu)
        {
            ToolStripMenuItem listMenu = AppendMenuItem(menu, "Add/Remove " + ParamLabel());
            listMenu.DropDown.Closed += Menu_Closing;

            listMenu.DropDown.Closing += (object sender, ToolStripDropDownClosingEventArgs e) => 
            {
                if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
                    e.Cancel = true;
            };

            foreach (Tuple<ParamInfo, bool> param in m_Params.OrderBy(x => x.Item1.Name))
                AppendMenuItem(listMenu.DropDown, param.Item1.Name, param.Item2);
        }

        /*************************************/

        public void AddParamList(System.Windows.Controls.ContextMenu menu)
        {
            System.Windows.Controls.MenuItem listMenu = new System.Windows.Controls.MenuItem { Header = "Add/Remove "+ ParamLabel() };
            listMenu.SubmenuClosed += Menu_Closing;
            menu.Items.Add(listMenu);

            foreach (Tuple<ParamInfo, bool> param in m_Params.OrderBy(x => x.Item1.Name))
                AppendMenuItem(listMenu, param.Item1.Name, param.Item2);
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
            int index = m_Params.FindIndex(x => x.Item1.Name == name);
            if (index >= 0)
                m_Params[index] = new Tuple<ParamInfo, bool>(m_Params[index].Item1, @checked);

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

        protected ToolStripMenuItem AppendMenuItem(ToolStrip menu, string text, bool @checked = false)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            item.Checked = @checked;
            item.Click += Item_CheckedChanged;
            menu.Items.Add(item);
            m_MenuItems[text] = item;
            return item;
        }

        /*************************************/

        protected System.Windows.Controls.MenuItem AppendMenuItem(System.Windows.Controls.MenuItem menu, string text, bool @checked = false)
        {
            System.Windows.Controls.MenuItem item = new System.Windows.Controls.MenuItem
            {
                Header = text,
                IsChecked = @checked,
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

            int index = m_Params.FindIndex(x => x.Item1.Name == text);
            if (index >= 0)
            {
                m_Params[index] = new Tuple<ParamInfo, bool>(m_Params[index].Item1, @checked);
                ParamToggled?.Invoke(this, m_Params[index]);
                m_SelectionChanged = true;
            }
        }

        /*************************************/

        protected void Menu_Closing(object sender, EventArgs e)
        {
            if (m_SelectionChanged)
                NewSelection?.Invoke(this, m_Params);
            m_SelectionChanged = false;
        }

        /*************************************/

        protected string ParamLabel()
        {
            var groups = m_Params.GroupBy(x => x.Item1.Kind);

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

        List<Tuple<ParamInfo, bool>> m_Params = new List<Tuple<ParamInfo, bool>>();
        Dictionary<string, object> m_MenuItems = new Dictionary<string, object>();
        bool m_SelectionChanged = false;

        /*************************************/
    }
}

