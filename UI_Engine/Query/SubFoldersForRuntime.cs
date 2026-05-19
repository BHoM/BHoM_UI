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

using BH.Engine.Base;
using BH.oM.Base.Attributes;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description("Returns the runtime-specific subdirectories of the BHoM Assemblies folder where assemblies compatible with the current .NET runtime can be found. " +
             "Returns '.../Assemblies/netfx/' on .NET Framework and '.../Assemblies/netX.0/' on CoreCLR (.NET X).")]
        [Output("subFolders", "runtime-specific subdirectories for the BHoM assemblies sorted in the order they should be traversed.")]
        public static List<string> SubFoldersForRuntime()
        {
            if (m_SubFoldersForRuntime != null) 
                return m_SubFoldersForRuntime;

            
            var desc = RuntimeInformation.FrameworkDescription;
            if (desc.StartsWith(".NET Framework", StringComparison.OrdinalIgnoreCase))
            {
                // Return 'netfx' if the framework is a .NET Framework
                m_SubFoldersForRuntime = new List<string> { "netfx" };
            }
            else
            {
                // For .NET Core, return exact TFM first, then descend to lower versions as fallback
                m_SubFoldersForRuntime = new List<string>();
                int major = Environment.Version.Major;
                for (int v = major; v >= 5; v--)
                    m_SubFoldersForRuntime.Add($"net{v}.0");
            }

            return m_SubFoldersForRuntime;
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private static List<string> m_SubFoldersForRuntime = null;

        /*************************************/
    }
}







