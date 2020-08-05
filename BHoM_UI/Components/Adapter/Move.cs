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
using BH.oM.Base;
using BH.oM.Adapter;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Base.Components
{
    public class MoveCaller : Caller
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

        [Description("Adapter Action 'Move': copies objects from an external software to another. \nThe objects do not pass through the UI to avoid performance bottleneck.")]
        [Input("source", "Adapter the data is copied from")]
        [Input("target", "Adapter the data is copied to")]
        [Input("request", "Filter on the objects to pull (default: get all)")]
        [Input("pullConfig", "Configuration for the Pull actioned by the Move. You can input an ActionConfig (it contains the configs common to all Toolkits); \n" +
            "consider that Toolkits may have a custom ActionConfig (e.g. GSAConfig, SpeckleConfig).")]
        [Input("pushConfig", "Configuration for the Push actioned by the Move. You can input an ActionConfig (it contains the configs common to all Toolkits); \n" +
            "consider that Toolkits may have a custom ActionConfig (e.g. GSAConfig, SpeckleConfig).")]
        [Input("active", "Execute the Move")]
        [Output("success", "Define if the Move was successful")]
        public static bool Move(BHoMAdapter source, BHoMAdapter target, IRequest request = null, 
            PullType pullType = PullType.AdapterDefault, ActionConfig pullConfig = null,
            PushType pushType = PushType.AdapterDefault, ActionConfig pushConfig = null, bool active = false)
        {
            if (source == null || target == null)
            {
                Engine.Reflection.Compute.RecordError("Adapter input cannot be null.");
                return false;
            }

            // ---------------------------------------------//
            // Mandatory Adapter Action set-up              //
            //----------------------------------------------//
            // The following are mandatory set-ups to be ALWAYS performed 
            // before the Adapter Action is called,
            // whether the Action is overrided at the Toolkit level or not.

            // If unset, set the actionConfig to a new ActionConfig.
            pullConfig = pullConfig == null ? new ActionConfig() : pullConfig;
            pushConfig = pushConfig == null ? new ActionConfig() : pushConfig;

            if (request == null)
                request = new FilterRequest();

            //----------------------------------------------//

            if (active)
                return source.Move(target, request, pullType, pullConfig, pushType, pushConfig);
            else
                return false;
        }

        /*************************************/
    }
}

