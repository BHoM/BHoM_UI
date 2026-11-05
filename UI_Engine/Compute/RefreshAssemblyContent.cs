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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BH.Engine.UI
{
    public static partial class Compute
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description("Loads the assemblies matching the filter that are new or newer than the ones recorded in the code elements, and updates the code elements and the file they are saved in with the content of those assemblies. The elements of an assembly that could not be fully extracted in the current process are saved as well: the assembly will only be extracted again when it changes.")]
        [Input("codeElements", "Code elements currently known. Elements of the assemblies that get reloaded are replaced.")]
        [Input("assemblyNameFilter", "Only assemblies with a name matching this regular expression are considered. If null or empty, the default filter for BHoM assemblies is used.")]
        [Input("filePath", "Path of the file the updated code elements are saved to, one tsv row per element. If null or empty, nothing is saved. The file is replaced atomically.")]
        [Output("codeElements", "Code elements updated with the content of the new assemblies. This is the input list itself if no assembly was loaded.")]
        public static List<CodeElementRecord> RefreshAssemblyContent(List<CodeElementRecord> codeElements, string assemblyNameFilter = null, string filePath = null)
        {
            codeElements = codeElements ?? new List<CodeElementRecord>();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Dictionary<string, DateTime> lastAssemblyUpdateTimes = codeElements
                .GroupBy(x => x.AssemblyName)
                .ToDictionary(x => x.Key, x => x.First().AssemblyModifiedTime);

            List<string> loadedAssemblies = LoadNewAssemblies(lastAssemblyUpdateTimes, assemblyNameFilter);

            stopwatch.Stop();
            BH.Engine.Base.Compute.RecordNote($"Time to load all updated/new assemblies from current domain: {stopwatch.Elapsed.TotalMilliseconds / 1000} s.");

            if (loadedAssemblies.Count == 0)
                return codeElements;

            stopwatch.Restart();

            // Only the assemblies that were just loaded are converted to code elements. The filter is a regular expression of the form
            // (?i)^(Geometry_oM|Structure_oM|...)$ where:
            //  - Regex.Escape makes any special character in an assembly name match literally,
            //  - ^( ... )$ requires the whole assembly name to match one of the alternatives separated by |, so that Geometry_oM does not also match Revit_Geometry_oM,
            //  - (?i) makes the match case-insensitive, as the names given by the loader and by the engine can differ in case.
            // For loadedAssemblies = ["Geometry_oM", "Structure_oM", "Rhinoceros_Engine"], it produces this string: (?i)^(Geometry_oM|Structure_oM|Rhinoceros_Engine)$
            string assemblyFilter = "(?i)^(" + string.Join("|", loadedAssemblies.Select(Regex.Escape)) + ")$";
            List<CodeElementRecord> loadedCodeElements = BH.Engine.UI.Query.CodeElements(assemblyFilter);

            List<CodeElementRecord> result = codeElements
                .Where(x => !loadedAssemblies.Contains(x.AssemblyName))
                .Concat(loadedCodeElements)
                .ToList();

            // Information only: the content of these assemblies is saved as extracted, and they are not tried again until they change
            HashSet<string> incompleteAssemblies = new HashSet<string>(BH.Engine.Base.Query.AssembliesWithExtractionIssues(), StringComparer.OrdinalIgnoreCase);
            foreach (string assemblyName in loadedAssemblies.Where(x => incompleteAssemblies.Contains(x)))
                BH.Engine.Base.Compute.RecordNote($"Assembly {assemblyName} could not be fully extracted in this process. Only the elements that could be extracted are available.");

            if (!string.IsNullOrEmpty(filePath))
                SaveAssemblyContent(result, filePath);

            stopwatch.Stop();
            BH.Engine.Base.Compute.RecordNote($"Time to update the code elements with the content of the updated/new assemblies: {stopwatch.Elapsed.TotalMilliseconds / 1000} s.");

            return result;
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private static void SaveAssemblyContent(List<CodeElementRecord> codeElements, string filePath)
        {
            List<string> lines = codeElements
                .Select(x => x.ToTsv())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();

            string tempPath = filePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                string folder = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(folder))
                    Directory.CreateDirectory(folder);

                // Write to a temporary file first so that concurrent readers never see a half-written file
                File.WriteAllLines(tempPath, lines);
                if (File.Exists(filePath))
                    File.Replace(tempPath, filePath, null);
                else
                    File.Move(tempPath, filePath);
            }
            catch (Exception e)
            {
                BH.Engine.Base.Compute.RecordError(e, $"Failed to save the assembly content to {filePath}.");
                try { File.Delete(tempPath); } catch { }
            }
        }

        /*************************************/
    }
}
