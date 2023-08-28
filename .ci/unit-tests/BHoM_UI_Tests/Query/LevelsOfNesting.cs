/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

using System;
using System.Collections.Generic;
using System.Text;
using BH.oM.Test.NUnit;
using NUnit.Framework;
using FluentAssertions;

namespace BH.Tests.UI.Query
{
    public class LevelsOfNesting : NUnitTest
    {
        [Test]
        public static void TestLevelsOfNesting()
        {
            string s = "";

            int levels = BH.Engine.UI.Query.LevelsOfNesting(s);
            levels.Should().Be(0);

            s = "Hello world";
            levels = BH.Engine.UI.Query.LevelsOfNesting(s);
            levels.Should().Be(0);

            int h = 1;
            levels = BH.Engine.UI.Query.LevelsOfNesting(h);
            levels.Should().Be(0);

            List<string> stringList = new List<string>();
            stringList.Add("Hello world");
            levels = BH.Engine.UI.Query.LevelsOfNesting(stringList);
            levels.Should().Be(1);

            List<List<string>> nestedStringList = new List<List<string>>();
            nestedStringList.Add(stringList);
            levels = BH.Engine.UI.Query.LevelsOfNesting(nestedStringList);
            levels.Should().Be(2);

            List<List<List<string>>> tripleNested = new List<List<List<string>>>();
            tripleNested.Add(nestedStringList);
            levels = BH.Engine.UI.Query.LevelsOfNesting(tripleNested);
            levels.Should().Be(3);

            string[] arrayCheck = new string[3];
            arrayCheck[0] = "Hello";
            levels = BH.Engine.UI.Query.LevelsOfNesting(arrayCheck);
            levels.Should().Be(1);

            char[] charCheck = new char[3];
            charCheck[0] = 'H';
            levels = BH.Engine.UI.Query.LevelsOfNesting(charCheck);
            levels.Should().Be(0);

            string[][] nestedArray = new string[3][];
            nestedArray[0] = new string[3];
            nestedArray[0][2] = "Hello world";
            levels = BH.Engine.UI.Query.LevelsOfNesting(nestedArray);
            levels.Should().Be(1);
        }
    }
}
