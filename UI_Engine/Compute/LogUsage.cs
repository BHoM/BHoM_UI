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

        [PreviousVersion("8.1", "BH.Engine.UI.Compute.LogUsage(System.String, System.String, System.Guid, System.String, System.Object, System.Collections.Generic.List<BH.oM.Base.Debugging.Event>, System.String, System.String)")]
        public static void LogUsage(string uiName, string uiVersion, Guid componentId, string callerName, object selectedItem, List<Event> events = null, string fileId = "", string fileName = "", string projectId = "")
        {
            //Special case for a component setting the project ID explicitly
            HandleSetProjectId(uiName, fileId, events);

            if (string.IsNullOrWhiteSpace(projectId))
            {
                lock (m_LogLock)
                {
                    if (!m_ProjectIDPerFile.TryGetValue(fileId, out projectId))
                        projectId = "";
                }

                if (string.IsNullOrWhiteSpace(projectId))
                {
                    TriggerLogUsageArgs args = new TriggerLogUsageArgs()
                    {
                        UIName = uiName,
                        UIVersion = uiVersion,
                        ComponentID = componentId,
                        CallerName = callerName,
                        SelectedItem = selectedItem,
                        FileID = fileId,
                        FileName = fileName,
                    };

                    if (m_documentOpening)
                        TriggerUIOpening(args);
                    else
                        TriggerUsageLog(args);
                }
            }

            LogToFile(uiName, uiVersion, componentId, callerName, selectedItem, events, fileId, fileName, projectId);
        }


        /*************************************/
        /**** Helper Methods              ****/
        /*************************************/

        private static void LogToFile(string uiName, string uiVersion, Guid componentId, string callerName, object selectedItem, List<Event> events, string fileId, string fileName, string projectId)
        {
            try
            {
                Task.Run(() =>
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
                        ProjectID = projectId,
                        Errors = events == null ? new List<Event>() : events.Where(x => x.Type == EventType.Error).ToList()
                    };

                    string json = info.ToJson();

                    lock (m_LogLock)
                    {
                        // Write to the log file
                        FileStream log = GetUsageLog(uiName);
                        using (StreamWriter writer = new StreamWriter(log, Encoding.UTF8, 4096, true))
                        {
                            writer.WriteLine(json);
                            writer.Flush();
                        }
                    }
                });

            }
            catch { }
        }

        /*************************************/

        private static void HandleSetProjectId(string uiName, string fileId, List<Event> events)
        {
            if (!string.IsNullOrEmpty(fileId))
            {
                ProjectIDEvent projectIDEvent = events?.OfType<ProjectIDEvent>().FirstOrDefault();
                if (projectIDEvent != null)
                {
                    if (string.IsNullOrEmpty(projectIDEvent.UIName) || string.IsNullOrEmpty(projectIDEvent.FileID))
                    {
                        if (!string.IsNullOrEmpty(projectIDEvent.ProjectID))
                        {
                            Base.Query.AllEvents().Remove(projectIDEvent);
                            projectIDEvent.UIName = uiName;
                            projectIDEvent.FileID = fileId;
                            Base.Compute.RecordEvent(projectIDEvent);
                        }
                    }
                }
            }
        }

        /*************************************/

        private static FileStream GetUsageLog(string uiName)
        {
            if (m_UsageLogStream == null)
            {
                string logFolder = Query.UsageLogFolder();

                // Get rid of log files old enough to be deleted
                RemoveDeprecatedLogs(logFolder);

                // Create the new log file
                string filePath = Query.UsageLogFileName(uiName);
                m_UsageLogStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);

                // Be ready to close the file when the UI is closed              
                AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            }

            return m_UsageLogStream;
        }

        /*************************************/

        private static void RemoveDeprecatedLogs(string logFolder)
        {
            long currentTicks = DateTime.UtcNow.Ticks;
            List<string> logFiles = Directory.GetFiles(logFolder).Where(x => x.Contains("Usage_")).ToList();

            foreach (string file in logFiles)
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

        static Compute()
        {
            Base.Compute.EventRecorded += EventRecorded;
        }

        /*************************************/

        private static void EventRecorded(object sender, Event e)
        {
            if (e != null && e is ProjectIDEvent projIdEvent)
            {
                if (!string.IsNullOrEmpty(projIdEvent.UIName) && !string.IsNullOrEmpty(projIdEvent.ProjectID) && !string.IsNullOrEmpty(projIdEvent.FileID))
                    Task.Run(() => UpdateProjectId(projIdEvent.UIName, projIdEvent.FileID, projIdEvent.ProjectID));
            }
        }

        /*************************************/

        private static void UpdateProjectId(string uiName, string fileID, string projectID)
        {
            lock (m_LogLock)
            {
                m_ProjectIDPerFile[fileID] = projectID;

                try
                {
                    FileStream log = GetUsageLog(uiName);
                    log.Position = 0;   //Set stream to start to read from top of file
                    List<string> logLines = new List<string>();
                    //Read all content
                    using (StreamReader reader = new StreamReader(log, Encoding.UTF8, true, 4096, true))
                    {
                        while (!reader.EndOfStream)
                        {
                            logLines.Add(reader.ReadLine());
                        }
                    }

                    //Update project ID for any items exiting before event triggered
                    var objects = logLines.Select(x => BH.Engine.Serialiser.Convert.FromJson(x) as UsageLogEntry).ToList();
                    foreach (var o in objects)
                    {
                        if (o != null)
                        {
                            if (o.FileId == fileID)
                                o.ProjectID = projectID;
                        }
                    }

                    //Write lines back to the file
                    logLines = objects.Select(x => x.ToJson()).ToList();
                    log.Position = 0;
                    using (StreamWriter writer = new StreamWriter(log, Encoding.UTF8, 4096, true))
                    {
                        foreach (var line in logLines)
                        {
                            writer.WriteLine(line);
                        }
                        writer.Flush();
                    }
                }
                catch (Exception)
                {

                }



            }
        }

        /*************************************/

        private static void OnProcessExit(object sender, EventArgs e)
        {
            // The file seems to be writable after the UI closed even without this but better safe than sorry.
            if (m_UsageLogStream != null)
                m_UsageLogStream.Close();

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
            if (m_UIEndOpening != null)
                m_UIEndOpening.Invoke(null, null);
        }

        /*************************************/
        /**** Static Fields               ****/
        /*************************************/

        private static object m_LogLock = new object();
        private static FileStream m_UsageLogStream = null;

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






