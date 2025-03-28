/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
    public class ExecuteCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Execute;

        public override Guid Id { get; protected set; } = new Guid("D45AD8E8-CF03-464C-BA89-2122F4C6E4FA");

        public override string Category { get; protected set; } = "Adapter";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        // The wrapping of the Adapter method in the Caller is needed in order to specify the `active` boolean input
        public ExecuteCaller() : base(typeof(ExecuteCaller).GetMethod("Execute")) { }


        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        protected override bool ShouldCalculateNewResult(List<object> inputs, ref object result)
        {
            bool active = (bool)inputs.Last();
            if (!active && m_CompiledSetters.Count > 0)
            {
                Output<List<object>, bool> output = result as Output<List<object>, bool>;
                if (output != null)
                    output.Item2 = false;
            }

            return active;
        }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        [Description("Adapter Action 'Execute': sends commands to be executed in an external software")]
        [Input("adapter", "Adapter to the external software")]
        [Input("command", "Command to run")]
        [Input("parameters", "Parameters of the command")]
        [Input("actionConfig", "Configuration for this Action. You can input an ActionConfig (it contains the configs common to all Toolkits); \n" +
            "consider that Toolkits may have a custom ActionConfig (e.g. GSAConfig, SpeckleConfig).")]
        [Input("active", "Execute the command")]
        [MultiOutput(0, "output", "Output of the executed command.")]
        [MultiOutput(1, "success", "True if the operation was successful.")]
        public static Output<List<object>, bool> Execute(BHoMAdapter adapter, IExecuteCommand command, ActionConfig actionConfig = null, bool active = false)
        {
            Output<List<object>, bool> result = new Output<List<object>, bool>() { Item1 = null, Item2 = false };

            if (!active)
                return result;

            ActionConfig executeConfig = null;
            if (!adapter.SetupExecuteConfig(actionConfig, out executeConfig))
            {
                BH.Engine.Base.Compute.RecordError($"Invalid `{nameof(actionConfig)}` input.");
                return result;
            }

            if(!adapter.BeforeExecute(command, executeConfig))
            {
                BH.Engine.Base.Compute.RecordError($"An error occurred within the setup actions for the Execute. Please rectify those issues to use the Execute component.");
                return result;
            }

            result = adapter.Execute(command, executeConfig); // Item1 is the result of the Execute; Item2 the `success` bool.

            if (!adapter.AfterExecute(command, executeConfig))
                BH.Engine.Base.Compute.RecordWarning($"An error occurred during the tear down operation for the Execute. Please take note of any additional warnings/errors received from the Execute component.");

            return result != null ? result : new Output<List<object>, bool>() { Item1 = null, Item2 = false };
        }

        /*************************************/
    }
}






