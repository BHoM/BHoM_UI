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
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using BH.UI.Base;

namespace BH.Test.UI
{
    public class DummyCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = null;

        public override Guid Id { get; protected set; } = new Guid("F8B38869-0396-4A8C-8AD8-ADE713FA4914");

        public override string Name { get; protected set; } = "Dummy";

        public override string Category { get; protected set; } = "Testing";

        public override string Description { get; protected set; } = "Caller that emulate teh behaviour of a regular caller but without running its selected item";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public DummyCaller() : base()
        {
        }


        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        public override object Run(List<object> inputs)
        {
            if (SelectedItem is MethodInfo)
                return Engine.Test.Compute.DummyObject(((MethodInfo)SelectedItem).ReturnType);
            else if (SelectedItem is Type)
                return Engine.Test.Compute.DummyObject(SelectedItem as Type);
            else
                return null;
        }

        /*************************************/
    }
}

