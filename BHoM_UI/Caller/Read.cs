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

using BH.Engine.Reflection;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BH.Engine.Serialiser;
using System.Windows.Forms;
using BH.oM.Base;
using System.Collections;
using BH.Engine.UI;

namespace BH.UI.Base
{
    public abstract partial class Caller
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public virtual bool Read(string json)
        {
            if (json == "")
                return true;

            try
            {
                object obj = BH.Engine.Serialiser.Convert.FromJson(json);
                CustomObject component = obj as CustomObject;

                // Old component, serialised only with the SelectedItem as object
                if (component == null)
                {
                    m_IsMissingParamInfo = true;
                    SetItem(obj, false);
                    return true;
                }

                // New serialisation, we stored a CustomObject with SelectedItem, InputParams and OutputParams
                object selectedItem = null;
                List<ParamInfo> inputParams;
                List<ParamInfo> outputParams;
                ExtractSavedData(component, out selectedItem, out inputParams, out outputParams);

                // If selected item is null, try to recover it
                if (selectedItem == null)
                    selectedItem = RecoverFromNullSelectedItem(json, inputParams, outputParams);

                // If selected Item is not null, restore it
                if (selectedItem != null)
                    return RestoreItem(selectedItem, inputParams, outputParams);
                else
                    return true;
            }
            catch
            {
                BH.Engine.Base.Compute.RecordError($"{this} failed to deserialise itself.");
                return false;
            }
        }


        /*************************************/
        /**** Helper Methods              ****/
        /*************************************/

        protected virtual void ExtractSavedData(CustomObject data, out object selectedItem, out List<ParamInfo> inputParams, out List<ParamInfo> outputParams)
        {
            // Get teh selected item
            selectedItem = null;
            data.CustomData.TryGetValue("SelectedItem", out selectedItem);

            // We also overwrite the InputParams and OutputParams, since we could have made some changes to them - e.g. ListInput
            // Also, if SelectedItem is null, the component will still have its input and outputs

            // Get input params
            object inputParamsRecord;
            if (data.CustomData.TryGetValue("InputParams", out inputParamsRecord))
                inputParams = (inputParamsRecord as IEnumerable).OfType<ParamInfo>().ToList();
            else
                inputParams = null;

            // Get output params
            object outputParamsRecord;
            if (data.CustomData.TryGetValue("OutputParams", out outputParamsRecord))
                outputParams = (outputParamsRecord as IEnumerable).OfType<ParamInfo>().ToList();
            else
                outputParams = null;
        }

        /*************************************/

        protected bool RestoreItem(object selectedItem, List<ParamInfo> inputParams, List<ParamInfo> outputParams)
        {
            // Finally Set the item 
            SetItem(selectedItem, false);

            // Make sure that saved selection is copied over
            if (inputParams != null)
                SelectInputs(inputParams.GroupBy(x => x.Name).Select(g => g.First()).ToDictionary(x => x.Name, x => x.IsSelected));
            if (outputParams != null)
                SelectOutputs(outputParams.GroupBy(x => x.Name).Select(g => g.First()).ToDictionary(x => x.Name, x => x.IsSelected));

            // Look for changes
            CallerUpdate update = new CallerUpdate
            {
                Cause = CallerUpdateCause.ReadFromSave,
                ComponentUpdate = new ComponentUpdate { Name = Name, Description = Description },
                InputUpdates = InputParams.Changes(inputParams).Where(x => x.Param.IsSelected).ToList(),
                OutputUpdates = OutputParams.Changes(outputParams).Where(x => x.Param.IsSelected).ToList()
            };

            // Record warnings if changes happened
            List<IParamUpdate> paramUpdates = update.InputUpdates.Concat(update.OutputUpdates).ToList();
            if (paramUpdates.Count > 0)
                Engine.Base.Compute.RecordWarning("This component was upgraded. Here's the resulting changes: \n" 
                    + paramUpdates.Select(x => "  - " + x.IToText()).Aggregate((a, b) => a + "\n" + b));

            // Send the notification 
            MarkAsModified(update);

            return true;
        }

        /*************************************/

        protected object RecoverFromNullSelectedItem(string json, List<ParamInfo> inputParams, List<ParamInfo> outputParams)
        {
            // Maybe this is an old Create method ?
            object selectedItem = null;
            if (outputParams.Count == 1)
                selectedItem = OldCreateMethodToType(json, inputParams, outputParams);

            // If the selected Item is not found, we need to make sure that the component keeps its old inputs and outputs
            if (selectedItem == null)
            {
                if (inputParams != null)
                    InputParams = inputParams;

                if (outputParams != null)
                    OutputParams = outputParams;

                CompileInputGetters();
                CompileOutputSetters();

                SetInputSelectionMenu();
                SetOutputSelectionMenu();
            }

            return selectedItem;
        }

        /*************************************/

        // This converts old create methods into their corresponding auto-generated constructor
        protected object OldCreateMethodToType(string json, List<ParamInfo> inputParams, List<ParamInfo> outputParams)
        {
            object selectedItem = null;
            CustomObject component = Engine.Serialiser.Convert.FromJson(json.Replace("\"_t\"", "_refType")) as CustomObject;
            if (component != null && outputParams.Count == 1 && component.CustomData.ContainsKey("SelectedItem"))
            {
                CustomObject itemData = component.CustomData["SelectedItem"] as CustomObject;
                if (itemData != null && itemData.CustomData.ContainsKey("_refType") && (string)itemData.CustomData["_refType"] == "System.Reflection.MethodBase")
                {
                    Type type = outputParams.First().DataType;
                    if (!type.IsNotImplemented() && !type.IsDeprecated() && type?.GetInterface("IImmutable") == null && !type.IsEnum && !type.IsAbstract) // Is type constructable ?
                    {
                        // Translate from method param name to property name
                        Dictionary<string, PropertyInfo> properties = type.GetProperties().ToDictionary(x => x.Name.ToLower(), x => x);
                        foreach (ParamInfo parameter in inputParams)
                        {
                            string key = parameter.Name.ToLower();
                            if (properties.ContainsKey(key) && parameter.DataType == properties[key].PropertyType)
                                parameter.Name = properties[key].Name;
                            else
                            {
                                List<PropertyInfo> typeProps = properties.Values.Where(x => x.PropertyType == parameter.DataType).ToList();
                                if(typeProps.Count == 1)
                                    parameter.Name = typeProps[0].Name;
                            }

                        }

                        // Set the type item 
                        selectedItem = type;
                        if (selectedItem is Type)
                            m_CompiledFunc = Engine.UI.Compute.Constructor((Type)selectedItem, inputParams);
                    }
                }
            }

            return selectedItem;
        }

        /*************************************/
    }
}






