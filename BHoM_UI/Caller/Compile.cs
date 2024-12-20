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

using BH.Engine.Base;
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
    public abstract partial class Caller
    {
        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        protected virtual void CompileInputGetters()
        {
            if (m_DataAccessor == null)
                return;

            Type accessorType = m_DataAccessor.GetType();
            m_CompiledGetters = new List<Func<IDataAccessor, int, object>>();

            for (int index = 0; index < InputParams.Count; index++)
            {
                ParamInfo param = InputParams[index];
                Func<IDataAccessor, int, object> func = Engine.UI.Create.InputAccessor(accessorType, param.DataType);
                m_CompiledGetters.Add(func);
            }
        }

        /*************************************/

        protected virtual void CompileOutputSetters()
        {
            if (m_DataAccessor == null)
                return;

            Type accessorType = m_DataAccessor.GetType();
            m_CompiledSetters = new List<Func<IDataAccessor, object, int, bool>>();

            for (int index = 0; index < OutputParams.Count; index++)
            {
                ParamInfo param = OutputParams[index];
                Func<IDataAccessor, object, int, bool> function = Engine.UI.Create.OutputAccessor(accessorType, param.DataType);
                m_CompiledSetters.Add(function);
            }
        }

        /*************************************/

        protected virtual void CompileMethod()
        {
            if (SelectedItem is MethodBase)
                m_CompiledFunc = ((MethodBase)SelectedItem).ToFunc();
            else if (SelectedItem is Type)
                m_CompiledFunc = Engine.UI.Compute.Constructor(SelectedItem as Type, InputParams);
            else
                m_CompiledFunc = null;
        }

        /*************************************/
    }
}






