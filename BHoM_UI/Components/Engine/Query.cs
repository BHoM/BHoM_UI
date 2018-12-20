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

namespace BH.UI.Components
{
    public class QueryCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Query;

        public override Guid Id { get; protected set; } = new Guid("2E60079C-3921-4C4F-8C44-7052C85FA36B");

        public override string Name { get; protected set; } = "Query";

        public override string Category { get; protected set; } = "Engine";

        public override string Description { get; protected set; } = "Query information about a BHoM object";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public QueryCaller() : base()
        {
            IEnumerable<MethodBase> methods = BH.Engine.Reflection.Query.BHoMMethodList().Where(x => x.DeclaringType.Name == "Query");
            SetPossibleItems(methods);
        }

        /*************************************/
    }
}
