/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.CodeDom.Compiler;
using BH.Engine.Reflection;
using System.Windows.Forms;
using System.Windows.Controls;
using BH.oM.Test.UnitTests;
using BH.oM.UI;
using BH.oM.Reflection.Interface;
using BH.Engine.Base;

namespace BH.UI.Base.Components
{
    public class UnitTestCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.UnitTest;

        public override Guid Id { get; protected set; } = new Guid("dd7ad577-8e27-42e0-ad72-f70e87fc7492");

        public override string Name { get; protected set; } = "UnitTest";

        public override string Category { get; protected set; } = "UI";

        public override string Description { get; protected set; } = "Creates unit tests based on the inputs from the selected method.";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public UnitTestCaller() : base()
        {
            SetPossibleItems(Engine.UI.Query.EngineItems());
        }


        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        protected override void SetOutputs(MethodBase method)
        {
            if (method == null)
                OutputParams = new List<ParamInfo>();

            OutputParams = new List<ParamInfo>();
            OutputParams.Add(new ParamInfo
            {
                Name = "UnitTest",
                DataType = typeof(UnitTest),
                Description = "UnitTest for the method " + method.Name +".",
                Kind = ParamKind.Output,
                IsRequired = true
            });
        }

        /*************************************/

        protected override void SetComponentDetails(MethodBase method)
        {
            if (method == null)
                return;

            // Set component name
            if (method is MethodInfo)
                Name = "UT:" + method.Name;
            else if (method is ConstructorInfo)
                Name = "UT:" + method.DeclaringType.Name;
            else
                Name = "UT:UnknownMethod";

            // Set description
            Description = "Creates a UnitTest for the method " + method.Name + ".";
        }

        /*************************************/

        public override object Run(List<object> inputs)
        {
            if (inputs != null && inputs.Count > 0)
            {
                // Deepclone must be done before the properties are set to ensure immutability
                for(int x = 0; x < inputs.Count; x++)
                    inputs[x] = inputs[x].DeepClone();
            }

            object returnValue = base.Run(inputs);

            if (m_CompiledFunc != null)
                return new UnitTest() { Method = m_OriginalItem as MethodBase, Data = new List<TestData>() { new TestData(inputs, SeparateOutputs(returnValue)) } };
            else
                return null;
        }

        /*************************************/

        private List<object> SeparateOutputs(object returnObject)
        {
            List<object> returnObjects = new List<object>();
            IOutput output = returnObject as IOutput;

            if (output == null)
                returnObjects.Add(returnObject);
            else
            {
                for (int i = 0; i < output.OutputCount(); i++)
                {
                    returnObjects.Add(output.IItem(i));
                }
            }

            return returnObjects;
        }
    }
}

