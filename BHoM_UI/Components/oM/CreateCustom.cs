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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BH.oM.Base;
using BH.oM.UI;
using BH.Engine.Reflection;
using BH.Engine.Serialiser;
using System.Collections;
using BH.Engine.UI;

namespace BH.UI.Base.Components
{
    public class CreateCustomCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.CustomObject;

        public override Guid Id { get; protected set; } = new Guid("EB7B72E5-B4D8-4FF6-BCBD-833CDEC5D1A2");

        public override string Name { get; protected set; } = "CreateCustom";

        public override string Category { get; protected set; } = "oM";

        public override string Description { get; protected set; } = "Creates an instance of a selected type of BHoM object by manually defining its properties (default type is CustomObject)";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateCustomCaller() : base()
        {
            InputParams = new List<ParamInfo>();
            OutputParams = new List<ParamInfo>() { new ParamInfo { DataType = typeof(IObject), Kind = ParamKind.Output, Name = "object", Description = "New Object with properties set as per the inputs.", IsRequired = true } };
        }

        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        public override object Run(object[] inputs)
        {
            List<string> names = InputParams.Select(x => x.Name).ToList();
            return Engine.Base.Create.CustomObject(names, inputs.ToList());
        }

        /*************************************/

        public override bool CanAddInput()
        {
            return true;
        }

        /*************************************/

        public override bool RemoveInput(string name)
        {
            if (name == null)
                return false;

            int index = InputParams.FindIndex(x => x.Name == name);
            if (index >= 0)
            {
                InputParams.RemoveAt(index);
                m_CompiledGetters.RemoveAt(index);
            }

            return true;
        }

        /*************************************/
    }
}

