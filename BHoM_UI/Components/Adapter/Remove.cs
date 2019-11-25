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
    public class RemoveCaller : MethodCaller
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

        [Description("Remove objects in the external software")]
        [Input("adapter", "Adapter to the external software")]
        [Input("request", "Specifies which objects to be deleted")]
        [Input("config", "Delete config")]
        [Input("active", "Execute the delete")]
        [Output("#deleted", "Number of objects that have been deleted")]
        public static int Remove(BHoMAdapter adapter, IRequest request = null, Dictionary<string, object> actionConfig = null, bool active = false)
        {
            // ---------------------------------------------//
            // Mandatory Adapter Action set-up              //
            //----------------------------------------------//
            // The following are mandatory set-ups to be ALWAYS performed 
            // before the Adapter Action is called,
            // whether the Action is overrided at the Toolkit level or not.

            // If specified, set the global ActionConfig value, otherwise make sure to reset it.
            adapter.ActionConfig = actionConfig == null ? new Dictionary<string, object>() : actionConfig;

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
