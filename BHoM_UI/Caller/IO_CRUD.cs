/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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
    public abstract partial class Caller
    {
        /*************************************/
        /**** Input Methods               ****/
        /*************************************/

        public virtual bool AddInput(int index, string name, Type type = null)
        {
            if (name == null || index < 0)
                return false;

            ParamInfo match = InputParams.Find(x => x.Name == name);
            if (match != null)
                match.IsSelected = true;
            else
            {
                ParamInfo param = Engine.UI.Create.ParamInfo(name, type);
                InputParams.Insert(index, param);
                m_CompiledGetters.Insert(index, Engine.UI.Create.InputAccessor(m_DataAccessor.GetType(), param.DataType));
            }

            return true;
        }

        /*************************************/

        public virtual bool RemoveInput(string name)
        {
            if (name == null)
                return false;

            ParamInfo match = InputParams.Find(x => x.Name == name);
            if (match != null)
                match.IsSelected = false;

            return true;
        }

        /*************************************/

        public virtual bool UpdateInput(int index, string name = null, Type type = null)
        {
            if (index < 0 || !CanUpdateInput(index, name))
                return false;

            if (InputParams.Count <= index)
                return AddInput(index, name, type);

            if (name != null)
                InputParams[index].Name = name;

            if (type != null)
            {
                if (type != InputParams[index].DataType)
                    m_CompiledGetters[index] = Engine.UI.Create.InputAccessor(m_DataAccessor.GetType(), type);

                InputParams[index].DataType = type;
            }
                
            return true;
        }

        /*************************************/

        public virtual bool SelectInputs(Dictionary<string, bool> selection)
        {
            // Only unselect a param if it is not required and is explicitely marked as unselected in the selection dictionary
            foreach (ParamInfo info in InputParams)
                info.IsSelected = info.IsRequired || !selection.ContainsKey(info.Name) || selection[info.Name];

            return true;
        }


        /*************************************/
        /**** Output Methods              ****/
        /*************************************/

        public bool RemoveOutput(string name)
        {
            if (name == null)
                return false;

            ParamInfo match = OutputParams.Find(x => x.Name == name);
            if (match != null)
                match.IsSelected = false;

            return true;
        }

        /*************************************/

        public virtual bool SelectOutputs(Dictionary<string, bool> selection)
        {
            // Only unselect a param if it is not required and is explicitely marked as unselected in the selection dictionary
            foreach (ParamInfo info in OutputParams)
                info.IsSelected = info.IsRequired || !selection.ContainsKey(info.Name) || selection[info.Name];

            return true;
        }


        /*************************************/
        /**** Output Methods              ****/
        /*************************************/

        public virtual void UpdateParams()
        {

        }


        /*************************************/
        /**** Authorisation Methods       ****/
        /*************************************/

        public virtual bool CanAddInput()
        {
            return false;
        }

        /*************************************/

        public virtual bool CanAddOutput()
        {
            return false;
        }

        /*************************************/

        public virtual bool CanRemoveInput(string name)
        {
            ParamInfo match = InputParams.Find(x => x.Name == name);
            return match != null && !match.IsRequired;
        }

        /*************************************/

        public virtual bool CanRemoveOutput(string name)
        {
            if (OutputParams.Count < 2)
                return false;
            else
            {
                ParamInfo match = OutputParams.Find(x => x.Name == name);
                return match != null && !match.IsRequired;
            }
        }

        /*************************************/

        public virtual bool CanUpdateInput(int index, string name)
        {
            return false;
        }

        /*************************************/
    }
}




