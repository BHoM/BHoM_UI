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

using BH.oM.Base;
using BH.oM.Base.Attributes;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Compute
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description("Loads all BHoM assemblies from the current domain.")]
        [Input("lastAssemblyUpdateTimes", "records of the last time each assembly was updated.")]
        [Output("loadedAssemblies", "Assemblies loaded as considered new")]
        public static List<string> LoadNewAssemblies(Dictionary<string, DateTime> lastAssemblyUpdateTimes)
        {
            if (lastAssemblyUpdateTimes == null)
            {
                BH.Engine.Base.Compute.RecordError("lastAssemblyUpdateTimes was not provided. No assembly was loaded.");
                return new List<string>();
            }

            // Make sure the keys for the assemblies are in lower case to avoid casing mismatching
            Dictionary<string, DateTime> lastUpdateTimes = lastAssemblyUpdateTimes.ToDictionary(x => x.Key.ToLower(), x => x.Value);

            List<string> loadedAssemblies = new List<string>();

            Regex regex = new Regex(@"oM$|_Engine$|_Adapter$");
            foreach (string file in Directory.GetFiles(BH.Engine.Base.Query.BHoMFolder(), "*.dll", SearchOption.TopDirectoryOnly))
            {
                string name = Path.GetFileNameWithoutExtension(file);
                if (regex.IsMatch(name))
                {
                    string key = name.ToLower();
                    if (!lastUpdateTimes.ContainsKey(key) || lastUpdateTimes[key] < File.GetLastWriteTimeUtc(file))
                    {
                        Assembly assembly = BH.Engine.Base.Compute.LoadAssembly(file);
                        if (assembly != null)
                        {
                            BH.Engine.Base.Compute.RecordNote($"Assembly {name} loaded as it was newer than its last recorded update time.");
                            loadedAssemblies.Add(name);
                        }   
                    }  
                }
            }

            return loadedAssemblies;
        }

        /*************************************/

    }
}






