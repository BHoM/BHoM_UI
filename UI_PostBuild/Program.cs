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
            Directory.CreateDirectory(targetFolder);

            // Create the list of files to move starting with the BHoM.dll 
            Dictionary<string, string> filesToMove = new Dictionary<string,string> { { "BHoM.dll", Path.Combine(sourceFolder, @"BHoM\Build\BHoM.dll") } };
            Console.WriteLine("Adding BHoM.dll");

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
                if (Directory.Exists(Path.Combine(path, "Build")))
                {
                    AddFilesToList(filesToMove, Directory.GetFiles(Path.Combine(path, "Build"), "*.dll"));
                    Console.WriteLine("Adding files from " + path);
                }
                else
                {
                    Console.WriteLine("Failed to collect files from " + path);
                }   
            }

            // Copy al the files accross
            Console.WriteLine("\nCopying " + filesToMove.Count.ToString() + " files to " + targetFolder);
            foreach (KeyValuePair<string,string> file in filesToMove)
            {
                File.Copy(file.Value, Path.Combine(targetFolder, file.Key), true);
            }

            Directory.GetDirectories(sourceFolder);
        }


        static void AddFilesToList(Dictionary<string, string> targetList, IEnumerable<string> filesToAdd)
        {
            foreach(string file in filesToAdd)
            {
                string fileName = file.Substring(file.LastIndexOf('\\') + 1);
                if (!targetList.ContainsKey(fileName))
                    targetList.Add(fileName, file);
            }
        }
    }
}
