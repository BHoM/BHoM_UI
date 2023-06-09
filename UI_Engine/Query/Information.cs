/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Text;

using BH.oM.UI;

using Microsoft.Win32;
using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        [Description("Obtain the installed version information of BHoM.")]
        [Output("info", "The information about BHoM currently installed on this machine.")]
        public static BHoMInformation Information()
        {
            var keyLocation = "SOFTWARE\\WOW6432NODE\\Microsoft\\Windows\\CurrentVersion\\Uninstall";
            var subKeys = Registry.LocalMachine.OpenSubKey(keyLocation);

            var subKeyNames = subKeys.GetSubKeyNames();

            RegistryKey bhomKey = null;
            foreach (var s in subKeyNames)
            {
                try
                {
                    var keyName = $"{keyLocation}\\{s}";
                    subKeys = Registry.LocalMachine.OpenSubKey(keyName);

                    var a = subKeys.GetValue("DisplayName");
                    if (a != null && a.ToString() == "Buildings and Habitats object Model")
                    {
                        bhomKey = subKeys;
                        break;
                    }
                }
                catch (Exception e)
                {

                }
            }

            string version = "";
            string installDate = "";
            try
            {
                version = bhomKey.GetValue("DisplayVersion").ToString();
            }
            catch { }

            try
            {
                installDate = bhomKey.GetValue("InstallDate").ToString();
            }
            catch { }

            if (string.IsNullOrEmpty(version))
                version = BH.Engine.Base.Query.BHoMVersion();

            if (!string.IsNullOrEmpty(installDate))
            {
                DateTime installDateTime;
                var installDateTimeFine = DateTime.TryParseExact(installDate, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out installDateTime);
                installDate = installDateTime.ToString("dd/MM/yyyy");
            }
            else
                installDate = "This appears to be a version of BHoM installed using an old installer.";

            return new BHoMInformation()
            {
                Version = version,
                InstallDate = installDate,
                BHoMWebsite = Base.Query.BHoMWebsiteURL(),
                WikiLink = Base.Query.DocumentationURL(),
                Assemblies = BH.Engine.Base.Query.BHoMAssemblyList().Select(x => x.GetName().Name).OrderBy(x => x).ToList(),
                Description = "This is the Buildings and Habitats object Model. A collaborative computational development project for the built environment.\n\n"
                + "It is crafted as transdisciplinary, software-agnostic and office/region/country independent, and therefore would be nothing without our active community and wide range of contributors.\n\n"
                + $"To find out more about our collective experiment go to {Base.Query.BHoMWebsiteURL()}",
            };
        }
    }
}
