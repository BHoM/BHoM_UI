/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using BH.oM.Base.Attributes;
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
        /**** Override Methods            ****/
        /*************************************/

        protected override bool ShouldCalculateNewResult(List<object> inputs, ref object result)
        {
            bool active = (bool)inputs.Last();
            if (!active && m_CompiledSetters.Count > 0)
                result = false;

            return active;
        }


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
        public static bool Move(BHoMAdapter source, BHoMAdapter target, object request = null,
            PullType pullType = PullType.AdapterDefault, ActionConfig pullConfig = null,
            PushType pushType = PushType.AdapterDefault, ActionConfig pushConfig = null, bool active = false)
        {
            if (!active)
                return false;

            IRequest actualRequest = null;
            if (!source.SetupPullRequest(request, out actualRequest))
            {
                BH.Engine.Base.Compute.RecordError($"Invalid `{nameof(request)}` input.");
                return false;
            }

            ActionConfig pullCfg = null;
            if (!source.SetupPullConfig(pullConfig, out pullCfg))
            {
                BH.Engine.Base.Compute.RecordError($"Invalid `{nameof(pullConfig)}` input.");
                return false;
            }

            ActionConfig pushCfg = null;
            if (!source.SetupPushConfig(pushConfig, out pushCfg))
            {
                BH.Engine.Base.Compute.RecordError($"Invalid `{nameof(pushConfig)}` input.");
                return false;
            }

            if(!source.BeforeMove(target, actualRequest, pullType, pullCfg, pushType, pushCfg))
            {
                BH.Engine.Base.Compute.RecordError($"An error occurred within the setup actions for the Move. Please rectify those issues to use the Move component.");
                return false;
            }


            bool result = source.Move(target, actualRequest, pullType, pullCfg, pushType, pushCfg);

            if(!source.AfterMove(target, actualRequest, pullType, pullCfg, pushType, pushCfg))
                BH.Engine.Base.Compute.RecordWarning($"An error occurred during the tear down operation for the Move. Please take note of any additional warnings/errors received from the Move component.");

            return result;
        }

        /*************************************/
    }
}





