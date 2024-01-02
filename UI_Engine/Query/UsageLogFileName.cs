/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        [Description("Obtain the file name of the current instance of a usage log file for the provided UI.")]
        [Input("uiName", "The UI currently using the usage log to obtain the log file name for.")]
        [Output("usageLogFileName", "The full file path to the current usage log file for the given UI.")]
        public static string UsageLogFileName(string uiName)
        {
            if (!string.IsNullOrEmpty(m_usageLogFileName))
                return m_usageLogFileName;

            string logFolder = UsageLogFolder();

            m_usageLogFileName = Path.Combine(logFolder, "Usage_" + uiName + "_" + DateTime.UtcNow.Ticks + ".log");

            return m_usageLogFileName;
        }

        private static string m_usageLogFileName;
    }
}


