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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using BH.oM.UI;
using BH.oM.Data.Collections;
using BH.Engine.Reflection;
using BH.Engine.Data;
using BH.Engine.Serialiser;
using System.Windows.Forms;
using System.Drawing;

namespace BH.UI.Base.Components
{
    public class SearchSettingsCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.SearchSettings;

        public override Guid Id { get; protected set; } = new Guid("75F9B96E-8CF2-4D8A-B41C-049D7CC97467");

        public override string Category { get; protected set; } = "UI";

        public override string Name { get; protected set; } = "SearchSettings";

        public override string Description { get; protected set; } = "Limits the results shown in the BHoM UI menus by excluding some toolkits. Once created, save thse settings using the SaveSettings component.";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public SearchSettingsCaller() : base()
        {
            InputParams = new List<ParamInfo>() { new ParamInfo { DataType = typeof(List<string>), Kind = ParamKind.Input, Name = "excludedToolkits", Description = "List of toolkits that will be excluded from the UI menus. The toolkit are provided as text corresponding to the toolkit part of the namespace (e.g. 'Acoustic' for 'BH.oM.Acoustic')", IsRequired = true } };
            OutputParams = new List<ParamInfo>() { new ParamInfo { DataType = typeof(object), Kind = ParamKind.Output, Name = Name, Description = Description } };
        }

        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        public override object Run(List<object> inputs)
        {
            if (inputs?.Count == 0)
            {
                BH.Engine.Base.Compute.RecordError("No input provided");
                return null;
            }

            List<string> excludedToolkis = inputs[0] as List<string>;
            if (excludedToolkis == null)
            {
                BH.Engine.Base.Compute.RecordError("List of toolkits could not be collected.");
                return null;
            }

            return new SearchSettings { ExcludedToolkits = excludedToolkis };
        }

        /*************************************/
    }
}




