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

using BH.Adapter;
using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using BH.oM.Reflection.Attributes;
using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Components
{
    public class ExecuteCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Execute;

        public override Guid Id { get; protected set; } = new Guid("D45AD8E8-CF03-464C-BA89-2122F4C6E4FA");

        public override string Category { get; protected set; } = "Adapter";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public ExecuteCaller() : base(typeof(ExecuteCaller).GetMethod("Execute")) { }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        [Description("Execute command in the external software")]
        [Input("adapter", "Adapter to the external software")]
        [Input("command", "Command to run")]
        [Input("parameters", "Parameters of the command")]
        [Input("config", "Execute config")]
        [Input("active", "Execute the command")]
        [Output("Confirms the success of the operation")]
        public static bool Execute(BHoMAdapter adapter, string command, Dictionary<string, object> parameters = null, Dictionary<string, object> config = null, bool active = false)
        {
            if (active)
                return adapter.Execute(command, parameters, config);
            else
                return false;
        }

        /*************************************/
    }
}
