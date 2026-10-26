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

        [Description("Loads the assemblies matching the filter that are new or newer than the ones recorded in the code elements, and updates the code elements and the file they are saved in with the content of those assemblies. Assemblies that could not be fully extracted in the current process are kept in the returned code elements, but are not saved to the file, so that another process can extract them completely.")]
        [Input("codeElements", "Code elements currently known. Elements of the assemblies that get reloaded are replaced. They are expected to be the content read from the file at filePath: the file is rewritten from them, so the elements of incomplete assemblies in the returned list must not be passed back in.")]
        [Input("assemblyNameFilter", "Only assemblies with a name matching this regular expression are considered. If null or empty, the default filter for BHoM assemblies is used.")]
        [Input("filePath", "Path of the file the updated code elements are saved to, one tsv row per element. If null or empty, nothing is saved. The file is replaced atomically.")]
        [Output("codeElements", "Code elements updated with the content of the new assemblies, including the ones of incomplete assemblies. For use in the current process only: it is not suitable as the input of a later call that saves to a file. This is the input list itself if no assembly was loaded.")]
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

            HashSet<string> droppedAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            List<CodeElementRecord> loadedCodeElements = BH.Engine.UI.Query.CodeElements("(?i)" + string.Join("|", loadedAssemblies.Select(x => "^" + Regex.Escape(x) + "$")), droppedAssemblies);

            List<CodeElementRecord> result = codeElements
                .Where(x => !loadedAssemblies.Contains(x.AssemblyName))
                .Concat(loadedCodeElements)
                .ToList();

            // An assembly is incomplete if the engine had issues extracting it or if some of its elements could not be converted to code elements
            HashSet<string> incompleteAssemblies = new HashSet<string>(droppedAssemblies.Concat(BH.Engine.Base.Query.AssembliesWithExtractionIssues()), StringComparer.OrdinalIgnoreCase);
            foreach (string assemblyName in loadedAssemblies.Where(x => incompleteAssemblies.Contains(x)))
                BH.Engine.Base.Compute.RecordNote($"Assembly {assemblyName} could not be fully extracted in this process. Its content is not saved to the assembly content file.");

            if (!string.IsNullOrEmpty(filePath))
            {
                // The file gets the input elements, without the stale ones of the reloaded assemblies, and the elements of the reloaded assemblies that are complete
                List<CodeElementRecord> toSave = codeElements
                    .Where(x => !loadedAssemblies.Contains(x.AssemblyName))
                    .Concat(loadedCodeElements.Where(x => !incompleteAssemblies.Contains(x.AssemblyName)))
                    .ToList();

                SaveAssemblyContent(toSave, filePath);
            }

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
