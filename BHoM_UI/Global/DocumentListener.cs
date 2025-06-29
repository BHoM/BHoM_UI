/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

            BH.Engine.UI.Compute.SetDocumentOpeningState(true);
        }

        /*************************************/

        public static void OnDocumentEndOpening(string documentName, string uiName, string fileId)
        {
            BH.Engine.UI.Compute.SetDocumentOpeningState(false);

            BH.Engine.UI.Compute.CheckLogOnUiEndOpening(uiName, fileId, documentName);

            if (string.IsNullOrEmpty(documentName) || !m_OpeningTimes.ContainsKey(documentName))
                return;

            long openingStartTicks = m_OpeningTimes[documentName];
            List<VersioningEvent> events = BH.Engine.Versioning.Query.VersioningEvents().Where(x => x.Time.Ticks >= openingStartTicks).ToList();
            BH.Engine.Base.Compute.ClearCurrentEvents();

            if (events.Count > 0)
            {
                if (m_VersioningForm != null)
                {
                    m_VersioningForm.Close();
                    m_VersioningForm.Dispose();
                    m_VersioningForm = null;
                }

                ShowForm(events);
            }
        }

        /*************************************/

        public static void OnDocumentClosing(string documentName)
        {
            if (m_VersioningForm != null)
            {
                m_VersioningForm.Close();
                m_VersioningForm.Dispose();
                m_VersioningForm = null;
            }

            if (!string.IsNullOrEmpty(documentName) && m_OpeningTimes.ContainsKey(documentName))
                m_OpeningTimes.Remove(documentName);

            BH.Engine.UI.Compute.SetDocumentOpeningState(false);
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private static void ShowForm(object input)
        {
            List<VersioningEvent> events = input as List<VersioningEvent>;
            if (events == null)
                return;

            m_VersioningForm = new Form
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
            m_VersioningForm.Controls.Add(layout);

            string message = $"The file was upgraded from BHoM version {events.First().OldVersion} to version {events.First().NewVersion}.";
            message += "\nPlease review the file before saving it.";


            int maxMessages = 30;   //Limiting to 30 messages as the forms takes to long to draw for to many encoutners
            var groups = events.GroupBy(e => new { e.OldVersion, e.NewVersion, e.OldDocument, NewDoc = e.NewDocument ?? e.Message });

            if (groups.Count() > maxMessages)
            {
                string logFileName = $@"C:\Temp\BHoMUpgradeLog_{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt";
                message += $"\n\nThe document contains a significant amount of versioning. The window log has been shortened showing the {maxMessages} most commonly upgraded components. For a full list see {logFileName}.";
                WriteLogFile(logFileName, groups);
            }
            else
                message += "\n\nHere's the list of components that have been modified:";

            layout.Controls.Add(new Label
            {
                Text = message,
                AutoSize = true,
                BackColor = System.Drawing.Color.White,
                Margin = new Padding(5, 20, 5, 5)
            });

            foreach (var group in groups.OrderByDescending(x => x.Count()).Take(maxMessages))   //OrderByDescending on count to always show the most commonly upgraded components, and Take maxMessages to limit to a number of tables that can be drawn within a reasonable timeframe
                layout.Controls.Add(GetTable(group.First(), group.Count()));

            m_VersioningForm.Show();
            m_VersioningForm.Focus();
            m_VersioningForm.BringToFront();

        }

        /*************************************/

        private static TableLayoutPanel GetTable(VersioningEvent e, int count)
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

            Label initialCell;
            if (count > 1)
            {
                initialCell = new Label
                {
                    Text = $"{count} Instances",
                    AutoSize = true,
                };
            }
            else
                initialCell = new Label();

            table.Controls.AddRange(new Control[] {
                initialCell,
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

        private static void WriteLogFile(string logFileName, IEnumerable<IGrouping<object, VersioningEvent>> groups)
        {
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(logFileName)))
                    Directory.CreateDirectory(Path.GetDirectoryName(logFileName));

                List<string> lines = new List<string>();


                foreach (var group in groups.OrderByDescending(x => x.Count()))
                {
                    VersioningEvent e = group.First();
                    int count = group.Count();

                    string newDoc = e.NewDocument;
                    if (newDoc != null && newDoc == e.OldDocument)
                        newDoc += " (the properties of this object have been updated)";

                    lines.Add($"From version: {e.OldVersion}");
                    lines.Add($"Old item: {e.OldDocument}");
                    lines.Add($"To version: {e.NewVersion}");
                    lines.Add($"New item: {newDoc ?? e.Message}");
                    lines.Add($"Instances upgraded: {count}");
                    lines.Add("");
                    lines.Add("/*************************************/");
                    lines.Add("");
                }

                Task.Factory.StartNew(() => File.WriteAllLines(logFileName, lines));    //Write asynchronously to avoid blocking the UI thread
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError(e, "Failed to write versioning log file");
            }
            
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private static Dictionary<string, long> m_OpeningTimes = new Dictionary<string, long>();
        private static Form m_VersioningForm = null;

        /*************************************/
    }

}






