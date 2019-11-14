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
using BH.oM.Data.Requests;
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
    public class MoveCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Move;

        public override Guid Id { get; protected set; } = new Guid("6D2C7F5B-7F64-47C8-AB69-424E5301582F");

        public override string Category { get; protected set; } = "Adapter";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        // The wrapping of the Adapter method in the Caller is needed in order to specify the `active` boolean input
        public MoveCaller() : base(typeof(MoveCaller).GetMethod("Move")) { }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        [Description("Copy objects from a Source adapter to a Target adapter")]
        [Input("source", "Adapter the data is copied from")]
        [Input("target", "Adapter the data is copied to")]
        [Input("request", "Filter on the objects to pull (default: get all)")]
        [Input("pullConfig", "Config options for the Pull from Source adapter")]
        [Input("pushConfig", "Config options for the Push to Target adapter")]
        [Input("active", "Execute the Move")]
        [Output("success", "Define if the Move was successful")]
        public static bool Move(BHoMAdapter source, BHoMAdapter target, IRequest request = null, 
            PullOption pullOption = PullOption.Unset, Dictionary<string, object> pullConfig = null, 
            PushOption pushOption = PushOption.Unset, Dictionary<string, object> pushConfig = null, bool active = false)
        {
            // ---------------------------------------------//
            // Mandatory Adapter Action set-up              //
            //----------------------------------------------//
            // The following are mandatory set-ups to be ALWAYS performed 
            // before the Adapter Action is called,
            // whether the Action is overrided at the Toolkit level or not.

            if (request == null)
                request = new FilterRequest();

            //----------------------------------------------//

            if (active)
                return source.Move(target, request, pullOption, pullConfig, pushOption, pushConfig);
            else
                return false;
        }

        /*************************************/
    }
}
