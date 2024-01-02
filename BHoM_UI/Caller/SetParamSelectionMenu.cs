/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using BH.Engine.UI;
using BH.UI.Base.Menus;

namespace BH.UI.Base
{
    public abstract partial class Caller
    {
        /*************************************/
        /**** Protected Methods           ****/
        /*************************************/

        protected virtual void SetInputSelectionMenu()
        {
            m_InputSelector = new ParamSelectorMenu(InputParams);
            m_InputSelector.SelectionChanged += (sender, changedIndices) =>
            {
                MarkAsModified(new CallerUpdate
                {
                    Cause = CallerUpdateCause.InputSelection,
                    InputUpdates = changedIndices.Select<int, IParamUpdate>(i =>
                    {
                        if (InputParams[i].IsSelected)
                            return new ParamAdded { Index = InputParams.SelectionIndex(i), Name = InputParams[i].Name, Param = InputParams[i] };
                        else
                            return new ParamRemoved { Name = InputParams[i].Name, Param = InputParams[i] };
                    }).ToList()
                });
            };
        }

        /*************************************/

        protected virtual void SetOutputSelectionMenu()
        {
            m_OutputSelector = new ParamSelectorMenu(OutputParams);
            m_OutputSelector.SelectionChanged += (sender, changedIndices) =>
            {
                MarkAsModified(new CallerUpdate
                {
                    Cause = CallerUpdateCause.OutputSelection,
                    OutputUpdates = changedIndices.Select<int, IParamUpdate>(i =>
                    {
                        if (OutputParams[i].IsSelected)
                            return new ParamAdded { Index = OutputParams.SelectionIndex(i), Name = OutputParams[i].Name, Param = OutputParams[i] };
                        else
                            return new ParamRemoved { Name = OutputParams[i].Name, Param = OutputParams[i] };
                    }).ToList()
                });
            };
        }

        /*************************************/
    }
}





