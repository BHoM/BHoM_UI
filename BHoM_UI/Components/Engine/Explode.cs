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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using BH.oM.UI;
using BH.oM.Base;
using BH.Engine.Reflection;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Forms;
using BH.Engine.Serialiser;
using BH.Engine.UI;

namespace BH.UI.Base.Components
{
    public class ExplodeCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Explode;

        public override Guid Id { get; protected set; } = new Guid("3647C48A-3322-476F-8B34-4011540AB916");

        public override string Name { get; protected set; } = "Explode";

        public override string Category { get; protected set; } = "Engine";

        public override string Description { get; protected set; } = "Explode an object into its properties";

        public override int GroupIndex { get; protected set; } = 2;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public ExplodeCaller() : base()
        {
            InputParams = new List<ParamInfo>() { new ParamInfo { DataType = typeof(object), Kind = ParamKind.Input, Name = "object", Description = "Object to explode", IsRequired = true } };
            OutputParams = new List<ParamInfo>() { };

            SetOutputSelectionMenu();
        }


        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        public override object Run(List<object> inputs)
        {
            if (inputs.Count != 1)
            {
                BH.Engine.Reflection.Compute.RecordError("The number of inputs is invalid.");
                return null;
            }
            else
            {
                object obj = inputs[0];
                if (obj == null)
                    return Enumerable.Repeat<object>(null, m_CompiledSetters.Count).ToList();

                return OutputParams.Select(x => obj.PropertyValue(x.Name)).ToList();
            }
        }

        /*************************************/

        protected override bool PushOutputs(object result)
        {
            if (!base.PushOutputs(result))
            {
                Engine.Reflection.Compute.ClearCurrentEvents();
                Engine.Reflection.Compute.RecordWarning("Output paramters do not match object properties. Please right click and <Update Outputs>");
                return false;
            }
            else
                return true;
        }

        /*************************************/

        public override void AddToMenu(ToolStripDropDown menu)
        {
            menu.Items.Add(new ToolStripMenuItem("Update Outputs", null, (sender, e) => CollectOutputTypes()));
            menu.Items.Add(new ToolStripSeparator());

            base.AddToMenu(menu);
        }

        /*************************************/

        public override void AddToMenu(System.Windows.Controls.ContextMenu menu)
        {
            System.Windows.Controls.MenuItem item = new System.Windows.Controls.MenuItem { Header = "Update Outputs" };
            item.Click += (sender, e) => CollectOutputTypes();
            menu.Items.Add(item);
            menu.Items.Add(new Separator());

            base.AddToMenu(menu);
        }

        /*************************************/

        public override bool UpdateInput(int index, string name, Type type = null)
        {
            bool isAllowedToUpdate = OutputParams.All(x => x.IsSelected);

            if (isAllowedToUpdate)
                return CollectOutputTypes();
            else
                return false;
        }

        /*************************************/

        protected override void ExtractSavedData(CustomObject data, out object selectedItem, out List<ParamInfo> inputParams, out List<ParamInfo> outputParams)
        {
            base.ExtractSavedData(data, out selectedItem, out inputParams, out outputParams);

            // Take care of backwards compatibility
            if (outputParams == null && data.CustomData.ContainsKey("Outputs") && data.CustomData.ContainsKey("PossibleOutputs"))
            {
                List<object> possibleOutputs = data.CustomData["PossibleOutputs"] as List<object>;
                List<object> selectedOutputs = data.CustomData["Outputs"] as List<object>;
                if (possibleOutputs != null && selectedOutputs != null)
                {
                    List<string> selection = selectedOutputs.OfType<ParamInfo>().Select(x => x.Name).ToList();
                    outputParams = possibleOutputs.OfType<ParamInfo>().ToList();
                    foreach (ParamInfo info in outputParams)
                    {
                        info.Kind = ParamKind.Output;
                        info.IsSelected = selection.Contains(info.Name);
                    }    
                }
            }
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public bool CollectOutputTypes(List<object> objects = null)
        {
            if (objects == null)
                objects = m_DataAccessor.GetAllData(0);

            // Do not update if the list of input is empty or if user has manually selected outputs
            if (objects.Count == 0)
                return false;

            // Save old outputs
            List<ParamInfo> oldOutputs = OutputParams.ToList();

            // Collect the new output params
            OutputParams = Engine.UI.Query.OutputParams(objects);

            // Compile the setters
            CompileOutputSetters();

            // Create the output menu
            SetOutputSelectionMenu();

            // Mark as modified if output have changed
            List<IParamUpdate> changes = OutputParams.Changes(oldOutputs).Where(x => x.Param.IsSelected).ToList();
            if (changes.Count > 0)
                MarkAsModified(new CallerUpdate { Cause = CallerUpdateCause.ItemSelected, OutputUpdates = changes });

            return true;
        }

        /*************************************/
    }
}

