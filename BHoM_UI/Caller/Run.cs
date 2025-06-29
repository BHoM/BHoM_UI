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

using BH.Engine.Reflection;
using BH.Engine.Base;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BH.Engine.Serialiser;
using System.Windows.Forms;
using System.Collections;
using BH.oM.Base.Debugging;

namespace BH.UI.Base
{
    public abstract partial class Caller
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public virtual bool Run()
        {
            BH.Engine.Base.Compute.ClearCurrentEvents();

            // Get all the inputs
            List<object> inputs = CollectInputs();
            if (inputs == null)
                return false;

            // Execute the method
            object result = m_CachedResult;
            if (m_CachedResult == null || ShouldCalculateNewResult(inputs, ref result))
            {
                try
                {
                    result = Run(inputs);
                    m_CachedResult = result;
                }
                catch (Exception e)
                {
                    Engine.UI.Compute.RecordExecutionError(e);
                    return false;
                }
            }
                
            // Set the output
            return PushOutputs(result);
        }

        /*************************************/

        public virtual object Run(List<object> inputs)
        {
            if (m_CompiledFunc != null)
            {
                try
                {
                    var result = m_CompiledFunc(inputs.ToArray());
                    ResetLogSuppression();
                    return result;
                }
                catch (InvalidCastException e)
                {
                    MethodInfo originalMethod = m_OriginalItem as MethodInfo;
                    if (originalMethod != null && originalMethod.IsGenericMethod)
                    {
                        // Try to update the generic method to fit the input types
                        MethodInfo method = Engine.Base.Compute.MakeGenericFromInputs(originalMethod, inputs.Select(x => x?.GetType()).ToList());
                        m_CompiledFunc = method.ToFunc();
                        var result = m_CompiledFunc(inputs.ToArray());
                        ResetLogSuppression();
                        return result;
                    }
                    else
                    {
                        ResetLogSuppression();
                        throw e;
                    }
                }
            }
            else if (InputParams.Count <= 0)
            {
                BH.Engine.Base.Compute.RecordWarning("This is a magic component. Right click on it and <Select a method>");
                return null;
            }
            else
            {
                BH.Engine.Base.Compute.RecordError("The component is not linked to a method.");
                return null;
            }
        }


        /*************************************/
        /**** Helper Methods              ****/
        /*************************************/

        protected virtual List<object> CollectInputs()
        {
            List<object> inputs = new List<object>();
            try
            {
                int index = 0;
                for (int i = 0; i < m_CompiledGetters.Count; i++)
                {
                    object input = null;
                    if (InputParams[i].IsSelected)
                    {
                        int preGetterEventCount = Engine.Base.Query.CurrentEvents().Where(x => x.Type == EventType.Error).Count();

                        try
                        {
                            input = m_CompiledGetters[i](m_DataAccessor, index);

                            if (input == null || input.ToString() == "" || (input is IEnumerable && ((IEnumerable)input).Cast<object>().All(x => x == null)))
                            {
                                string warning = InputParams[i].DefaultValueWarning;
                                if (!string.IsNullOrEmpty(warning))
                                    Engine.Base.Compute.RecordNote(warning);

                                // Temporary solution to handle error casting events that didn't throw an exception. 
                                // Long term, we need Events of different types to better identify them instead of relying on message content.
                                int newErrorCount = Engine.Base.Query.CurrentEvents().Skip(preGetterEventCount).Where(x => x.Type == EventType.Error).Where(x => x.Message.StartsWith("Failed to cast")).Count();
                                if (newErrorCount > 0)
                                    throw new Exception();
                            }
                        }
                        catch (Exception e)
                        {
                            Type originalInputType = InputType(m_OriginalItem, i);
                            MethodInfo originalMethod = m_OriginalItem as MethodInfo;
                            if (originalInputType != null && originalInputType.IsGenericType && originalMethod != null && originalMethod.IsGenericMethod)
                            {
                                Engine.Base.Compute.RemoveEvents(Engine.Base.Query.CurrentEvents().Skip(preGetterEventCount).ToList());
                                UpdateInputGenericType(i);
                                input = m_CompiledGetters[i](m_DataAccessor, index);
                            }
                            else
                                throw e;
                        }
                        index++;
                    } 
                    else
                    {
                        string warning = InputParams[i].DefaultValueWarning;
                        if (!string.IsNullOrEmpty(warning))
                            Engine.Base.Compute.RecordNote(warning);
                        input = InputParams[i].DefaultValue;
                    }
                        
                    inputs.Add(input);
                }
            }
            catch (Exception e)
            {
                if (m_IsMissingParamInfo)
                    Engine.UI.Compute.RecordError(e, "This component failed to run properly.\n"
                        + "It looks like this is a component created before version 2.3 and some data is missing to restore it properly.\n"
                        + "Please, create a new component for " + SelectedItem.IToText(true) + "\n");
                else 
                    Engine.UI.Compute.RecordError(e, "This component failed to run properly. Inputs cannot be collected properly.\n");
                return null;
            }

            return inputs.ToList();
        }

