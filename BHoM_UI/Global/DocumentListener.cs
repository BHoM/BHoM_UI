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

namespace BH.UI.Base.Global
{
    public static class DocumentListener
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static void OnDocumentBeginOpening(string documentName)
        {
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
                Thread newThread = new Thread(ShowForm);
                newThread.Start(events);
            }
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private static void ShowForm(object input)
        {
            List<VersioningEvent> events = input as List<VersioningEvent>;
            if (events == null)
                return;

            string message = $"The file was upgraded from version {events.First().OldVersion} to version {events.First().NewVersion}.";
            message += "\nPlease review the file before saving it.";
            message += "\n\nHere's the list of components that have been modified:";

            Form form = new Form
            {
                Text = "Versioning report",
                AutoSize = true,
                BackColor = System.Drawing.Color.White,
                MaximumSize = new System.Drawing.Size(1200, 900),
                AutoScroll = true
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

        private static DataGridView GetTable(VersioningEvent e)
        {
            DataTable table = new DataTable();

            table.Columns.Add(new DataColumn { ColumnName = " ", DataType = typeof(string) });
            table.Columns.Add(new DataColumn { ColumnName = "Version", DataType = typeof(string) });
            table.Columns.Add(new DataColumn { ColumnName = "Item", DataType = typeof(string) });

            DataRow oldRow = table.NewRow();
            oldRow[" "] = "Old";
            oldRow["Version"] = e.OldVersion;
            oldRow["Item"] = e.OldDocument;
            table.Rows.Add(oldRow);

            DataRow newRow = table.NewRow();
            newRow[" "] = "New";
            newRow["Version"] = e.NewVersion;
            newRow["Item"] = e.NewDocument ?? e.Message;
            table.Rows.Add(newRow);

            DataGridView view = new DataGridView
            {
                DataSource = table,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                AutoSize = true,
                RowHeadersVisible = false,
                BackgroundColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.Black,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                BorderStyle = BorderStyle.None,
                ReadOnly = true
            };
            view.RowsDefaultCellStyle.SelectionBackColor = System.Drawing.Color.White;
            view.RowsDefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;

            if (e.NewDocument == null)
                view.DefaultCellStyle.WrapMode = DataGridViewTriState.True;

            return view;
        }

        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private static Dictionary<string, long> m_OpeningTimes = new Dictionary<string, long>();


        /*************************************/
    }

}


