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

using BH.Engine.Serialiser;
using BH.oM.Base;
using BH.oM.Reflection.Debugging;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Compute
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static void LogUsage(string uiName, Guid componentId, object selectedItem, List<Event> events = null)
        {
            try
            {
                // Create the log item
                UsageLogEntry info = new UsageLogEntry
                {
                    UI = uiName,
                    BHoMVersion = BHoMVersion(),
                    ComponentId = componentId,
                    SelectedItem = selectedItem,
                    Errors = events == null ? new List<Event>() : events.Where(x => x.Type == EventType.Error).ToList()
                };

                string json = info.ToJson();

                // Write to the log file
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
                // Make sure teh folder exists
                string logFolder = @"C:\ProgramData\BHoM\Logs";
                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);

                // Get rid of log files old enough to be deleted
                RemoveDeprecatedLogs(logFolder);

                // Create the new log file
                string filePath = Path.Combine(logFolder, "Usage_" + uiName + "_" + DateTime.UtcNow.Ticks + ".log");
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
        }

        /*************************************/

        public static string BHoMVersion()
        {
            if (m_BHoMVersion == null)
                m_BHoMVersion = Engine.Reflection.Query.BHoMVersion();

            return m_BHoMVersion;
        }


        /*************************************/
        /**** Static Fields               ****/
        /*************************************/

        private static StreamWriter m_UsageLog = null;

        private static string m_BHoMVersion = null;

        private static long m_DeprecationPeriod = 7 * TimeSpan.TicksPerDay; // 7 days in ticks

        /*************************************/
    }
}