        /*************************************/

        protected virtual bool PushOutputs(object result)
        {
            try
            {
                int index = 0;
                for (int i = 0; i < m_CompiledSetters.Count; i++)
                {
                    // There is a problem when the output is a list of one apparently (try to explode a tree with a single branch on the first level)
                    object output = (m_CompiledSetters.Count == 1) ? result : BH.Engine.Base.Query.IItem(result, i);

                    try
                    {
                        if (OutputParams[i].IsSelected)
                        {
                            m_CompiledSetters[i](m_DataAccessor, output, index);
                            index++;
                        }
                    }
                    catch (Exception e)
                    {
                        Type originalOutputType = OutputType(m_OriginalItem, i);
                        if (originalOutputType != null && originalOutputType.IsGenericType)
                        {
                            m_CompiledSetters[i] = Engine.UI.Compute.OutputAccessor(m_DataAccessor.GetType(), output.GetType(), QuantitiesAsDouble);
                            m_CompiledSetters[i](m_DataAccessor, output, 0);
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
                Engine.UI.Compute.RecordError(e, "This component failed to run properly. Output data is calculated but cannot be set.\n");
                return false;
            }

            return true;
        }

        /*************************************/

        protected void UpdateInputGenericType(int index)
        {
            Type rawType = typeof(object);
            switch (InputType(m_OriginalItem, index).UnderlyingType().Depth)
            {
                case 0:
                    object raw = m_DataAccessor.GetDataItem<object>(index);
                    rawType = raw.GetType();
                    break;
                case 1:
                    List<object> list = m_DataAccessor.GetDataList<object>(index);
                    if (list.Count == 0)
                        rawType = typeof(List<object>);
                    else
                        rawType = typeof(List<>).MakeGenericType(new Type[] { list[0].GetType() });
                    break;
                default:
                    List<List<object>> tree = m_DataAccessor.GetDataTree<object>(index);
                    if (tree.Count == 0 || tree[0].Count == 0)
                        rawType = typeof(List<List<object>>);
                    else
                    {
                        Type inType = typeof(List<>).MakeGenericType(new Type[] { tree[0][0].GetType() });
                        rawType = typeof(List<>).MakeGenericType(new Type[] { inType });
                    }
                    break;
            }

            m_CompiledGetters[index] = Engine.UI.Compute.InputAccessor(m_DataAccessor.GetType(), rawType);
        }

        /*************************************/

        protected Type InputType(object item, int index)
        {
            if (item is MethodBase)
            {
                ParameterInfo[] parameters = ((MethodBase)item).GetParameters();
                if (parameters.Count() > index)
                    return parameters[index].ParameterType;
            }
            else if (item is Type)
            {
                PropertyInfo[] properties = ((Type)item).GetProperties();
                if (properties.Count() > index)
                    return properties[index].PropertyType;
            }

            return null;
        }

        /*************************************/

        protected Type OutputType(object item, int index)
        {
            if (item is MethodBase)
            {
                MethodBase method = item as MethodBase;
                if (method.IsMultipleOutputs())
                {
                    Type[] types = method.OutputType().GenericTypeArguments;
                    return (types.Count() > index) ? null : types[index];
                }
                else
                    return method.OutputType();
            }
            else if (item is Type)
                return item as Type;
            else
                return null;
        }

        /*************************************/

        protected virtual bool ShouldCalculateNewResult(List<object> inputs, ref object result)
        {
            return true;
        }

        /*************************************/

        protected void ResetLogSuppression()
        {
            BH.Engine.Base.Compute.StopSuppressRecordingEvents(); //Ensure suppression is reset in case the calling method forgot to do it
            BH.Engine.Base.Compute.ThrowErrorsAsExceptions(false); //Reset throwing errors to whatever the user may have set it to prior to running this method
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        protected object m_CachedResult = null;

        /*************************************/
    }
}






