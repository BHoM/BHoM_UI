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

namespace BH.UI.Templates
{
    public abstract partial class Caller
    {
        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        protected virtual void CompileInputGetters()
        {
            if (DataAccessor == null)
                return;

            m_CompiledGetters = new List<Func<IDataAccessor, object>>();

            for (int index = 0; index < InputParams.Count; index++)
            {
                ParamInfo param = InputParams[index];
                Func<IDataAccessor, object> func = Engine.UI.Create.CreateInputAccessor(DataAccessor.GetType(), param.DataType, index);
                m_CompiledGetters.Add(func);
            }
        }

        /*************************************/

        protected virtual void CompileOutputSetters()
        {
            if (DataAccessor == null)
                return;

            m_CompiledSetters = new List<Func<IDataAccessor, object, bool>>();

            for (int index = 0; index < OutputParams.Count; index++)
            {
                ParamInfo param = OutputParams[index];
                Func<IDataAccessor, object, bool> function = Engine.UI.Create.CreateOutputAccessor(DataAccessor.GetType(), param.DataType, index);
                m_CompiledSetters.Add(function);
            }
        }

        /*************************************/
    }
}

