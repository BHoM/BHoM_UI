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

            // Copy Assemblies 
            CopyAssemblies(sourceFolder, targetFolder);

            // Copy Datasets
            string targetDatasetsFolder = targetFolder.Replace(@"Assemblies", @"DataSets");
            CopyDatasets(sourceFolder, targetDatasetsFolder);

            // Create Upgrades file
            CopyUpgrades(sourceFolder, targetFolder);
        }
    }
}

