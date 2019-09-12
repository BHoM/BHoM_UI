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
    public class UpdatePropertyCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.UpdateProperty;

        public override Guid Id { get; protected set; } = new Guid("33F6744B-AB9C-40B8-8606-479C6E10C2CC");

        public override string Category { get; protected set; } = "Adapter";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        // The wrapping of the Adapter method in the Caller is needed in order to specify the `active` boolean input
        public UpdatePropertyCaller() : base(typeof(UpdatePropertyCaller).GetMethod("UpdateProperty")) { }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        [Description("Update a specific property of objects from the external software")]
        [Input("adapter", "Adapter to the external software")]
        [Input("property", "Name of the property to update values from")]
        [Input("newValue", "New value to assign to the property")]
        [Input("filter", "Filters the objects to be updated")]
        [Input("config", "UpdateProperty config")]
        [Input("active", "Execute the update")]
        [Output("#updated", "Number of objects that have been updated")]
        public static int UpdateProperty(BHoMAdapter adapter, string property, object newValue, FilterRequest filter = null, Dictionary<string, object> config = null, bool active = false)
        {
            if (filter == null)
                filter = new FilterRequest();

            if (active)
                return adapter.UpdateProperty(filter, property, newValue, config);
            else
                return 0;
        }

        /*************************************/
    }
}
