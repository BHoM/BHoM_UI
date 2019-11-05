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
    public class InputSelectorMenu
    {
        /*************************************/
        /**** Public Events               ****/
        /*************************************/

        public event EventHandler<Tuple<string, bool>> InputToggled;

        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public InputSelectorMenu(List<Tuple<string, bool>> inputs)
        {
            m_Inputs = inputs.ToDictionary(x => x.Item1, x => x.Item2);
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        protected void AddInputList(ToolStripDropDown menu)
        {
            ToolStripMenuItem treeMenu = AppendMenuItem(menu, "Override Inputs");
            foreach (KeyValuePair<string, bool> input in m_Inputs)
                AppendMenuItem(treeMenu.DropDown, input.Key, input.Value);
        }


        /*************************************/
        /**** Protected Methods           ****/
        /*************************************/

        protected ToolStripMenuItem AppendMenuItem(ToolStrip menu, string text, bool @checked = false)
        {
            ToolStripMenuItem item = new ToolStripMenuItem(text);
            item.Checked = @checked;
            item.CheckedChanged += Item_CheckedChanged;
            menu.Items.Add(item);
            return item;
        }

        /*************************************/

        protected System.Windows.Controls.MenuItem AppendMenuItem(System.Windows.Controls.ContextMenu menu, string text, bool @checked = false)
        {
            System.Windows.Controls.MenuItem item = new System.Windows.Controls.MenuItem
            {
                Header = text,
                IsChecked = @checked,
            };
            item.Checked += Item_CheckedChanged;
            menu.Items.Add(item);
            return item;
        }

        /*************************************/

        private void Item_CheckedChanged(object sender, EventArgs e)
        {
            string text = "";
            bool @checked = false;

            if (sender is ToolStripMenuItem)
            {
                ToolStripMenuItem item = sender as ToolStripMenuItem;
                text = item.Text;
                @checked = item.Checked;
            }
            else if (sender is MenuItem)
            {
                System.Windows.Controls.MenuItem item = sender as System.Windows.Controls.MenuItem;
                text = item.Header.ToString();
                @checked = item.IsChecked;
            }

            if (m_Inputs.ContainsKey(text))
            {
                m_Inputs[text] = @checked;
                InputToggled?.Invoke(this, new Tuple<string, bool>(text, @checked));
            }
        }

        /*************************************/
        /**** Protected Fields            ****/
        /*************************************/

        Dictionary<string, bool> m_Inputs = new Dictionary<string, bool>();

        /*************************************/
    }
}
