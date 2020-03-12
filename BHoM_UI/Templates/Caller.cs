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
    public abstract class Caller
    {
        /*************************************/
        /**** Events                      ****/
        /*************************************/

        public event EventHandler<object> ItemSelected;

        public event EventHandler SolutionExpired;


        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public virtual System.Drawing.Bitmap Icon_24x24 { get; protected set; }

        public virtual Guid Id { get; protected set; }

        public virtual string Name { get; protected set; } = "Undefined";

        public virtual string Category { get; protected set; } = "Undefined";

        public virtual string Description { get; protected set; } = "";

        public virtual int GroupIndex { get; protected set; } = 1;

        public virtual IItemSelector Selector { get; protected set; } = null;

        public DataAccessor DataAccessor { get; protected set; } = null;

        public List<ParamInfo> InputParams { get; protected set; } = new List<ParamInfo>();

        public List<ParamInfo> OutputParams { get; protected set; } = new List<ParamInfo>();

        public object SelectedItem { get; set; } = null;

        public bool WasUpgraded { get; protected set; } = false;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public Caller()
        {
            Engine.UI.Compute.LoadAssemblies();
        }


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
                string message = "This component failed to run properly.\n- Error: "; 

                if (e.InnerException != null)
                    message += e.InnerException.Message;
                else
                    message += e.Message;

                List<string> stack = e.StackTrace.Split(new char[] { '\n' })
                    .Where(x => x.Contains(" BH."))
                    .Take(2)
                    .ToList();

                if (stack.Count > 0)
                    message += "\n- Occured in " + stack[0].Trim().Substring(2);
                if (stack.Count > 1)
                    message += "\n     called from" + stack[1].Trim().Substring(2);

                message += "\n- Are you sure you have the correct type of inputs? Check their description for more details.";

                Engine.Reflection.Compute.RecordError(message);

                return false;
            }

            // Set the output
            return PushOutputs(result);
        }

        /*************************************/

        public abstract object Run(object[] inputs);

        /*************************************/

        public virtual bool SetItem(object item)
        {
            SelectedItem = item;
            return true;
        }

        /*************************************/

        public virtual void SetDataAccessor(DataAccessor accessor)
        {
            DataAccessor = accessor;
            CompileInputGetters();
            CompileOutputSetters();
        }

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

        public virtual void AddToMenu(ToolStripDropDown menu)
        {
            if (Selector != null && SelectedItem == null)
                Selector.AddToMenu(menu);
        }

        /*************************************/

        public virtual void AddToMenu(System.Windows.Controls.ContextMenu menu)
        {
            if (Selector != null && SelectedItem == null)
                Selector.AddToMenu(menu);
        }

        /*************************************/

        public virtual void AddToMenu(object menu)
        {
            if (Selector != null && SelectedItem == null)
                Selector.AddToMenu(menu);
        }

        /*************************************/

        public virtual string Write()
        {
            try
            {
                CustomObject component = new CustomObject();
                component.CustomData["SelectedItem"] = SelectedItem;
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

        public virtual bool Read(string json)
        {
            if (json == "")
                return true;

            try
            {
                object obj = BH.Engine.Serialiser.Convert.FromJson(json);

                CustomObject component = obj as CustomObject; // Old component, serialised only with the SelectedItem as object
                if (component == null)
                {
                    //If component failed to de-serialise, try upgrade version.
                    if (obj == null)
                    {
                        json = Engine.Versioning.Convert.ToNewVersion(json);
                        obj = BH.Engine.Serialiser.Convert.FromJson(json);

                        //If component was sucessfully upgraded, show the message to inform the user.
                        //This will happen if: Was not serialised as CustomObject (current serialisation method), failed to de-serialised orginially and was able to be upgraded.
                        if (obj != null && !m_UpgradeMessageShown)
                        {
                            MessageBox.Show("BHoM 2.3 or earlier components found in this script." + Environment.NewLine +
                                            "Automatic upgrade of components to 3.0 has been applied. Some wires may have been disconnected." + Environment.NewLine + Environment.NewLine +
                                            "To preserve wire connectivity as well you could try upgrading to 2.4 first:" + Environment.NewLine +
                                            "1. Close the script without saving" + Environment.NewLine +
                                            "2. Downgrade to BHoM 2.4.beta" + Environment.NewLine +
                                            "3. Open and save the script" + Environment.NewLine +
                                            "4. Upgrade back to current version of BHoM and open the script again." + Environment.NewLine + Environment.NewLine +
                                            "Note that this message will only be shown once per session of the UI.", "BHoM auto versioning");
                            m_UpgradeMessageShown = true;
                        }
                    }

                    SetItem(obj);

                    if (SelectedItem != null)
                        ItemSelected?.Invoke(this, SelectedItem);
                    return true;
                }

                // New serialisation, we stored a CustomObject with SelectedItem, InputParams and OutputParams
                object backendElement;
                if (component.CustomData.TryGetValue("SelectedItem", out backendElement))
                {
                    if (backendElement == null)
                    {
                        MongoDB.Bson.BsonDocument bson = Engine.Serialiser.Convert.ToBson(json);
                        MongoDB.Bson.BsonValue item = bson["SelectedItem"];
                        if (!item.IsBsonNull)
                        {
                            MongoDB.Bson.BsonDocument newVersion = Engine.Versioning.Convert.ToNewVersion(item.AsBsonDocument);
                            if (newVersion != null)
                                backendElement = Engine.Serialiser.Convert.FromBson(newVersion);
                        }
                        SetItem(backendElement);
                        WasUpgraded = backendElement != null;
                    }
                    SetItem(backendElement);
                }

                // We also overwrite the InputParams and OutputParams, since we could have made some changes to them - e.g. ListInput
                // Also, if SelectedItem is null, the component will still have its input and outputs
                object inputParamsRecord;
                List<ParamInfo> inputParams = new List<ParamInfo>();
                if (component.CustomData.TryGetValue("InputParams", out inputParamsRecord))
                    inputParams = (inputParamsRecord as IEnumerable).OfType<ParamInfo>().ToList();
                
                object outputParamsRecord;
                List<ParamInfo> outputParams = new List<ParamInfo>();
                if (component.CustomData.TryGetValue("OutputParams", out outputParamsRecord))
                    outputParams = (outputParamsRecord as IEnumerable).OfType<ParamInfo>().ToList();


                if (WasUpgraded)
                {
                    FindOldIndex(InputParams, inputParams);
                    FindOldIndex(OutputParams, outputParams);
                    ItemSelected?.Invoke(this, SelectedItem);
                }
                else
                {
                    InputParams = inputParams;
                    CompileInputGetters();

                    OutputParams = outputParams;
                    CompileOutputSetters();
                }
                
                return true;
            }
            catch
            {
                BH.Engine.Reflection.Compute.RecordError($"{this} failed to deserialise itself.");
                return false;
            }
        }


        /*************************************/
        /**** Private Methods             ****/
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
                        if (m_OriginalTypes.Count > i && m_OriginalTypes[i].IsGenericType)
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
                inputs = null;
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
                    catch (InvalidCastException e)
                    {
                        if (output != null && output.GetType().IsGenericType)
                        {
                            m_CompiledSetters[i] = CreateOutputAccessor(output.GetType(), 0);
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

        protected virtual void CompileInputGetters()
        {
            if (DataAccessor == null)
                return;

            m_CompiledGetters = new List<Func<DataAccessor, object>>();

            for (int index = 0; index < InputParams.Count; index++)
            {
                ParamInfo param = InputParams[index];
                Func<DataAccessor, object> func = CreateInputAccessor(param.DataType, index);
                m_CompiledGetters.Add(func);
            }
        }

        /*************************************/

        protected virtual void CompileOutputSetters()
        {
            if (DataAccessor == null)
                return;

            m_CompiledSetters = new List<Func<DataAccessor, object, bool>>();

            for (int index = 0; index < OutputParams.Count; index++)
            {
                ParamInfo param = OutputParams[index];
                Func<DataAccessor, object, bool> function = CreateOutputAccessor(param.DataType, index);
                m_CompiledSetters.Add(function);
            }
        }

        /*******************************************/

        protected virtual Func<DataAccessor, object> CreateInputAccessor(Type dataType, int index)
        {
            UnderlyingType subType = dataType.UnderlyingType();
            string methodName = (subType.Depth == 0) ? "GetDataItem" : (subType.Depth == 1) ? "GetDataList" : "GetDataTree";

            MethodInfo method = DataAccessor.GetType().GetMethod(methodName).MakeGenericMethod(subType.Type);

            ParameterExpression lambdaInput1 = Expression.Parameter(typeof(DataAccessor), "accessor");
            ParameterExpression[] lambdaInputs = new ParameterExpression[] { lambdaInput1 };

            Expression[] methodInputs = new Expression[] { Expression.Constant(index) };
            MethodCallExpression methodExpression = Expression.Call(Expression.Convert(lambdaInput1, DataAccessor.GetType()), method, methodInputs);
            Func<DataAccessor, object> lambda = Expression.Lambda<Func<DataAccessor, object>>(Expression.Convert(methodExpression, typeof(object)), lambdaInputs).Compile();

            if (!dataType.IsArray)
                return lambda;
            
            // If dataType is an array type, the underlying method asks for an array type
            // Thus, we add a new node to the syntax tree that casts the List to an Array
            MethodInfo castMethod = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(subType.Type);
            ParameterExpression lambdaResult = Expression.Parameter(typeof(object), "lambdaResult");
            MethodCallExpression castExpression = Expression.Call(null, castMethod, Expression.Convert(lambdaResult, typeof(IEnumerable<>).MakeGenericType(subType.Type)));
            Func<object, object> castDelegate = Expression.Lambda<Func<object, object>>(castExpression, lambdaResult).Compile();

            return (accessor) => { return castDelegate(lambda(accessor)); };
        }

        /*******************************************/

        protected virtual Func<DataAccessor, object, bool> CreateOutputAccessor(Type dataType, int index)
        {
            UnderlyingType subType = dataType.UnderlyingType();
            string methodName = (subType.Depth == 0) ? "SetDataItem" : (subType.Depth == 1) ? "SetDataList" : "SetDataTree";
            MethodInfo method = DataAccessor.GetType().GetMethod(methodName).MakeGenericMethod(subType.Type);

            ParameterExpression lambdaInput1 = Expression.Parameter(typeof(DataAccessor), "accessor");
            ParameterExpression lambdaInput2 = Expression.Parameter(typeof(object), "data");
            ParameterExpression[] lambdaInputs = new ParameterExpression[] { lambdaInput1, lambdaInput2 };

            Expression[] methodInputs = new Expression[] { Expression.Constant(index), Expression.Convert(lambdaInput2, method.GetParameters()[1].ParameterType) };
            MethodCallExpression methodExpression = Expression.Call(Expression.Convert(lambdaInput1, DataAccessor.GetType()), method, methodInputs);

            return Expression.Lambda<Func<DataAccessor, object, bool>>(methodExpression, lambdaInputs).Compile();
        }

        /*******************************************/

        protected static void RecordError(Exception e, string message = "")
        {
            if (e.InnerException != null)
                message += e.InnerException.Message;
            else
                message += e.Message;
            BH.Engine.Reflection.Compute.RecordError(message);
        }

        /*******************************************/

        protected void SetPossibleItems<T>(IEnumerable<T> items)
        {
            Selector = new ItemSelector<T>(items, Name);
            Selector.ItemSelected += (sender, e) =>
            {
                SetItem(e);
                if (SelectedItem != null)
                    ItemSelected?.Invoke(this, SelectedItem);
            };
        }

        /*************************************/

        protected void OnDataUpdated()
        {
            if (SolutionExpired != null)
                SolutionExpired?.Invoke(this, new EventArgs());
        }

        /*************************************/

        protected void FindOldIndex(List<ParamInfo> newList, List<ParamInfo> oldList)
        {
            for (int i = 0; i < newList.Count; i++)
            {
                ParamInfo parameter = newList[i];
                int oldIndex = oldList.FindIndex(x => x.Name == parameter.Name);
                parameter.Fragments.AddOrReplace(new ParamOldIndexFragment { OldIndex = oldIndex });
            }
        }

        /*************************************/

        protected void UpdateInputGenericType(int index)
        {
            Type rawType = typeof(object);
            switch (m_OriginalTypes[index].UnderlyingType().Depth)
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

            m_CompiledGetters[index] = CreateInputAccessor(rawType, index);
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        protected List<Func<DataAccessor, object>> m_CompiledGetters = new List<Func<DataAccessor, object>>();
        protected List<Func<DataAccessor, object, bool>> m_CompiledSetters = new List<Func<DataAccessor, object, bool>>();
        protected List<Type> m_OriginalTypes = new List<Type>();

        private static bool m_UpgradeMessageShown = false;

        /*************************************/
    }
}

