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

using BH.Engine.Reflection;
using BH.oM.Reflection.Attributes;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description(@"Extract settings of a given type from C:/ProgramData/BHoM/Settings")]
        [Input("type", "Object type of the settings you want to recover")]
        [Output("settings", @"Settings recovered from the corresponding file in C:/ProgramData/BHoM/Settings")]
        public static ISettings Settings(Type type)
        {
            if (type == null)
            {
                Engine.Reflection.Compute.RecordError("Settings type is null.");
                return null;
            }

            // Get the config file name
            string[] splittedNamespace = type.Namespace.Split(new char[] { '.' });
            if (splittedNamespace.Length != 3)
            {
                Engine.Reflection.Compute.RecordError("This settings object doesn't have a valid namespace. It should be `BH.oM.ToolkitName` .");
                return null;
            }

            string toolkitName = splittedNamespace[2];
            return Settings(toolkitName);
        }

        /*************************************/

        [Description(@"Extract settings for a given toolkit from C:/ProgramData/BHoM/Settings")]
        [Input("toolkitName", "Toolkit you want to recover the settings for")]
        [Output("settings", @"Settings recovered from the corresponding file in C:/ProgramData/BHoM/Settings")]
        public static ISettings Settings(string toolkitName)
        {
            // Make sure the file exists
            string filePath = Path.Combine(@"C:\ProgramData\BHoM\Settings", toolkitName + ".cfg");
            if (!File.Exists(filePath))
            {
                Engine.Reflection.Compute.RecordWarning("There is no setting file for toolkit " + toolkitName + ".");
                return null;
            }

            // Get the json out of the file
            string json = "";
            try
            {
                json = File.ReadAllText(filePath);
            }
            catch
            {
                Reflection.Compute.RecordError("There setting file " + filePath + " cannot be read. Make sure it isn't locked by another program.");
                return null;
            }

            // Convert back into a Settings object
            object settings = Engine.Serialiser.Convert.FromJson(json);
            if (settings is ISettings)
                return settings as ISettings;
            else
            {
                Reflection.Compute.RecordError("The content of the file" + filePath + " doesn't contain a valid ISettings object.");
                return null;
            }
        }

        /*************************************/
    }
}


