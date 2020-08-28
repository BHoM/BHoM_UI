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
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static List<IParamUpdate> Changes(this List<ParamInfo> newList, List<ParamInfo> oldList, bool selectedOnly = true)
        {
            List<IParamUpdate> changes = new List<IParamUpdate>();
            if (newList == null || oldList == null)
                return changes;

            List<ParamInfo> oldSelection = selectedOnly ? oldList.Where(x => x.IsSelected).ToList() : oldList.ToList();
            List<ParamInfo> newSelection = selectedOnly ? newList.Where(x => x.IsSelected).ToList() : newList.ToList();

            // Look for matching param in old list
            for (int i = 0; i < newSelection.Count; i++)
            {
                ParamInfo newParam = newSelection[i];
                ParamInfo oldParam = (i < oldSelection.Count) ? oldSelection[i] : null;

                if (oldParam != null && newParam.Name == oldParam.Name)
                {
                    // If type or description is different => Tag as update. Otherwise, no action needed
                    if (newParam.DataType != oldParam.DataType || newParam.Description != oldParam.Description)
                        changes.Add(new ParamUpdated { Name = oldParam.Name, Param = newParam, OldParam = oldParam });
                }
                else
                {
                    // Finding the old Index
                    int oldIndex = oldSelection.FindIndex(x => x.Name == newParam.Name);
                    if (oldIndex < 0)
                    {
                        Type matchingType = newSelection[i].DataType;
                        List<int> matchingIndices = Enumerable.Range(0, oldSelection.Count).Where(index => oldSelection[index].DataType == matchingType).ToList();
                        if (matchingIndices.Count == 1)
                            oldIndex = matchingIndices[0];
                    }

                    // Was it moved updated, or added ?
                    if (oldIndex < 0)
                    {
                        // It was added
                        changes.Add(new ParamAdded { Index = i, Name = newParam.Name, Param = newParam });
                        oldSelection.Insert(i, newParam);
                    }
                    else if (oldIndex != i)
                    {
                        oldParam = oldSelection[oldIndex];

                        // It was moved
                        changes.Add(new ParamMoved { Index = i, Name = oldParam.Name, Param = newParam });
                        oldSelection.Move(oldIndex, i);

                        // Was it also updated ?
                        if (newParam.DataType != oldParam.DataType || newParam.Description != oldParam.Description || newParam.Name != oldParam.Name)
                            changes.Add(new ParamUpdated { Name = oldParam.Name, Param = newParam, OldParam = oldParam });
                    }
                    else
                    { 
                        // It was updated
                        changes.Add(new ParamUpdated { Name = oldSelection[oldIndex].Name, Param = newParam, OldParam = oldSelection[oldIndex] });
                    }  
                }
            }

            // Remove all old params that have not been matched
            for (int i = newSelection.Count; i < oldSelection.Count; i++)
                changes.Add(new ParamRemoved { Name = oldSelection[i].Name, Param = oldSelection[i] });

            return changes;
        }


        /*************************************/
        /**** Helper Methods              ****/
        /*************************************/

        private static void Move(this List<ParamInfo> list, int from, int to)
        {
            ParamInfo item = list[from];
            list.RemoveAt(from);

            if (list.Count >= to)
                list.Insert(to, item);
            else
            {
                while (list.Count < to)
                    list.Add(new ParamInfo());
                list.Add(item);
            }
        }

        /*************************************/
    }
}

