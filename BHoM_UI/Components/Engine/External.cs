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

using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using BH.Adapter;

namespace BH.UI.Components
{
    public class ExternalCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.External;

        public override Guid Id { get; protected set; } = new Guid("4DEC2B78-4A83-49A2-BD7D-A03DD5CAE43E");

        public override string Name { get; protected set; } = "External";

        public override string Category { get; protected set; } = "Engine";

        public override string Description { get; protected set; } = "Exposes methods from external libraries without porting them to the BHoM common language";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public ExternalCaller() : base()
        {
            SetPossibleItems(Engine.UI.Query.ExternalItems());
        }

        /*************************************/
    }
}
