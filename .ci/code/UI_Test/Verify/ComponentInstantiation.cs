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

using BH.oM.Reflection.Debugging;
using BH.oM.Test.Results;
using BH.oM.UI;
using BH.UI.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Test.UI
{
    public static partial class Verify
    {
        /*************************************/
        /**** Test Methods                ****/
        /*************************************/

        public static TestResult ComponentInstantiation()
        {
            List<SearchItem> items = Helpers.PossibleComponentItems();
            List<TestResult> results = items.Select(x => ComponentInstantiation(x)).ToList();
            List<TestResult> fails = results.Where(x => x.Status == ResultStatus.Fail).ToList();

            ResultStatus status = fails.Count == 0 ? ResultStatus.Pass : ResultStatus.Fail;
            List<Event> events = fails.SelectMany(x => x.Events).ToList();
            string description = $"Testing instatiation of the {results.Count} possible BHoM components.";

            return new TestResult(status, events, description);
        }

        /*************************************/

        public static TestResult ComponentInstantiation(SearchItem item)
        {
            Caller caller = Helpers.InstantiateCaller(item);
            if (caller == null)
                return Engine.Test.Create.FailResult($"Failed to instatiate {item.Text}.", item.Text);
            else
                return Engine.Test.Create.PassResult(item.Text);
        }

        /*************************************/
    }
}
