/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using BH.Engine.Serialiser;
using BH.oM.Reflection.Attributes;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BH.Engine.UI
{
    public static partial class Compute
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description(@"Set the project number or unique project ID this script is associated with.")]
        [Input("projectID", "Project number/unique project ID this script is associated with.")]
        [Output("success", "Returns true if the project ID was correctly registered.")]
        public static bool SetProjectID(string projectID)
        {
            if (projectID == "")
            {
                Engine.Reflection.Compute.RecordWarning("Please enter the project number your work in this script relates to");
                return false;
            }
                
            Engine.Reflection.Compute.RecordEvent(new ProjectIDEvent
            {
                Message = "The project ID for this file is now set to " + projectID,
                ProjectID = projectID
            });

            return true;
        }

        /*************************************/
    }
}


