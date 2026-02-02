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
using BH.Engine.Base.Objects;
using BH.Engine.Reflection;
using BH.Engine.UI;
using BH.oM.Data.Requests;
using BH.oM.UI;
using BH.UI.Base.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BH.UI.Base.Global
{
    public class AssemblyResolver : IAssemblyResolver
    {
        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public AssemblyResolver(Dictionary<string, List<string>> assemblyNamePerType = null)
        {
            if (assemblyNamePerType != null) 
                m_AssemblyNamePerType = assemblyNamePerType;
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public bool MakeSureAssemblyIsLoadedForType(string type)
        {
            if (string.IsNullOrEmpty(type) || !type.StartsWith("BH."))
                return false;

            string[] parts = type.Split(',');
            List<string> assemblyNames = new List<string>();

            if (parts.Length > 1)
            {
                // Assembly is alreaady in the type
                assemblyNames.Add(parts[1].Trim());
            }
            else if (parts.Length == 1)
            {
                // We don't have the assembly registered in the type so we need to deduce it 
                if (m_AssemblyNamePerType.ContainsKey(type))
                    assemblyNames.AddRange(m_AssemblyNamePerType[type]);
            }

            foreach (string assemblyName in assemblyNames.Where(x => !string.IsNullOrEmpty(x)))
            {
                if (!BH.Engine.Base.Query.IsAssemblyLoaded(assemblyName))
                {
                    Assembly assembly = BH.Engine.Base.Compute.LoadAssembly(Path.Combine(BH.Engine.Base.Query.BHoMFolder(), assemblyName + ".dll"));
                    return assembly != null;
                }
            }

            return false;
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/



        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        Dictionary<string, List<string>> m_AssemblyNamePerType = new Dictionary<string, List<string>>();

        /*************************************/
    }
}






