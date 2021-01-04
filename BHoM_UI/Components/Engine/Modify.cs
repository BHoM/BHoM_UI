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
using BH.Engine.Base;

namespace BH.UI.Base.Components
{
    public class ModifyCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Modify;

        public override Guid Id { get; protected set; } = new Guid("2B79756E-C774-470B-8F62-0F20C4AE2DC8");

        public override string Name { get; protected set; } = "Modify";

        public override string Category { get; protected set; } = "Engine";

        public override string Description { get; protected set; } = "Modify a BHoM object";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public ModifyCaller() : base()
        {
            SetPossibleItems(Engine.UI.Query.ModifyItems());
        }


        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        public override object Run(List<object> inputs)
        {
            if (inputs != null && inputs.Count >= 1 && inputs[0] != null)
            {
                // Deepclone must be done before the properties are set to ensure immutability
                // TODO: DeepClone should ignore fragments and CustomData
                inputs[0] = inputs[0].DeepClone();
            }
            return base.Run(inputs);
        }

        /*************************************/
    }
}


