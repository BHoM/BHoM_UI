/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors. All rights reserved.
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

using BH.oM.Base.Attributes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BH.Engine.UI
{
    public static partial class Compute
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description("Merges two maps between a key and a list of values into a new map, combining the lists of matching keys without duplicating values. Values are compared with the default equality comparer for T.")]
        [Input("existing", "Map to merge the other one into. Treated as empty if null.")]
        [Input("added", "Map to merge into the existing one. Its values are appended to the lists of matching keys, and its keys that are not already present are added as new entries. Ignored if null.")]
        [Output("merged", "New map containing the combined keys and values of both inputs.")]
        public static Dictionary<string, List<T>> Merge<T>(Dictionary<string, List<T>> existing, Dictionary<string, List<T>> added)
        {
            Dictionary<string, List<T>> result = existing == null
                ? new Dictionary<string, List<T>>()
                : existing.ToDictionary(x => x.Key, x => new List<T>(x.Value));

            if (added == null)
                return result;

            foreach (KeyValuePair<string, List<T>> pair in added)
            {
                if (!result.ContainsKey(pair.Key))
                    result[pair.Key] = new List<T>();

                foreach (T value in pair.Value)
                {
                    if (!result[pair.Key].Contains(value))
                        result[pair.Key].Add(value);
                }
            }

            return result;
        }

        /*************************************/

        [Description("Merges two nested maps between a key and a map of lists of values into a new map, combining the inner maps of matching keys with Merge(Dictionary<string, List<T>>, Dictionary<string, List<T>>).")]
        [Input("existing", "Map to merge the other one into. Treated as empty if null.")]
        [Input("added", "Map to merge into the existing one. Its inner maps are merged into the matching keys, and its keys that are not already present are added as new entries. Ignored if null.")]
        [Output("merged", "New map containing the combined keys and merged inner maps of both inputs.")]
        public static Dictionary<string, Dictionary<string, List<T>>> Merge<T>(Dictionary<string, Dictionary<string, List<T>>> existing, Dictionary<string, Dictionary<string, List<T>>> added)
        {
            Dictionary<string, Dictionary<string, List<T>>> result = existing == null
                ? new Dictionary<string, Dictionary<string, List<T>>>()
                : existing.ToDictionary(x => x.Key, x => Merge(x.Value, null));

            if (added == null)
                return result;

            foreach (KeyValuePair<string, Dictionary<string, List<T>>> pair in added)
            {
                Dictionary<string, List<T>> current = result.ContainsKey(pair.Key) ? result[pair.Key] : null;
                result[pair.Key] = Merge(current, pair.Value);
            }

            return result;
        }

        /*************************************/
    }
}
