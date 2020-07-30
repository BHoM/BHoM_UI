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
using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Components
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
        /**** Public Method               ****/
        /*************************************/

        [Description("Adapter Action 'Remove': removes the objects from an external software.")]
        [Input("adapter", "Adapter to the external software")]
        [Input("request", "Specifies which objects to be removed")]
        [Input("actionConfig", "Configuration for this Action. You can input an ActionConfig (it contains the configs common to all Toolkits); \n" +
            "consider that Toolkits may have a custom ActionConfig (e.g. GSAConfig, SpeckleConfig).")]
        [Input("active", "Execute the delete")]
        [Output("#removed", "Number of objects that have been removed")]
        public static int Remove(BHoMAdapter adapter, IRequest request = null, ActionConfig actionConfig = null, bool active = false)
        {
            // ---------------------------------------------//
            // Mandatory Adapter Action set-up              //
            //----------------------------------------------//
            // The following are mandatory set-ups to be ALWAYS performed 
            // before the Adapter Action is called,
            // whether the Action is overrided at the Toolkit level or not.

            // If unset, set the actionConfig to a new ActionConfig.
            actionConfig = actionConfig == null ? new ActionConfig() : actionConfig;

            if (request == null)
                request = new FilterRequest();
            //----------------------------------------------//

            if (active)
                return adapter.Remove(request, actionConfig);
            else
                return 0;
        }

        /*************************************/
    }
}

