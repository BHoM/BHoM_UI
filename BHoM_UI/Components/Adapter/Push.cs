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
using BH.oM.Reflection;
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
    public class PushCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Push;

        public override Guid Id { get; protected set; } = new Guid("F27E94AD-6939-41AA-B680-094BA245F5C1");

        public override string Category { get; protected set; } = "Adapter";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        // The wrapping of the Adapter method in the Caller is needed in order to specify the `active` boolean input
        public PushCaller() : base(typeof(PushCaller).GetMethod("Push")) { }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        [Description("Push objects to the external software")]
        [Input("adapter", "Adapter to the external software")]
        [Input("objects", "Objects to push")]
        [Input("tag", "Tag to apply to the objects being pushed")]
        [Input("config", "Push config")]
        [Input("active", "Execute the push")]
        [MultiOutput(0, "objects", "Objects that have been pushed (with potentially additional information stored in their CustomData to reflect the push)")]
        [MultiOutput(1, "success", "Define if the push was sucessful")]
        public static Output<List<IObject>, bool> Push(BHoMAdapter adapter, IEnumerable<IObject> objects, string tag = "",
            PushOption pushOption = PushOption.Unset, Dictionary<string, object> config = null, bool active = false)
        {
            // ---------------------------------------------//
            // Mandatory Adapter Action set-up              //
            //----------------------------------------------//
            // The following are mandatory set-ups to be ALWAYS performed 
            // before the Adapter Action is called,
            // whether the Action is overrided at the Toolkit level or not.

            // If specified, set the global ActionConfig value, otherwise make sure to reset it.
            adapter.ActionConfig = config == null ? new Dictionary<string, object>() : config;

            //----------------------------------------------//

            List<IObject> result = new List<IObject>();
            if (active)
                result = adapter.Push(objects, tag, pushOption, config);

            return BH.Engine.Reflection.Create.Output(result, result.Count() == objects.Count());
        }

        /*************************************/
    }
}
