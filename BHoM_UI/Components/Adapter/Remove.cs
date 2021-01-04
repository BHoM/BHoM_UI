/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
    public class RemoveCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Delete;

        public override Guid Id { get; protected set; } = new Guid("BF39598E-A021-4C52-8D65-20BC491B0BBD");

        public override string Category { get; protected set; } = "Adapter";

        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        // The wrapping of the Adapter method in the Caller is needed in order to specify the `active` boolean input
        public RemoveCaller() : base(typeof(RemoveCaller).GetMethod("Remove")) { }


        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        protected override bool ShouldCalculateNewResult(List<object> inputs, ref object result)
        {
            return (bool)inputs.Last() == true;
        }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        [Description("Adapter Action 'Remove': removes the objects from an external software.")]
        [Input("adapter", "Adapter to the external software")]
        [Input("request", "Specifies which objects to be removed")]
        [Input("actionConfig", "Configuration for this Action. You can input an ActionConfig (it contains the configs common to all Toolkits); \n" +
            "consider that Toolkits may have a custom ActionConfig (e.g. GSAConfig, SpeckleConfig).")]
        [Input("active", "Execute the delete")]
        [Output("#removed", "Number of objects that have been removed")]
        [PreviousVersion("4.0", "BH.UI.Base.Components.RemoveCaller.Remove(BH.Adapter.BHoMAdapter, BH.oM.Data.Requests.IRequest, BH.Adapter.BHoMAdapter.ActionConfig, System.Boolean)")]
        public static int Remove(BHoMAdapter adapter, object request = null, ActionConfig actionConfig = null, bool active = false)
        {
            if (!active)
                return 0;

            IRequest actualRequest = null;
            if (!adapter.SetupRemoveRequest(request, out actualRequest))
            {
                BH.Engine.Reflection.Compute.RecordError($"Invalid `{nameof(request)}` input.");
                return 0;
            }

            ActionConfig removeConfig = null;
            if (!adapter.SetupRemoveConfig(actionConfig, out removeConfig))
            {
                BH.Engine.Reflection.Compute.RecordError($"Invalid `{nameof(actionConfig)}` input.");
                return 0;
            }

            return adapter.Remove(actualRequest, removeConfig);
        }

        /*************************************/
    }
}


