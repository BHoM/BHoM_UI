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

using BH.Engine.Test;
using BH.oM.Base;
using BH.oM.Base.Debugging;
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

        public static TestResult ComponentReadWriteCycle()
        {
            List<SearchItem> items = Helpers.PossibleComponentItems()
                .Where(x => !(x.Item is string && (x.Item as string).StartsWith("TestSets")))
                .ToList();

            List<TestResult> results = items.Select(x => ComponentReadWriteCycle(x)).ToList();

            // Generate the result message
            int errorCount = results.Where(x => x.Status == TestStatus.Error).Count();
            int warningCount = results.Where(x => x.Status == TestStatus.Warning).Count();

            // Returns a summary result 
            return new TestResult()
            {
                ID = "UIComponentReadWriteCycle",
                Description = $"Testing copy of the {results.Count} available BHoM components.",
                Message = $"{errorCount} errors and {warningCount} warnings reported.",
                Status = results.MostSevereStatus(),
                Information = results.Where(x => x.Status != TestStatus.Pass).ToList<ITestInformation>(),
                UTCTime = DateTime.UtcNow,
            };
        }

        /*************************************/

        public static TestResult ComponentReadWriteCycle(SearchItem item)
        {
            Engine.Base.Compute.ClearCurrentEvents();
            Caller caller = Helpers.InstantiateCaller(item);
            if (caller == null)
                return new TestResult
                {
                    Description = item.Text,
                    Status = TestStatus.Error,
                    Message = $"Error: Failed to instatiate {item.Text}.",
                    Information = Engine.Base.Query.CurrentEvents().Select(x => x.ToEventMessage()).ToList<ITestInformation>()
                };

            Caller copy = null;
            try
            {
                string json = caller.Write();
                copy = Helpers.InstantiateCaller(caller.GetType());
                copy.Read(json);
            }
            catch (Exception e)
            {
                Engine.Base.Compute.RecordError(e.Message);
                return new TestResult
                {
                    Description = item.Text,
                    Status = TestStatus.Error,
                    Message = $"Error: Failed to copy {item.Text}.",
                    Information = Engine.Base.Query.CurrentEvents().Select(x => x.ToEventMessage()).ToList<ITestInformation>()
                };
            }

            try
            {
                ComparisonConfig config = new ComparisonConfig();
                config.PropertyExceptions = new List<string> { "Icon_24x24", "BHoM_Guid", "DefaultValue" };  // DefaultValue needs to be excluded because ot can be a DateTime set to Now()
                Output<bool, List<string>, List<string>, List<string>> isEqual = Engine.Test.Query.IsEqual(caller, copy, config);
                if (isEqual.Item1)
                    return Engine.Test.Create.PassResult(item.Text);
                else
                    return new TestResult
                    {
                        Description = item.Text,
                        Status = TestStatus.Error,
                        Message = $"Error: Incorrect copy of {item.Text}.",
                        Information = Engine.Base.Query.CurrentEvents().Select(x => x.ToEventMessage()).ToList<ITestInformation>()
                    };
            }
            catch (Exception e)
            {
                Engine.Base.Compute.RecordError(e.Message);
                return new TestResult
                {
                    Description = item.Text,
                    Status = TestStatus.Error,
                    Message = $"Error: Failed to test equality of {item.Text}.",
                    Information = Engine.Base.Query.CurrentEvents().Select(x => x.ToEventMessage()).ToList<ITestInformation>()
                };
            }
        }

        /*************************************/
    }
}




