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

using BH.Adapter;
using BH.Engine.Reflection;
using BH.Engine.UI;
using BH.oM.UI;
using BH.UI.Components;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BH.UI.Global
{
    public static class Initialisation
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static bool Activate()
        {
            bool success = true;

            success &= LoadToolkitSettings();

            return success;
        }

        /*************************************/

        public static bool LoadToolkitSettings()
        {
            if (!Directory.Exists(@"C:\ProgramData\BHoM\Settings"))
            {
                Engine.Reflection.Compute.RecordWarning(@"C:\ProgramData\BHoM\Settings doesn't exist. Toolkits setting are not loaded.");
                return false;
            }

            bool success = true;
            foreach (string file in Directory.GetFiles(@"C:\ProgramData\BHoM\Settings", "*.cfg"))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                ISettings settings = Engine.UI.Query.Settings(fileName);

                // Initialise the toolkit if needed
                if (settings is IInitialisationSettings)
                    success = InitialiseToolkit(settings as IInitialisationSettings);
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
            List<Type> typeCandidates = Engine.Reflection.Create.AllTypes(typeName).Where(x => x.FullName == typeName).ToList();
            if (typeCandidates.Count == 0)
            {
                Engine.Reflection.Compute.RecordWarning("Type " + typeName + " is unknown");
                return false;
            }
            Type type = typeCandidates.First();

            // Get the method itself
            MethodInfo method = Engine.Reflection.Create.MethodInfo(type, methodName, new List<Type>());
            if (method == null)
            {
                Engine.Reflection.Compute.RecordWarning("A static method with no argument could not be found for " + settings.InitialisationMethod);
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
                Engine.Reflection.Compute.RecordWarning("Method " + settings.InitialisationMethod + " failed to run properly during toolkit initialisation. Error: \n" + e.Message);
                return false;
            }
        }

        /*************************************/
    }

}

