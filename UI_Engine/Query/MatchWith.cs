/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

        public static Dictionary<ParamInfo, ParamInfo> MatchWith(this List<ParamInfo> newList, List<ParamInfo> oldList, out List<ParamInfo> added, out List<ParamInfo> removed)
        {
            added = newList.ToList();
            removed = oldList.ToList();
            Dictionary<ParamInfo, ParamInfo> matches = new Dictionary<ParamInfo, ParamInfo>();

            // Look for matches if neither of the list is empty
            if (newList.Count > 0 && oldList.Count > 0)
            {
                // First, get all the pairs that have matching names
                foreach (ParamInfo newParam in added.ToList())
                {
                    ParamInfo oldParam = removed.Find(x => newParam.IsMatchingName(x.Name));
                    if (oldParam != null)
                    {
                        matches[newParam] = oldParam;
                        added.Remove(newParam);
                        removed.Remove(oldParam);
                    }
                }

                // Then get the ones with matching types if only one candidate each way
                foreach (ParamInfo newParam in added.ToList())
                {
                    // Make sure there is only one old with matching type
                    List<ParamInfo> oldCandidates = removed.FindAll(x => x.DataType == newParam.DataType);
                    if (oldCandidates.Count != 1)
                        continue;

                    // Make sure there is only one new with matching type
                    ParamInfo oldParam = oldCandidates[0];
                    List<ParamInfo> newCandidates = added.FindAll(x => x.DataType == oldParam.DataType);
                    if (newCandidates.Count != 1)
                        continue;

                    // With those two conditions met, we have a match
                    matches[newParam] = oldParam;
                    added.Remove(newParam);
                    removed.Remove(oldParam);
                }
            }

            return matches;
        }

        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private static bool IsMatchingName(this ParamInfo param, string name)
        {
            if (param.Name == name)
                return true;

            PreviousNamesFragment fragment = param.Fragments.OfType<PreviousNamesFragment>().FirstOrDefault();
            return fragment != null && fragment.OldNames.Contains(name);
        }

        /*************************************/
    }
}



