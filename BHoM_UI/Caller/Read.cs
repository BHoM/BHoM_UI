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
                object backendElement;
                if (component.CustomData.TryGetValue("SelectedItem", out backendElement))
                    SetItem(backendElement);

                // We also overwrite the InputParams and OutputParams, since we could have made some changes to them - e.g. ListInput
                // Also, if SelectedItem is null, the component will still have its input and outputs
                object inputParamsRecord;
                List<ParamInfo> inputParams = new List<ParamInfo>();
                if (component.CustomData.TryGetValue("InputParams", out inputParamsRecord))
                    inputParams = (inputParamsRecord as IEnumerable).OfType<ParamInfo>().ToList();

                object outputParamsRecord;
                List<ParamInfo> outputParams = new List<ParamInfo>();
                if (component.CustomData.TryGetValue("OutputParams", out outputParamsRecord))
                    outputParams = (outputParamsRecord as IEnumerable).OfType<ParamInfo>().ToList();


                if (backendElement == null)
                {
                    // If the method is not found, we need to make sure that the component keeps its old inputs and outputs
                    InputParams = inputParams;
                    CompileInputGetters();

                    OutputParams = outputParams;
                    CompileOutputSetters();
                }
                else
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
                        Modified?.Invoke(this, new CallerUpdate
                        {
                            Cause = CallerUpdateCause.UpgradedVersion
                        });
                    }
                }

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
    }
}

