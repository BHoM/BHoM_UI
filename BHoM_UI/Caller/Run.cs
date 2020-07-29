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

        public virtual bool Run()
        {
            BH.Engine.Reflection.Compute.ClearCurrentEvents();

            // Get all the inputs
            object[] inputs = CollectInputs();
            if (inputs == null)
                return false;

            // Execute the method
            object result = null;
            try
            {
                result = Run(inputs);
            }
            catch (Exception e)
            {
                RecordExecutionError(e);
                return false;
            }

            // Set the output
            return PushOutputs(result);
        }


        /*************************************/
        /**** Helper Methods              ****/
        /*************************************/

        protected virtual object[] CollectInputs()
        {
            List<object> inputs = new List<object>();
            try
            {
                for (int i = 0; i < m_CompiledGetters.Count; i++)
                {
                    object input = null;
                    try
                    {
                        input = m_CompiledGetters[i](DataAccessor);
                    }
                    catch (Exception e)
                    {
                        if (m_OriginalInputTypes.Count > i && m_OriginalInputTypes[i].IsGenericType)
                        {
                            UpdateInputGenericType(i);
                            input = m_CompiledGetters[i](DataAccessor);
                        }
                        else
                        {
                            throw e;
                        }
                    }
                    inputs.Add(input);
                }
            }
            catch (Exception e)
            {
                RecordError(e, "This component failed to run properly. Inputs cannot be collected properly.\n");
                return null;
            }

            return inputs.ToArray();
        }

        /*************************************/

        protected virtual bool PushOutputs(object result)
        {
            try
            {
                for (int i = 0; i < m_CompiledSetters.Count; i++)
                {
                    // There is a problem when the output is a list of one apparently (try to explode a tree with a single branch on the first level)
                    object output = (m_CompiledSetters.Count == 1) ? result : BH.Engine.Reflection.Query.IItem(result, i);

                    try
                    {
                        m_CompiledSetters[i](DataAccessor, output);
                    }
                    catch (Exception e)
                    {
                        if (m_OriginalOutputTypes.Count > i && m_OriginalOutputTypes[i].IsGenericType)
                        {
                            m_CompiledSetters[i] = Engine.UI.Create.CreateOutputAccessor(DataAccessor.GetType(), output.GetType(), 0);
                            m_CompiledSetters[i](DataAccessor, output);
                        }
                        else
                        {
                            throw e;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                RecordError(e, "This component failed to run properly. Output data is calculated but cannot be set.\n");
                return false;
            }

            return true;
        }

        /*************************************/

        protected void UpdateInputGenericType(int index)
        {
            Type rawType = typeof(object);
            switch (m_OriginalInputTypes[index].UnderlyingType().Depth)
            {
                case 0:
                    object raw = DataAccessor.GetDataItem<object>(index);
                    rawType = raw.GetType();
                    break;
                case 1:
                    List<object> list = DataAccessor.GetDataList<object>(index);
                    if (list.Count == 0)
                        rawType = typeof(List<object>);
                    else
                        rawType = typeof(List<>).MakeGenericType(new Type[] { list[0].GetType() });
                    break;
                default:
                    List<List<object>> tree = DataAccessor.GetDataTree<object>(index);
                    if (tree.Count == 0 || tree[0].Count == 0)
                        rawType = typeof(List<List<object>>);
                    else
                    {
                        Type inType = typeof(List<>).MakeGenericType(new Type[] { tree[0][0].GetType() });
                        rawType = typeof(List<>).MakeGenericType(new Type[] { inType });
                    }
                    break;
            }

            m_CompiledGetters[index] = Engine.UI.Create.CreateInputAccessor(DataAccessor.GetType(), rawType, index);
        }

        /*************************************/
    }
}

