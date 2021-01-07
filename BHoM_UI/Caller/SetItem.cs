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

using BH.Engine.Reflection;
using BH.oM.Reflection;
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
using BH.Engine.UI;

namespace BH.UI.Base
{
    public abstract partial class Caller
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public virtual void SetItem(object item, bool sendNotification = true, bool updateOriginal = true)
        {
            if (item == null)
                return;

            if (updateOriginal)
                m_OriginalItem = item;
            SelectedItem = FromGeneric(item as dynamic);

            List<ParamInfo> oldInputs = InputParams.ToList();
            List<ParamInfo> oldOutputs = OutputParams.ToList();

            SetComponentDetails();

            SetInputs();
            SetOutputs();

            SetInputSelectionMenu();
            SetOutputSelectionMenu();

            CompileMethod();
            CompileInputGetters();
            CompileOutputSetters();

            if (sendNotification)
            {
                MarkAsModified(new CallerUpdate
                {
                    Cause = CallerUpdateCause.ItemSelected,
                    ComponentUpdate = new ComponentUpdate { Name = Name, Description = Description },
                    InputUpdates = InputParams.Changes(oldInputs).Where(x => x.Param.IsSelected).ToList(),
                    OutputUpdates = OutputParams.Changes(oldOutputs).Where(x => x.Param.IsSelected).ToList()
                });
            }
            
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        protected virtual object FromGeneric(MethodInfo method)
        {
            if (method == null)
                return null;
            else if (method.IsGenericMethodDefinition)
                return method.MakeFromGeneric();
            else
                return method;
        }

        /*************************************/

        protected virtual object FromGeneric(Type type)
        {
            if (type == null)
                return null;
            if (type.IsGenericTypeDefinition)
                return type.MakeFromGeneric();
            else
                return type;
        }

        /*************************************/

        protected virtual object FromGeneric(object item)
        {
            return item;
        }


        /*************************************/
    }
}


