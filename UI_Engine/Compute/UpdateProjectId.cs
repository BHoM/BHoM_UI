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

        [Description("Updates the project ID for a specific file.")]
        [Input("uiName", "The name of the UI.")]
        [Input("fileId", "The file guid.")]
        [Input("projectId", "The project identifier to set.")]
        public static void UpdateProjectId(string uiName, string fileId, string projectId)
        {
            if (string.IsNullOrWhiteSpace(uiName) || string.IsNullOrWhiteSpace(fileId))
                return;

            bool wasUpdated = true;
            lock (m_LogLock)
            {
                string prevPojectId;
                if (m_ProjectIDPerFile.TryGetValue(fileId, out prevPojectId) && projectId == prevPojectId)  //Check if the ID changed from previous cached value
                    wasUpdated = false;

                m_ProjectIDPerFile[fileId] = projectId; //Set the project Id to the cached dictionary
            }

            if(wasUpdated)  //If changed from previous value, make sure the logfiles are updated
                Task.Run(() => UpdateProjectIdsInLogFile(uiName, fileId, projectId));
        }

        

        /*************************************/
        /**** Helper Methods              ****/
        /*************************************/

        private static void UpdateProjectIdsInLogFile(string uiName, string fileId, string projectID)
        {
            lock (m_LogLock)
            {
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
                            if (o.FileId == fileId)
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
    }
}






