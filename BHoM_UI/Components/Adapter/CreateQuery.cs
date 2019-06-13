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
using BH.Engine.Reflection;

namespace BH.UI.Components
{
    public class CreateQueryCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.QueryAdapter;

        public override Guid Id { get; protected set; } = new Guid("A4C4D4BA-8FB9-4CE5-802E-46A39B89FE5E");

        public override string Name { get; protected set; } = "CreateQuery";

        public override string Category { get; protected set; } = "Adapter";

        public override string Description { get; protected set; } = "Creates an instance of a selected type of adapter query";

        public override int GroupIndex { get; protected set; } = 2;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateQueryCaller() : base()
        {
            Type queryType = typeof(BH.oM.DataManipulation.Queries.IQuery);
            IEnumerable<MethodBase> methods  = BH.Engine.Reflection.Query.BHoMMethodList()
                .Where(x => x.DeclaringType.Name == "Create"
                && queryType.IsAssignableFrom(x.ReturnType)
                && !x.IsDeprecated())
                .OrderBy(x => x.Name);
            SetPossibleItems(methods);
        }

        /*************************************/
    }
}
