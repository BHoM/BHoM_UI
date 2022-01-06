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

using BH.Engine.Test;
using BH.oM.Reflection.Debugging;
using BH.oM.Test;
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
            List<SearchItem> items = Helpers.PossibleComponentItems()
                .Where(x => !(x.Item is string && (x.Item as string).StartsWith("TestSets")))
                .ToList();

            List<TestResult> results = items.Select(x => ComponentInstantiation(x)).ToList();

            // Generate the result message
            int errorCount = results.Where(x => x.Status == TestStatus.Error).Count();
            int warningCount = results.Where(x => x.Status == TestStatus.Warning).Count();

            // Returns a summary result 
            return new TestResult()
            {
                ID = "UIComponentInstantiation",
                Description = $"Testing instantiation of the {results.Count} available BHoM components.",
                Message = $"{errorCount} errors and {warningCount} warnings reported.",
                Status = results.MostSevereStatus(),
                Information = results.Where(x => x.Status != TestStatus.Pass).ToList<ITestInformation>(),
                UTCTime = DateTime.UtcNow,
            };
        }

        /*************************************/

        public static TestResult ComponentInstantiation(SearchItem item)
        {
            Engine.Reflection.Compute.ClearCurrentEvents();
            Caller caller = Helpers.InstantiateCaller(item);
            if (caller == null)
                return new TestResult
                {
                    Description = item.Text,
                    Status = TestStatus.Error,
                    Message = $"Error: Failed to instatiate {item.Text}.",
                    Information = Engine.Reflection.Query.CurrentEvents().Select(x => x.ToEventMessage()).ToList<ITestInformation>()
                };
            else
                return Engine.Test.Create.PassResult(item.Text);
        }

        /*************************************/
    }
}

