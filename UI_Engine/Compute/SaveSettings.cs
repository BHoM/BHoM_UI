/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BH.Engine.UI
{
    public static partial class Compute
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description(@"Saves the settings for a toolkit into C:/ProgramData/BHoM/Settings. If Any previoulsy saved settings for that toolkit will be overwritten")]
        [Input("settings", "Settings for a toolkit that need to be saved permanently.")]
        [Output("success", "Returns true if the settings were saved successfully.")]
        public static bool SaveSettings(ISettings settings)
        {
            if (settings == null)
            {
                Engine.Base.Compute.RecordError("Settings object is null.");
                return false;
            }
                
            // Get the config file name
            string[] splittedNamespace = settings.GetType().Namespace.Split(new char[] { '.' });
            if (splittedNamespace.Length != 3)
            {
                Engine.Base.Compute.RecordError("This settings object doesn't have a valid namespace. It should be `BH.oM.ToolkitName` .");
                return false;
            }

            string toolkitName = splittedNamespace[2];
            string filePath = Path.Combine(@"C:\ProgramData\BHoM\Settings", toolkitName + ".cfg");

            // Save the setting in that file
            File.WriteAllText(filePath, settings.ToJson());

            return true;
        }

        /*************************************/
    }
}



