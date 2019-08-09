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

using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using BH.oM.UI;
using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.UI.Components
{
    public class IterateCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.BHoM_GetProperty;

        public override Guid Id { get; protected set; } = new Guid("C0BCB684-80E5-4A67-BF0E-6B8C3C917312");

        public override string Name { get; protected set; } = "Iterate";

        public override int GroupIndex { get; protected set; } = 4;

        public override string Category { get; protected set; } = "Engine";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public IterateCaller() : base()
        {
            InputParams = new List<ParamInfo>()
            {
                new ParamInfo { DataType = typeof(CustomObject), Kind = ParamKind.Input,
                    Name = "initialState", Description = "The initial value of the variables before iterating on them." },
                new ParamInfo { DataType = typeof(CustomObject), Kind = ParamKind.Input, HasDefaultValue = true, DefaultValue = null,
                    Name = "currentState", Description = "The state of the variables in the current iteration. You should plug in the values after the iteration" },
                new ParamInfo { DataType = typeof(int), Kind = ParamKind.Input, HasDefaultValue = true, DefaultValue = 0,
                    Name = "maxIterations", Description = "Number of iterations to perform" },
                new ParamInfo { DataType = typeof(bool), Kind = ParamKind.Input, HasDefaultValue = true, DefaultValue = false, 
                    Name = "run", Description = "The method starts iterating if true" },
            };
            OutputParams = new List<ParamInfo>()
            {
                new ParamInfo { DataType = typeof(CustomObject), Kind = ParamKind.Output, Name = "newState", Description = "The state of the variables after the iteration." },
                //new ParamInfo { DataType = typeof(int), Kind = ParamKind.Output, Name = "i", Description = "The current iteration" },

            };
        }

        /*************************************/
        /**** Override Method             ****/
        /*************************************/

        public override object Run(object[] inputs)
        {
            if (inputs.Length < 4)
                return null;

            CustomObject initialState = inputs[0] as CustomObject;
            CustomObject currentState = CurrentIteration == 0 ? initialState : inputs[1] as CustomObject;
            int maxIterations = inputs[2] is int ? (int)inputs[2] : 0;
            bool run = inputs[3] is bool ? (bool)inputs[3] : false;

            if (run && CurrentIteration < maxIterations)
            {
                this.CurrentIteration++;
                this.OnDataUpdated();
            }

            if (!run)
                Reset();

            return currentState;
        }

        public void Reset()
        {
            CurrentIteration = 0;
            this.OnDataUpdated();
        }

        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        public int CurrentIteration { get; set; } = 0;

    }
}
