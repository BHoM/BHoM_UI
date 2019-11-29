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
    public class PullCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Pull;

        public override Guid Id { get; protected set; } = new Guid("B25011DD-5F30-4279-B9D9-0F9C169D6685");

        public override string Category { get; protected set; } = "Adapter";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        // The wrapping of the Caller is needed in order to specify the `active` boolean input
        public PullCaller() : base(typeof(PullCaller).GetMethod("Pull")) { }


        /*************************************/
        /**** Override Method             ****/
        /*************************************/

        public override object Run(object[] inputs)
        {
            if (inputs.Length > 0)
            {
                BHoMAdapter adapter = inputs[0] as BHoMAdapter;
                Guid id = adapter.AdapterGuid;
                if (id != m_AdapterId)
                {
                    m_AdapterId = id;
                    adapter.DataUpdated += (sender, e) => OnDataUpdated();
                }
            }

            return base.Run(inputs);
        }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        [Description("Pull objects from the external software")]
        [Input("adapter", "Adapter to the external software")]
        [Input("request", "Connect a type of object to Pull objects of that type." +
            "Connect a Request to pull only objects that satisfy a certain rule.")]
        [Input("config", "Pull config")]
        [Input("active", "Execute the pull")]
        [Output("objects", "Objects pulled")]
        public static IEnumerable<object> Pull(BHoMAdapter adapter, IRequest request = null, 
            PullType pullType = PullType.AdapterDefault, ActionConfig actionConfig = null, 
            bool active = false)
        {
            IRequest actualRequest = null;

            // ---------------------------------------------//
            // Mandatory Adapter Action set-up              //
            //----------------------------------------------//
            // The following are mandatory set-ups to be ALWAYS performed 
            // before the Adapter Action is called,
            // whether the Action is overrided at the Toolkit level or not.
 
            if (request == null)
                actualRequest = new FilterRequest();

            Type objType = request as Type;
            if (objType != null)
            {
                // it's a Type. Check the namespace to see if it's to pull results.
                bool isResult = objType.FullName.IndexOf("result", StringComparison.OrdinalIgnoreCase) >= 0;
                if (isResult)
                {
                    Engine.Reflection.Compute.RecordError("In order to Pull results, please input a ResultRequest.");
                    return new List<object>();
                }
                else
                    actualRequest = Engine.Data.Create.FilterRequest(objType, ""); // Concept: add a new TypeRequest
            }

            //----------------------------------------------//

            if (active)
                return adapter.Pull(actualRequest, pullType, actionConfig);
            else
                return new List<object>();
        }

        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        private Guid m_AdapterId;

        /*************************************/
    }
}
