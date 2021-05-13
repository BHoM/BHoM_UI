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
using BH.oM.Versioning;
using BH.UI.Base.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading;
using System.Text.RegularExpressions;

namespace BH.UI.Base.Global
{
    public static class DocumentListener
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static void OnDocumentBeginOpening(string documentName)
        {
            if (string.IsNullOrEmpty(documentName))
                return;

            m_OpeningTimes[documentName] = DateTime.Now.Ticks;
            Debug.WriteLine($"DocumentBeginOpening at {DateTime.Now.ToString("HH:mm:ss.ffffzzz")}");
        }

        /*************************************/

        public static void OnDocumentEndOpening(string documentName)
        {
            if (!m_OpeningTimes.ContainsKey(documentName))
                return;

            long openingStartTicks = m_OpeningTimes[documentName];
            List<VersioningEvent> events = BH.Engine.Versioning.Query.VersioningEvents().Where(x => x.Time.Ticks >= openingStartTicks).ToList();
            BH.Engine.Reflection.Compute.ClearCurrentEvents();

            if (events.Count > 0)
            {
                if (m_VersioningFormThead != null)
                    m_VersioningFormThead.Abort();

                m_VersioningFormThead = new Thread(ShowForm);
                m_VersioningFormThead.Start(events);
            }
        }

        /*************************************/

        public static void OnDocumentClosing(string documentName)
        {
            if (m_VersioningFormThead != null)
            {
                m_VersioningFormThead.Abort();
                m_VersioningFormThead = null;
            }

            if (m_OpeningTimes.ContainsKey(documentName))
                m_OpeningTimes.Remove(documentName);
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private static void ShowForm(object input)
        {
            List<VersioningEvent> events = input as List<VersioningEvent>;
            if (events == null)
                return;

            string message = $"The file was upgraded from BHoM version {events.First().OldVersion} to version {events.First().NewVersion}.";
            message += "\nPlease review the file before saving it.";
            message += "\n\nHere's the list of components that have been modified:";

            Form form = new Form
            {
                Text = "BHoM versioning report",
                AutoSize = true,
                BackColor = System.Drawing.Color.White,
                MaximumSize = new System.Drawing.Size(1200, 900),
                AutoScroll = true,
                Icon = Properties.Resources.BHoM_Icon
            };

            FlowLayoutPanel layout = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true
            };
            form.Controls.Add(layout);

            layout.Controls.Add(new Label
            {
                Text = message,
                AutoSize = true,
                BackColor = System.Drawing.Color.White,
                Margin = new Padding(5, 20, 5, 5)
            });

            foreach (VersioningEvent e in events)
                layout.Controls.Add(GetTable(e));

            form.ShowDialog();
        }


        /*************************************/

        private static TableLayoutPanel GetTable(VersioningEvent e)
        {
            TableLayoutPanel table = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 3,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Margin = new Padding(5, 20, 5, 20)
            };

            string newDoc = e.NewDocument;
            if (newDoc != null && newDoc == e.OldDocument)
                newDoc += " (the properties of this object have been updated)";

            table.Controls.AddRange(new Control[] {
                new Label(),
                GetCell("Version"),
                GetCell("Item"),
                GetCell("Old"),
                GetCell(e.OldVersion),
                GetCell(e.OldDocument),
                GetCell("New"),
                GetCell(e.NewVersion),
                GetCell(newDoc ?? e.Message)
            });

            if (e.NewDocument == null)
                table.Controls[8].MaximumSize = new System.Drawing.Size(table.Controls[5].Width, 0);

            return table;
        }

        /*************************************/

        private static Label GetCell(string text)
        {
            Match match = Regex.Match(text, @"http[s]?://\S+");
            if (match != null && match.Captures.Count > 0)
            {
                LinkLabel label = new LinkLabel
                {
                    Text = text,
                    AutoSize = true,
                    LinkArea = new LinkArea(match.Index, match.Length)
                };

                label.LinkClicked += (sender, e) => Process.Start(match.Captures[0].Value);

                return label;
            }
            else
            {
                return new Label
                {
                    Text = text,
                    AutoSize = true
                };
            }
            
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private static Dictionary<string, long> m_OpeningTimes = new Dictionary<string, long>();
        private static Thread m_VersioningFormThead = null;


        /*************************************/
    }

}


