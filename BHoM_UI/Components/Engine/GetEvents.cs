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
using System.ComponentModel;
using BH.oM.Reflection.Attributes;
using BH.oM.UI;
using BH.Adapter;
using BH.oM.Reflection;
using BH.oM.Reflection.Debugging;

namespace BH.UI.Base.Components
{
    public class GetEventsCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.GetEvents;

        public override Guid Id { get; protected set; } = new Guid("F521EF89-3204-412A-9822-5D8F5A88E56F");

        public override int GroupIndex { get; protected set; } = 4;

        public override string Description { get; protected set; } = "Get all events (errors, warnings, and notes) occuring on BHoM components.";

        public override string Category { get; protected set; } = "Engine";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public GetEventsCaller() : base(typeof(GetEventsCaller).GetMethod("GetEvents")) { }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        [Description("Get all events (errors, warnings, and notes) occuring on BHoM components.")]
        [Input("since", "Only events after this time will be provided in the outputs. To get all events, leave this empty.")]
        [MultiOutput(0, "errors", "All recorded errors.")]
        [MultiOutput(1, "warnings", "All recorded warnings.")]
        [MultiOutput(2, "notes", "All recorded notes.")]
        public static Output<List<Event>, List<Event>, List<Event>> GetEvents(DateTime? since = null)
        {
            List<Event> events = Engine.Reflection.Query.AllEvents().ToList();
            
            if (since != null)
                events = events.Where(x => x.Time > since).ToList();

            return Engine.Reflection.Create.Output
            (
                events.Where(x => x.Type == EventType.Error).ToList(),
                events.Where(x => x.Type == EventType.Warning).ToList(),
                events.Where(x => x.Type == EventType.Note).ToList()
            );
        }

        /*************************************/
    }
}
