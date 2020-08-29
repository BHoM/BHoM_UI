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

            // Look for matches between the two lists
            List<ParamInfo> added, removed;
            Dictionary<ParamInfo, ParamInfo > matches = newSelection.MatchWith(oldSelection, out added, out removed);

            // First remove all the old params that are not matching with any new one
            foreach (ParamInfo param in removed)
            {
                changes.Add(new ParamRemoved { Name = param.Name, Param = param });
                oldSelection.Remove(param);
            }
                
            // Then create the other changes. We will need to update old list as we go to make sure we detect moved params correctly
            for (int i = 0; i < newSelection.Count; i++)
            {
                ParamInfo newParam = newSelection[i];
                if (!matches.ContainsKey(newParam)) 
                {
                    // New parameter added
                    changes.Add(new ParamAdded { Index = i, Name = newParam.Name, Param = newParam });
                    oldSelection.Insert(i, newParam);
                }
                else
                {
                    // check if we need to update old param
                    ParamInfo oldParam = matches[newParam];
                    if (newParam.DataType != oldParam.DataType || newParam.Description != oldParam.Description || newParam.Name != oldParam.Name)
                        changes.Add(new ParamUpdated { Name = oldParam.Name, Param = newParam, OldParam = oldParam });

                    // Check if we need to move old param (can be on top of the update)
                    int oldIndex = oldSelection.IndexOf(oldParam);
                    if (oldIndex != i)
                    {
                        changes.Add(new ParamMoved { Index = i, Name = oldParam.Name, Param = newParam });
                        oldSelection.Move(oldIndex, i);
                    }
                }   
            }

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

