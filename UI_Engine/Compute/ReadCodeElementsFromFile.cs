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

using BH.oM.Base.Attributes;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace BH.Engine.UI
{
    public static partial class Compute
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description("Reads the code elements stored in a tsv file, for example an assembly content file. The file can be written by other BHoM UIs at the same time, so it is opened in a sharing mode.")]
        [Input("filePath", "Path of the tsv file. The file may not exist.")]
        [Output("codeElements", "The code elements of the file. Empty if the file does not exist or cannot be read, in which case an error is recorded.")]
        public static List<CodeElementRecord> ReadCodeElementsFromFile(string filePath)
        {
            List<CodeElementRecord> records = new List<CodeElementRecord>();
            if (!File.Exists(filePath))
                return records;

            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        CodeElementRecord record = Convert.CodeElementFromTsv(line);
                        if (record != null)
                            records.Add(record);
                    }
                }
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError(e, $"Failed to read the code elements from '{Path.GetFileName(filePath)}'.");
            }

            return records;
        }

        /*************************************/
    }
}
