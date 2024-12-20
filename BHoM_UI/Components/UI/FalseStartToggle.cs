/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2025, the respective contributors. All rights reserved.
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
using BH.oM.UI;
using BH.oM.Data.Collections;
using BH.Engine.Reflection;
using BH.Engine.Data;
using BH.Engine.Serialiser;
using System.Windows.Forms;
using System.Drawing;

namespace BH.UI.Base.Components
{
    public class FalseStartToggleCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.BooleanToggleOff;

        public override Guid Id { get; protected set; } = new Guid("e1e0d75f-41b0-4268-ae5c-a55673d77851");

        public override string Category { get; protected set; } = "UI";

        public override string Name { get; protected set; } = "";

        public override string Description { get; protected set; } = "";

        public bool Value
        {
            get
            {
                return (bool)SelectedItem;
            }
            protected set
            {
                SelectedItem = value;
            }
        }

        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public FalseStartToggleCaller() : base()
        {
            SelectedItem = false;
            OutputParams = new List<ParamInfo>() { new ParamInfo { DataType = typeof(bool), Kind = ParamKind.Output, Name = Name, Description = Description, IsRequired = true } };
        }

        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        public override void SetItem(object value, bool sendNotification = true, bool updateOriginal = true)
        {
            if (updateOriginal)
                m_OriginalItem = value;
            SelectedItem = (bool)value;

            if (Value)
                Icon_24x24 = Properties.Resources.BooleanToggleOn;
            else
                Icon_24x24 = Properties.Resources.BooleanToggleOff;

            if (sendNotification)
            {
                MarkAsModified(new CallerUpdate
                {
                    Cause = CallerUpdateCause.ItemSelected,
                    ComponentUpdate = new ComponentUpdate { Name = Name, Description = Description }
                });
            }
        }

        /*************************************/

        public override object Run(List<object> inputs)
        {
            return Value;
        }

        /*************************************/
    }
}



