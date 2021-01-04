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
using BH.Engine.Reflection;
using System.Linq.Expressions;
using BH.oM.UI;
using BH.Engine.UI;
using System.Windows.Forms;
using BH.oM.Base;
using System.Collections;

namespace BH.UI.Base.Components
{
    public class CreateObjectCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.CreateBHoM;

        public override Guid Id { get; protected set; } = new Guid("76221701-C5E7-4A93-8A2B-D34E77ED9CC1");

        public override string Name { get; protected set; } = "CreateObject";

        public override string Category { get; protected set; } = "oM";

        public override string Description { get; protected set; } = "Creates an instance of a selected type of BHoM object";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateObjectCaller() : base()
        {
            List<MemberInfo> possibleItems = new List<MemberInfo>();
            possibleItems.AddRange(Engine.UI.Query.ConstructableTypeItems());
            possibleItems.AddRange(Engine.UI.Query.CreateItems());

            SetPossibleItems(possibleItems);
        }

        /*************************************/
    }

}


