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

            //Make sure the source and target folders exists
            if (!Directory.Exists(sourceFolder))
                throw new DirectoryNotFoundException("The source folder does not exists: " + sourceFolder);
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);
            else
                CleanDirectory(targetFolder);
                

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
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to copy sub-folders from " + path);
                    }
                }
                else
                {
                    Console.WriteLine("Failed to collect files from " + path);
                }   
            }

            // Copy al the files accross
            Console.WriteLine("\nCopying " + filesToMove.Count.ToString() + " files to " + targetFolder);
            foreach (KeyValuePair<string, List<string>> kvp in filesToMove)
            {
                string file = kvp.Value.OrderBy(x => File.GetLastWriteTime(x)).Last();
                File.Copy(file, Path.Combine(targetFolder, kvp.Key), true);
            }

        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private static void CleanDirectory(string folder)
        {
            DirectoryInfo di = new DirectoryInfo(folder);

            foreach (FileInfo file in di.GetFiles())
                file.Delete();
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
    }
}
