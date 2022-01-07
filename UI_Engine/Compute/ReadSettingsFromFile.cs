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

using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using System.ComponentModel;
using System.IO;

namespace BH.Engine.UI
{
    public static partial class Compute
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description("Reads BHoM toolkit settings from a file under given path.")]
        [Input("filePath", "Path to the file to extract the settings from.")]
        [Output("settings", "BHoM toolkit settings extracted from the input file path.")]
        public static ISettings ReadSettingsFromFile(this string filePath)
        {
            // Make sure the file exists
            if (!File.Exists(filePath))
            {
                Engine.Reflection.Compute.RecordWarning($"Settings file does not exits under path {filePath}.");
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
                Reflection.Compute.RecordError($"Settings could not be read from the file under path {filePath}. Please make sure it isn't locked by another program.");
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
