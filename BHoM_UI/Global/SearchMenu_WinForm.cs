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

using BH.Engine.UI;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace BH.UI.Global
{
    public class SearchMenu_WinForm : SearchMenu
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public override bool SetParent(object parent)
        {
            ContainerControl container = parent as ContainerControl;
            if (container == null)
                return false;

            if (m_Popup == null)
            {
                // Create the popup form
                m_Popup = new Form()
                {
                    FormBorderStyle = FormBorderStyle.FixedSingle,
                    KeyPreview = true,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    ShowIcon = false,
                    ShowInTaskbar = false,
                    SizeGripStyle = SizeGripStyle.Hide,
                    StartPosition = FormStartPosition.Manual,
                    ClientSize = new System.Drawing.Size(m_MinWidth, 22),
                    ControlBox = false,
                    BackColor = Color.White
                };
                m_Popup.SuspendLayout();


                //Add the search box
                m_SearchTextBox = new TextBox() {
                    BorderStyle = BorderStyle.FixedSingle,
                    Dock = DockStyle.Top,
                    Font = new Font("Microsoft Sans Serif", 9.75f, System.Drawing.FontStyle.Regular, GraphicsUnit.Point, 0),
                    Location = new System.Drawing.Point(0, 0),
                    Size = new System.Drawing.Size(m_MinWidth, 22),
                    TabIndex = 0,
                    TextAlign = System.Windows.Forms.HorizontalAlignment.Center,
                    WordWrap = false,
                    Text = ""
                };
                m_SearchTextBox.TextChanged += TextBox_TextChanged;
                m_SearchTextBox.LostFocus += M_SearchTextBox_LostFocus;
                m_Popup.Controls.Add(m_SearchTextBox);


                //Add the result panel
                m_SearchResultPanel = new Panel()
                {
                    Location = new System.Drawing.Point(0, 25),
                    Size = new System.Drawing.Size(m_MinWidth, 1),
                    TabIndex = 1,
                };
                m_Popup.Controls.Add(m_SearchResultPanel);  
            }

            // Finish the popup form
            m_SearchTextBox.Text = "";
            m_SearchTextBox.Width = m_MinWidth;
            m_SearchResultPanel.Controls.Clear();
            m_SearchResultPanel.Size = new System.Drawing.Size(m_MinWidth, 1);

            System.Drawing.Point position = System.Windows.Forms.Cursor.Position;
            int h = m_SearchTextBox.Height;
            int x = position.X - m_Popup.Width / 2;
            int y = position.Y - m_SearchTextBox.Height / 2;
            m_Popup.SetBounds(x, y, m_Popup.Width, h);

            m_Popup.ResumeLayout(false);
            m_Popup.PerformLayout();
            m_Popup.Show(container);
            m_SearchTextBox.Focus();

            TextBox_TextChanged(null, null);

            return true;
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private void M_SearchTextBox_LostFocus(object sender, EventArgs e)
        {
            m_Popup.Hide();
        }

        /*************************************/

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            List<SearchItem> hits = PossibleItems.Hits(m_SearchTextBox.Text);

            int yPos = 0;
            int maxWidth = m_MinWidth;
            m_SearchResultPanel.Controls.Clear();
            for (int i = 0; i < hits.Count; i++)
            {
                SearchItem hit = hits[i];
                Label label = new Label
                {
                    Text = hit.Text,
                    BackColor = Color.White,
                    Cursor = Cursors.Hand,
                    TextAlign = ContentAlignment.MiddleLeft,
                };
                label.Width = (int)Math.Ceiling(label.CreateGraphics().MeasureString(label.Text, label.Font).Width);
                label.MouseUp += (a, b) =>
                {
                    m_Popup.Hide();
                    NotifySelection(hit);
                };

                PictureBox icon = new PictureBox { Image = hit.Icon, Height = label.Height, Width = label.Height };
                label.Location = new System.Drawing.Point(icon.Width + 5, 0);

                Panel row = new Panel { Width = label.Width + icon.Width, Height = label.Height, Location = new System.Drawing.Point(0, yPos) };
                row.Controls.Add(icon);
                row.Controls.Add(label);
                maxWidth = Math.Max(maxWidth, row.Width);

                m_SearchResultPanel.Controls.Add(row);
                yPos += row.Height;
            }

            if (hits.Count == 0)
            {
                Label label = new Label { Text = "No result found...", BackColor = Color.White, Location = new System.Drawing.Point(0, yPos), Width = m_MinWidth, TextAlign = ContentAlignment.TopCenter };
                m_SearchResultPanel.Controls.Add(label);
                yPos += label.Height;
            }
            
            m_Popup.Width = maxWidth;
            m_SearchResultPanel.Size = new System.Drawing.Size(maxWidth, yPos);
            m_Popup.Height = m_SearchResultPanel.Bottom + 10;
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private static int m_MinWidth = 200;
        private static Form m_Popup = null;
        private static TextBox m_SearchTextBox = null;
        private static Panel m_SearchResultPanel = null;

        /*************************************/
    }
}
