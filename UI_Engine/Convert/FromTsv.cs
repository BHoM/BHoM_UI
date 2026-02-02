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

using BH.Engine.Reflection;
using BH.oM.Base.Attributes;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Convert
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description("Convert a row in an Excel file (in tsv format) into a code element record.")]
        [Input("tsv", "Excel row that contains the data related to the code element in a tsv format.")]
        [Output("codeElement", "Converted code element.")]
        public static CodeElementRecord FromTsv(this string tsv)
        {
            string[] parts = tsv.Split('\t');
            if (parts.Length < 5)
                return null;

            if (!Enum.TryParse(parts[1], out CodeElementType type))
                return null;

            if (!long.TryParse(parts[4], out long utcTime))
                return null;

            return new CodeElementRecord
            {
                AssemblyName = parts[0],
                Type = type,
                DisplayText = parts[2],
                Json = parts[3],
                AssemblyModifiedTime = DateTime.FromFileTimeUtc(utcTime)
            };

        }

        /*************************************/
    }
}






