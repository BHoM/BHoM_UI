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

using BH.Engine.Reflection;
using BH.oM.Reflection;
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

namespace BH.UI.Templates
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
                    SetItem(obj);
                    return true;
                }

                // New serialisation, we stored a CustomObject with SelectedItem, InputParams and OutputParams
                object selectedItem = null;
                List<ParamInfo> inputParams = new List<ParamInfo>();
                List<ParamInfo> outputParams = new List<ParamInfo>();
                ExtractSavedData(component, out selectedItem, out inputParams, out outputParams);

                // If selected item is null, try to recover it
                if (selectedItem == null)
                    RecoverFromNullSelectedItem(json, inputParams, outputParams);
                
                // Make sure that saved params are matching the ones generated from selected item (in case of versioning)
                if (selectedItem != null)
                    EnsureMatchingParams(inputParams, outputParams);

                return true;
            }
            catch
            {
                BH.Engine.Reflection.Compute.RecordError($"{this} failed to deserialise itself.");
                return false;
            }
        }


        /*************************************/
        /**** Helper Methods              ****/
        /*************************************/

        protected void ExtractSavedData(CustomObject data, out object selectedItem, out List<ParamInfo> inputParams, out List<ParamInfo> outputParams)
        {
            // Get teh selected item
            selectedItem = null;
            if (data.CustomData.TryGetValue("SelectedItem", out selectedItem))
                SetItem(selectedItem);

            // We also overwrite the InputParams and OutputParams, since we could have made some changes to them - e.g. ListInput
            // Also, if SelectedItem is null, the component will still have its input and outputs

            // Get input params
            object inputParamsRecord;
            if (data.CustomData.TryGetValue("InputParams", out inputParamsRecord))
                inputParams = (inputParamsRecord as IEnumerable).OfType<ParamInfo>().ToList();
            else
                inputParams = new List<ParamInfo>();

            // Get output params
            object outputParamsRecord;
            if (data.CustomData.TryGetValue("OutputParams", out outputParamsRecord))
                outputParams = (outputParamsRecord as IEnumerable).OfType<ParamInfo>().ToList();
            else
                outputParams = new List<ParamInfo>();
        }

        /*************************************/

        protected void RecoverFromNullSelectedItem(string json, List<ParamInfo> inputParams, List<ParamInfo> outputParams)
        {
            // Maybe this is an old Create method ?
            if (outputParams.Count == 1)
                OldCreateMethodToType(json);

            // If the selected Item is not found, we need to make sure that the component keeps its old inputs and outputs
            if (SelectedItem == null)
            {
                InputParams = inputParams;
                CompileInputGetters();

                OutputParams = outputParams;
                CompileOutputSetters();
            }

            return;
        }

        /*************************************/

        protected void EnsureMatchingParams(List<ParamInfo> inputParams, List<ParamInfo> outputParams)
        {
            bool matchingInputs = true;
            if (SelectedItem is Type)
                matchingInputs = Engine.UI.Query.AreMatching(InputParams, inputParams);
            else
                matchingInputs = Engine.UI.Query.AreMatching(((Type)SelectedItem).GetProperties().ToList(), inputParams);

            if (!matchingInputs || !Engine.UI.Query.AreMatching(OutputParams, outputParams))
            {
                FindOldIndex(InputParams, inputParams);
                FindOldIndex(OutputParams, outputParams);

                WasUpgraded = true;
                MarkAsModified(new CallerUpdate
                {
                    Cause = CallerUpdateCause.UpgradedVersion
                });
            }
        }

        /*************************************/

        protected void FindOldIndex(List<ParamInfo> newList, List<ParamInfo> oldList)
        {
            List<int> newToMatch = Enumerable.Range(0, newList.Count).ToList();
            List<int> oldToMatch = Enumerable.Range(0, oldList.Count).ToList();

            // First match using names
            for (int i = 0; i < newList.Count; i++)
            {
                ParamInfo parameter = newList[i];
                int oldIndex = oldList.FindIndex(x => x.Name == parameter.Name);
                if (oldIndex >= 0)
                {
                    newToMatch.Remove(i);
                    oldToMatch.Remove(oldIndex);
                }
                parameter.Fragments.AddOrReplace(new ParamOldIndexFragment { OldIndex = oldIndex });
            }

            // Then match using types on the remaining params (Only allowed when a single matching type is found)
            List<int> newToMatchCopy = newToMatch.ToList();
            foreach (int i in newToMatchCopy)
            {
                ParamInfo parameter = newList[i];
                IEnumerable<int> matches = oldToMatch.Where(x => parameter.DataType == oldList[x].DataType);
                if (matches.Count() == 1)
                {
                    int oldIndex = matches.First();
                    newToMatch.Remove(i);
                    oldToMatch.Remove(oldIndex);
                    parameter.Fragments.AddOrReplace(new ParamOldIndexFragment { OldIndex = oldIndex });
                }
            }

            // Then match using indices (Only allowed when number of parameters are equal for new and old)
            if (newToMatch.Count == oldToMatch.Count)
            {
                newToMatchCopy = newToMatch.ToList();
                foreach (int i in newToMatchCopy)
                {
                    if (oldToMatch.Contains(i))
                    {
                        newToMatch.Remove(i);
                        oldToMatch.Remove(i);
                        newList[i].Fragments.AddOrReplace(new ParamOldIndexFragment { OldIndex = i });
                    }
                }
            }

            // Provide a warning message if not all parameters where matched successfully
            if (newToMatch.Count > 0 && oldToMatch.Count > 0)
            {
                string message = "This component was upgraded but the following parameters could not be matched with existing ones:\n";
                message += newToMatch.Select(i => " - " + newList[i].Name).Aggregate((a, b) => a + "\n" + b);
                Engine.Reflection.Compute.RecordWarning(message);
            }
        }

        /*************************************/

        // This converts old create methods into their corresponding auto-generated constructor
        protected void OldCreateMethodToType(string json)
        {
            CustomObject component = Engine.Serialiser.Convert.FromJson(json.Replace("\"_t\"", "_refType")) as CustomObject;
            if (component != null && component.CustomData.ContainsKey("SelectedItem"))
            {
                CustomObject itemData = component.CustomData["SelectedItem"] as CustomObject;
                if (itemData != null && itemData.CustomData.ContainsKey("_refType") && (string)itemData.CustomData["_refType"] == "System.Reflection.MethodBase")
                {
                    Type type = OutputParams.First().DataType;
                    if (!type.IsNotImplemented() && !type.IsDeprecated() && type?.GetInterface("IImmutable") == null && !type.IsEnum && !type.IsAbstract) // Is type constructable ?
                    {
                        // Translate from method param name to property name
                        Dictionary<string, PropertyInfo> properties = type.GetProperties().ToDictionary(x => x.Name.ToLower(), x => x);
                        foreach (ParamInfo parameter in InputParams)
                        {
                            string key = parameter.Name.ToLower();
                            if (properties.ContainsKey(key) && parameter.DataType == properties[key].PropertyType)
                                parameter.Name = properties[key].Name;
                        }

                        // Set the type item and the selection menu
                        SelectedItem = type;
                        SetInputSelectionMenu();

                        if (SelectedItem is Type)
                            m_CompiledFunc = Engine.UI.Compute.Constructor((Type)SelectedItem, InputParams);
                    }
                }
            }
        }


        /*************************************/
    }
}

