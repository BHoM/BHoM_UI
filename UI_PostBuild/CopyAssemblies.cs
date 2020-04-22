/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BHoM_UI
{
    partial class Program
    {
        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static void CopyAssemblies(string sourceFolder, string targetFolder)
        {
            // Create the list of files to move starting with the BHoM.dll 
            Dictionary<string, List<string>> filesToMove = new Dictionary<string, List<string>>();

            // Add all the oM dlls to the list
            AddFilesToList(filesToMove, Directory.GetFiles(Path.Combine(sourceFolder, @"BHoM\Build"), "*.dll"));
            Console.WriteLine("Adding files from BHoM");

            // Add all the Engine dlls to the list
            AddFilesToList(filesToMove, Directory.GetFiles(Path.Combine(sourceFolder, @"BHoM_Engine\Build"), "*.dll"));
            Console.WriteLine("Adding files from BHoM_Engine");

            // Add all the Adapter dlls to the list
            AddFilesToList(filesToMove, Directory.GetFiles(Path.Combine(sourceFolder, @"BHoM_Adapter\Build"), "*.dll"));
            Console.WriteLine("Adding files from BHoM_Adapter");

            // Add all the UI dlls to the list
            AddFilesToList(filesToMove, Directory.GetFiles(Path.Combine(sourceFolder, @"BHoM_UI\Build"), "*.dll"));
            Console.WriteLine("Adding files from BHoM_UI");

            // Add all the Toolkit dlls to the list
            foreach (string path in Directory.GetDirectories(sourceFolder).Where(x => x.EndsWith(@"_Toolkit")))
            {
                string buildPath = Path.Combine(path, "Build");
                if (Directory.Exists(buildPath))
                {
                    AddFilesToList(filesToMove, Directory.GetFiles(buildPath, "*.dll"));
                    Console.WriteLine("Adding files from " + path);

                    try
                    {
                        foreach (string folderPath in Directory.GetDirectories(buildPath))
                        {
                            foreach (string file in Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories))
                            {
                                string target = file.Replace(buildPath, targetFolder);
                                Directory.CreateDirectory(Path.GetDirectoryName(target));
                                File.Copy(file, target, true);
                            }

                        }
                    }
                    catch
                    {
                        Console.WriteLine("Failed to copy sub-folders from " + path);
                    }
                }
                else
                {
                    Console.WriteLine("Failed to collect files from " + path);
                }
            }

            // Copy all the files accross
            Console.WriteLine("\nCopying " + filesToMove.Count.ToString() + " files to " + targetFolder);
            foreach (KeyValuePair<string, List<string>> kvp in filesToMove)
            {
                string file = kvp.Value.OrderBy(x => File.GetLastWriteTime(x)).Last();
                File.Copy(file, Path.Combine(targetFolder, kvp.Key), true);
            }
        }


        /***************************************************/
        /**** Helper Methods                            ****/
        /***************************************************/

        private static void AddFilesToList(Dictionary<string, List<string>> targetList, IEnumerable<string> filesToAdd)
        {
            foreach (string file in filesToAdd)
            {
                string fileName = file.Substring(file.LastIndexOf('\\') + 1);
                if (!targetList.ContainsKey(fileName))
                    targetList.Add(fileName, new List<string> { file });
                else
                    targetList[fileName].Add(file);
            }
        }

        /***************************************************/
    }
}
