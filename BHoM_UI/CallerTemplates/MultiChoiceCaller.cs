/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using BH.Engine.Reflection;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BH.Engine.Serialiser;
using System.Windows.Forms;
using BH.oM.Base;
using System.Collections;

namespace BH.UI.Base
{
    public abstract class MultiChoiceCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public List<object> Choices { get; protected set; } = new List<object>();


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public override void SetItem(object item, bool sendNotification = true, bool updateOriginal = true)
        {
            if (updateOriginal)
                m_OriginalItem = item;
            SelectedItem = item;

            SetComponentDetails();

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
            if (inputs.Count != 1)
                return null;

            int index = (int)inputs[0];
            if (index >= 0 && index < Choices.Count)
                return Choices[index];
            else
                return null;
        }

        /*************************************/

        public abstract List<string> GetChoiceNames();

        /*************************************/
    }
}



