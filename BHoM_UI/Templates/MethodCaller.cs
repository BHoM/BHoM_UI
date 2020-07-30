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
using BH.Engine.Serialiser;
using BH.oM.Base;
using BH.oM.Reflection.Attributes;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BH.UI.Templates
{
    public class MethodCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public MethodBase Method
        {
            get
            {
                return SelectedItem as MethodBase;
            }
            protected set
            {
                SelectedItem = value;
            }
        }


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public MethodCaller() : base()
        {
            if (Method != null)
                SetItem(Method);
        }

        /*************************************/

        public MethodCaller(MethodBase method) : base()
        {
            SetItem(method);
        }

        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public override object Run(object[] inputs)
        {
            if (m_CompiledFunc != null)
            {
                try
                {
                    return m_CompiledFunc(inputs);
                }
                catch(InvalidCastException e)
                {
                    MethodInfo originalMethod = m_OriginalItem as MethodInfo;
                    if (Method is MethodInfo && originalMethod != null && originalMethod.IsGenericMethod)
                    {
                        // Try to update the generic method to fit the input types
                        Method = Engine.Reflection.Compute.MakeGenericFromInputs(originalMethod, inputs.Select(x => x.GetType()).ToList());
                        m_CompiledFunc = Method.ToFunc();
                        return m_CompiledFunc(inputs);
                    }
                    else
                        throw e;
                }
            }
            else if (InputParams.Count <= 0)
            {
                BH.Engine.Reflection.Compute.RecordWarning("This is a magic component. Right click on it and <Select a method>");
                return null;
            }
            else
            {
                BH.Engine.Reflection.Compute.RecordError("The component is not linked to a method.");
                return null;
            }
        }

        /*************************************/

        public override string Write()
        {
            try
            {
                MethodInfo originalMethod = m_OriginalItem as MethodInfo;

                CustomObject component = new CustomObject();
                component.CustomData["SelectedItem"] = (originalMethod == null) ? SelectedItem : originalMethod;
                component.CustomData["InputParams"] = InputParams;
                component.CustomData["OutputParams"] = OutputParams;
                return component.ToJson();
            }
            catch
            {
                BH.Engine.Reflection.Compute.RecordError($"{this} failed to serialise itself.");
                return "";
            }
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        

        /*************************************/
    }
}

