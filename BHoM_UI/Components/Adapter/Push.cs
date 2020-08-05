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
using BH.oM.Reflection;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Base.Components
{
    public class PushCaller : Caller
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

        [Description("Adapter Action 'Push': exports objects to the external software. \nIf the Toolkit supports it, this also performs the replace/update of the objects as appropriate.")]
        [Input("adapter", "Adapter to the external software")]
        [Input("objects", "Objects to push")]
        [Input("tag", "Tag to apply to the objects being pushed")]
        [Input("pushType", "Push type. Connect the enum PushType for all the alternatives.")]
        [Input("actionConfig", "Configuration for this Action. You can input an ActionConfig (it contains the configs common to all Toolkits); \n" +
            "consider that Toolkits may have a custom ActionConfig (e.g. GSAConfig, SpeckleConfig).")]
        [Input("active", "Execute the push")]
        [MultiOutput(0, "objects", "Objects that have been pushed.\nThese objects may be different from the input objects (e.g. their correspondent external software id may be stored in their CustomData).")]
        [MultiOutput(1, "success", "Define if the push was sucessful")]
        public static Output<List<object>, bool> Push(BHoMAdapter adapter, IEnumerable<object> objects, string tag = "",
            PushType pushType = PushType.AdapterDefault, ActionConfig actionConfig = null, 
            bool active = false)
        {
            // ---------------------------------------------//
            // Mandatory Adapter Action set-up              //
            //----------------------------------------------//
            // The following are mandatory set-ups to be ALWAYS performed 
            // before the Adapter Action is called,
            // whether the Action is overrided at the Toolkit level or not.

            // If unset, set the actionConfig to a new ActionConfig.
            actionConfig = actionConfig == null ? new ActionConfig() : actionConfig;

            //----------------------------------------------//

            List<object> result = new List<object>();
            if (active)
                result = adapter.Push(objects, tag, pushType, actionConfig);

            return BH.Engine.Reflection.Create.Output(result, result.Count() == objects.Count());
        }

        /*************************************/
    }
}

