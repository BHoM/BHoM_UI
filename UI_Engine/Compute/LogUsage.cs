/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

using BH.Engine.Serialiser;
using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.Base.Debugging;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;

namespace BH.Engine.UI
{
    public static partial class Compute
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static void LogUsage(string uiName, string uiVersion, Guid componentId, string callerName, object selectedItem, List<Event> events = null, string fileId = "", string fileName = "")
        {
            TriggerLogUsageArgs args = new TriggerLogUsageArgs()
            {
                UIName = uiName,
                UIVersion = uiVersion,
                ComponentID = componentId,
                CallerName = callerName,
                SelectedItem = selectedItem,
                FileID = fileId,
            };

            if (m_documentOpening)
                TriggerUIOpening(args);
            else
                TriggerUsageLog(args);

            // If a projectID event is available, save the project code for this file
            var allEvents = BH.Engine.Base.Query.AllEvents();            
            if (allEvents != null)
            {
                ProjectIDEvent e = allEvents.OfType<ProjectIDEvent>().Where(x => x.FileID == fileId).FirstOrDefault();
                if (e != null && !string.IsNullOrEmpty(fileId))
                    m_ProjectIDPerFile[fileId] = e.ProjectID;
            }

            try
            {
                // Create the log item
                UsageLogEntry info = new UsageLogEntry
                {
                    UI = uiName,
                    UiVersion = uiVersion,
                    BHoMVersion = BHoMVersion(),
                    ComponentId = componentId,
                    CallerName = callerName,
                    SelectedItem = selectedItem,
                    FileId = fileId,
                    FileName = fileName,
                    Errors = events == null ? new List<Event>() : events.Where(x => x.Type == EventType.Error).ToList()
                };

                // Record the project code if it exists
                if (m_ProjectIDPerFile.ContainsKey(fileId))
                    info.ProjectID = m_ProjectIDPerFile[fileId];

                // Write to the log file
                string json = info.ToJson();
                StreamWriter log = GetUsageLog(uiName);
                log.WriteLine(json);
                log.Flush();
            }
            catch { }
        }


        /*************************************/
        /**** Helper Methods              ****/
        /*************************************/

        private static StreamWriter GetUsageLog(string uiName)
        {
            if (m_UsageLog == null)
            {
                string logFolder = Query.UsageLogFolder();

                // Get rid of log files old enough to be deleted
                RemoveDeprecatedLogs(logFolder);

                // Create the new log file
                string filePath = Query.UsageLogFileName(uiName);
                FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                m_UsageLog = new StreamWriter(stream);

                // Be ready to close the file when the UI is closed
                AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            }

            return m_UsageLog;
        }

        /*************************************/

        private static void RemoveDeprecatedLogs(string logFolder)
        {
            long currentTicks = DateTime.UtcNow.Ticks;
            List<string> logFiles = Directory.GetFiles(logFolder).Where(x => x.Contains("Usage_")).ToList(); 

            foreach( string file in logFiles)
            {
                string[] parts = file.Split(new char[] { '_', '.' });
                if (parts.Length >= 4)
                {
                    long ticks = 0;
                    if (long.TryParse(parts.Reverse().ToList()[1], out ticks))
                    {
                        if (currentTicks - ticks >= m_DeprecationPeriod)
                            File.Delete(file);
                    }
                }
            }
        }

        /*************************************/

        private static void OnProcessExit(object sender, EventArgs e)
        {
            // The file seems to be writable after the UI closed even without this but better safe than sorry.
            if (m_UsageLog != null)
                m_UsageLog.Close();

            TriggerUIClose();
        }

        /*************************************/

        public static string BHoMVersion()
        {
            if (m_BHoMVersion == null)
                m_BHoMVersion = Engine.Base.Query.BHoMVersion();

            return m_BHoMVersion;
        }

        /*************************************/

        private static void TriggerUsageLog(TriggerLogUsageArgs e)
        {
            if (m_UsageLogTriggered != null && !Compute.m_documentOpening)
                m_UsageLogTriggered.Invoke(null, e); //Force the data to be set if the set project ID component is being run during the script load
        }

        /*************************************/

        private static void TriggerUIClose()
        {
            if (m_UIClosed != null)
                m_UIClosed.Invoke(null, null);
        }

        /*************************************/

        private static void TriggerUIOpening(TriggerLogUsageArgs e)
        {
            if (m_UIOpening != null)
                m_UIOpening.Invoke(null, e);
        }

        /*************************************/

        private static void TriggerUIEndOpening()
        {
            if(m_UIEndOpening != null)
                m_UIEndOpening.Invoke(null, null);
        }

        /*************************************/
        /**** Static Fields               ****/
        /*************************************/

        private static StreamWriter m_UsageLog = null;

        private static string m_BHoMVersion = null;

        private static long m_DeprecationPeriod = 28 * TimeSpan.TicksPerDay; // 28 days in ticks

        private static Dictionary<string, string> m_ProjectIDPerFile = new Dictionary<string, string>();

        public static event EventHandler m_UsageLogTriggered;
        public static event EventHandler m_UIClosed;
        public static event EventHandler m_UIOpening;
        public static event EventHandler m_UIEndOpening;

        /*************************************/
    }
}




