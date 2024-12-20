/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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

using BH.Adapter;
using BH.Engine.Reflection;
using BH.Engine.UI;
using BH.oM.Base;
using BH.oM.UI;
using BH.UI.Base.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BH.UI.Base.Global
{
    public static class Initialisation
    {
        /*************************************/
        /**** Public Properties           ****/
        /*************************************/

        public static DateTime? CompletionTime { get; set; } = null;


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static bool Activate()
        {
            bool success = true;

            success &= LoadToolkitSettings();

            CompletionTime = DateTime.UtcNow;

            return success;
        }

        /*************************************/

        public static bool LoadToolkitSettings()
        {
            string directory = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "BHoM", "Settings");
            if(!Directory.Exists(directory))
            {
                BH.Engine.Base.Compute.RecordWarning($"{directory} doesn't exist. Toolkit settings are not loaded.");
                return false;
            }

            BH.Engine.Settings.Compute.LoadSettings(directory);
            BH.Engine.Settings.Compute.LoadSettings(directory, "*.cfg"); //Legacy cfg files to be loaded in

            List<ISettings> allSettings = BH.Engine.Settings.Query.GetAllSettings();
            List<IInitialisationSettings> initialisationSettings = allSettings.OfType<IInitialisationSettings>().ToList();

            bool success = true;
            foreach(var settings in initialisationSettings)
            {
                try
                {
                    success &= InitialiseToolkit(settings);
                }
                catch(Exception e)
                {
                    BH.Engine.Base.Compute.RecordWarning(e, $"Failed to load settings of type {settings.GetType().Name}.");
                }
            }

            return success;
        }

        /*************************************/

        private static bool InitialiseToolkit(IInitialisationSettings settings)
        {
            // Get details about intialisation method to run
            int separatorIndex = settings.InitialisationMethod.LastIndexOf('.');
            string typeName = settings.InitialisationMethod.Substring(0, separatorIndex);
            string methodName = settings.InitialisationMethod.Substring(separatorIndex + 1);


            // Get method declaring type
            List<Type> typeCandidates = Engine.Base.Create.AllTypes(typeName).Where(x => x.FullName == typeName).ToList();
            if (typeCandidates.Count == 0)
            {
                Engine.Base.Compute.RecordWarning("Type " + typeName + " is unknown");
                return false;
            }
            Type type = typeCandidates.First();

            // Get the method itself
            MethodInfo method = Engine.Reflection.Create.MethodInfo(type, methodName, new List<Type>());
            if (method == null)
            {
                Engine.Base.Compute.RecordWarning("A static method with no argument could not be found for " + settings.InitialisationMethod);
                return false;
            }

            // Calling the method
            try
            {
                method.Invoke(null, null);
                return true;
            }
            catch (Exception e)
            {
                Engine.Base.Compute.RecordWarning("Method " + settings.InitialisationMethod + " failed to run properly during toolkit initialisation. Error: \n" + e.Message);
                return false;
            }
        }

        /*************************************/
    }

}






