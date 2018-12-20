/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Templates
{
    public abstract class MultiChoiceCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public List<object> Choices { get; protected set; } = new List<object>();


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public MultiChoiceCaller() : base()
        {
            InputParams = new List<ParamInfo>() { new ParamInfo { DataType = typeof(int), Kind = ParamKind.Input, Name = "index", Description = "index of the enum value" } };
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public override object Run(object[] inputs)
        {
            if (inputs.Length != 1)
                return null;

            int index = (int)inputs[0];
            if (index >= 0 && index < Choices.Count)
                return Choices[index];
            else
                return null;
        }

        /*************************************/

        public abstract List<string> GetChoiceNames();

        /*************************************/
    }
}
