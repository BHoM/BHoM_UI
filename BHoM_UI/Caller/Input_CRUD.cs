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
        /**** Public Methods              ****/
        /*************************************/

        public virtual bool AddInput(int index, string name, Type type)
        {
            if (name == null)
                return false;

            InputParams.Insert(index, Engine.UI.Create.ParamInfo(name, type));
            CompileInputGetters();
            return true;
        }

        /*************************************/

        public virtual bool RemoveInput(string name)
        {
            if (name == null)
                return false;

            bool success = InputParams.RemoveAll(p => p.Name == name) > 0;
            CompileInputGetters();
            return success;
        }

        /*************************************/

        public virtual bool UpdateInput(int index, string name, Type type = null)
        {
            if (InputParams.Count <= index)
                return AddInput(index, name, type);

            if (name != null)
                InputParams[index].Name = name;

            if (type != null)
                InputParams[index].DataType = type;

            CompileInputGetters();
            return true;
        }

        /*************************************/
    }
}

