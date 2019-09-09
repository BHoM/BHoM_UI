/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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
    class Program
    {
        static void Main(string[] args)
        {
            // Get programs arguments
            if (args.Length < 2)
            {
                Console.Write("UI PostBuild reqires at least 2 arguments: the source folder and the target folder where the files will be copied.");
                return;
            }
            string sourceFolder = args[0];
            string targetFolder = args[1];

            //set targetDatasetsFolder via Replace below such that we can avoid changing executable arguments. This can be updated once clean up and removal of calling exe from other repos is complete
            string targetAssembliesFolder = targetFolder;
            string targetDatasetsFolder = targetFolder.Replace(@"Assemblies", @"DataSets"); 


            //Make sure the source and target folders exists
            if (!Directory.Exists(sourceFolder))
                throw new DirectoryNotFoundException("The source folder does not exists: " + sourceFolder);
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);
            else
                CleanDirectory(targetFolder);


            //********************* Copy Assemblies *****************************

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

            // Add all the test dlls to the list
            string testPath = Path.Combine(sourceFolder, @"BHoM_Test\Build");
            if (Directory.Exists(testPath))
            {
                AddFilesToList(filesToMove, Directory.GetFiles(Path.Combine(sourceFolder, @"BHoM_Test\Build"), "*.dll"));
                Console.WriteLine("Adding files from BHoM_Test");
            }


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
            Console.WriteLine("\nCopying " + filesToMove.Count.ToString() + " files to " + targetAssembliesFolder);
            foreach (KeyValuePair<string, List<string>> kvp in filesToMove)
            {
                string file = kvp.Value.OrderBy(x => File.GetLastWriteTime(x)).Last();
                File.Copy(file, Path.Combine(targetAssembliesFolder, kvp.Key), true);
            }


            //********************* Copy Datasets *****************************


            foreach (string path in Directory.GetDirectories(sourceFolder).Where((x => x.EndsWith(@"_Datasets") || x.EndsWith(@"_Toolkit"))))
            {
                string datasetPath = Path.Combine(path, "DataSets");

                if (Directory.Exists(datasetPath))
                    CopyJsonInFoldersRecursively(datasetPath, targetDatasetsFolder);
            }
        }

        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static void CleanDirectory(string folder)
        {
            DirectoryInfo di = new DirectoryInfo(folder);
            List<string> skipThese = new List<string> { ".gha", ".xll", ".dna", ".Addin" };
            foreach (FileInfo file in di.GetFiles())
            {
                if (!skipThese.Contains(file.Extension))
                    file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
                dir.Delete(true);
        }

        /*************************************/

        private static void AddFilesToList(Dictionary<string, List<string>> targetList, IEnumerable<string> filesToAdd)
        {
            foreach(string file in filesToAdd)
            {
                string fileName = file.Substring(file.LastIndexOf('\\') + 1);
                if (!targetList.ContainsKey(fileName))
                    targetList.Add(fileName, new List<string> { file });
                else
                    targetList[fileName].Add(file);
            }
        }

        /*************************************/

        private static void CopyJsonInFoldersRecursively(string sourceFolder, string targetFolder)
        {
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);
            
            //Copy all files
            string[] files = Directory.GetFiles(sourceFolder, "*.json");
            foreach (string file in files)
                File.Copy(file, Path.Combine(targetFolder, Path.GetFileName(file)), true);

            //Copy all sub folders
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
                CopyJsonInFoldersRecursively(folder, Path.Combine(targetFolder, Path.GetFileName(folder)));
            
        }


    /*************************************/
}
}
