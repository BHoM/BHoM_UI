/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

        [Description("Checks and logs when UI finishes opening a document.")]
        [Input("uiName", "The name of the UI.")]
        [Input("fileId", "The file guid.")]
        [Input("fileName", "The file full name (including path).")]
        public static void CheckLogOnUiEndOpening(string uiName, string fileId, string fileName)
        {
            if (string.IsNullOrWhiteSpace(uiName) || string.IsNullOrWhiteSpace(fileId))
                return;

            if (fileName != null)   //Only call when opening a pre-existing file, not for new files with no filename set
            {
                if (m_FilesTriedToBeLogged.Contains(fileId))  //Only call when file has been atempted to be logged to
                {
                    string projectId;
                    m_ProjectIDPerFile.TryGetValue(fileId, out projectId);

                    if (string.IsNullOrEmpty(projectId))    //Only call when projectId is not set
                    {
                        TriggerLogUsageArgs args = new TriggerLogUsageArgs()
                        {
                            UIName = uiName,
                            FileID = fileId,
                            FileName = fileName
                        };

                        TriggerUsageLog(args);
                        //Check if the projectID has been set to the args
                        if (!string.IsNullOrWhiteSpace(args.ProjectID))
                        {
                            projectId = args.ProjectID; //Set the project ID
                            UpdateProjectId(uiName, fileId, projectId); //Ensure the project ID is udpated
                        }
                    }

                }
            }
        }

        /*************************************/
    }
}







