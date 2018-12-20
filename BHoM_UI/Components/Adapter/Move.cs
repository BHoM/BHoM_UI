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

using BH.Adapter;
using BH.oM.Base;
using BH.oM.DataManipulation.Queries;
using BH.oM.Reflection.Attributes;
using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Components
{
    public class MoveCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Move;

        public override Guid Id { get; protected set; } = new Guid("6D2C7F5B-7F64-47C8-AB69-424E5301582F");

        public override string Category { get; protected set; } = "Adapter";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public MoveCaller() : base(typeof(MoveCaller).GetMethod("Move")) { }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        [Description("Copy objects from a source adapter to a target adapter")]
        [Input("source", "Adapter the data is copied from")]
        [Input("target", "Adapter the data is copied to")]
        [Input("query", "Filter on the objects to pull (default: get all)")]
        [Input("config", "Move config")]
        [Input("active", "Execute the move")]
        [Output("Confirms the success of the operation")]
        public static bool Move(BHoMAdapter source, BHoMAdapter target, IQuery query = null, Dictionary<string, object> config = null, bool active = false)
        {
            if (query == null)
                query = new FilterQuery();

            if (active)
                return source.PullTo(target, query, config);
            else
                return false;
        }

        /*************************************/
    }
}
