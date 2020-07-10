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
                CustomObject info = Engine.Base.Create.CustomObject(new Dictionary<string, object>
                {
                    { "Time", DateTime.UtcNow },
                    { "UI", uiName },
                    { "ComponentId", componentId.ToString() },
                    { "Item", selectedItem },
                    { "Events", events == null ? new List<Event>() : events.Where(x => x.Type == EventType.Error) }
                });
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
                string logFolder = @"C:\ProgramData\BHoM\Logs";
                if (!Directory.Exists(logFolder))
                    Directory.CreateDirectory(logFolder);

                string filePath = Path.Combine(logFolder, "Usage_" + uiName + "_" + DateTime.UtcNow.Ticks + ".log");
                FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
                m_UsageLog = new StreamWriter(stream);

                AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            }

            return m_UsageLog;
        }

        /*************************************/

        private static void OnProcessExit(object sender, EventArgs e)
        {
            // The file seems to be writable after the UI closed even without this but better safe than sorry.
            if (m_UsageLog != null)
                m_UsageLog.Close();
        }


        /*************************************/
        /**** Static Fields               ****/
        /*************************************/

        private static StreamWriter m_UsageLog = null;

        /*************************************/
    }
}

