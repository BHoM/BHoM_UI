/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

        public override bool SetItem(object method)
        {
            if (!base.SetItem(method))
                return false;

            if (Method == null)
                return false;

            if (Method is MethodInfo)
                Method = ((MethodInfo)Method).MakeGeneric();

            SetName();
            SetCategory();
            SetDescription();

            SetInputParams();
            SetOutputParams();

            SetDelegate();
            CompileInputGetters();
            CompileOutputSetters();

            return true;
        }

        /*************************************/

        public override object Run(object[] inputs)
        {
            if (m_CompiledFunc != null)
            {
                return m_CompiledFunc(inputs);
            }
            else if (m_CompiledInstanceFunc != null)
            {
                return m_CompiledInstanceFunc(inputs[0], inputs);
            }
            else if (m_CompiledAction != null)
            {
                m_CompiledAction(inputs);
                return null;
            }
            else if (m_CompiledInstanceAction != null)
            {
                m_CompiledInstanceAction(inputs[0], inputs);
                return null;
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
        /**** Private Methods             ****/
        /*************************************/

        protected virtual void SetName()
        {
            if (Method == null)
                return;

            if (Method is MethodInfo)
                Name = Method.Name;
            else if (Method is ConstructorInfo)
                Name = Method.DeclaringType.Name;
            else
                Name = "UnknownMethod";
        }

        /*************************************/

        protected virtual void SetDescription()
        {
            if (Method != null)
                Description = Method.Description();
        }

        /*************************************/

        protected virtual void SetCategory()
        {
            if (Method != null && Category == "Undefined")
            {
                string[] nameSpace = Method.DeclaringType.Namespace.Split('.');
                if (nameSpace.Length >= 2 && nameSpace[0] == "BH")
                    Category = nameSpace[1];
                else
                    Category = "Other";
            }
        }

        /*************************************/

        public virtual void SetInputParams()
        {
            if (Method == null)
                InputParams = new List<ParamInfo>();
            else
            {
                Dictionary<string, string> descriptions = Method.InputDescriptions();
                InputParams = Method.GetParameters().Select(x => new ParamInfo
                {
                    Name = x.Name,
                    DataType = x.ParameterType,
                    Description = descriptions.ContainsKey(x.Name) ? descriptions[x.Name] : "",
                    Kind = ParamKind.Input,
                    HasDefaultValue = x.HasDefaultValue,
                    DefaultValue = x.DefaultValue
                }).ToList();
                if (!Method.IsStatic)
                {
                    InputParams.Insert(0, new ParamInfo
                    {
                        Name = Method.DeclaringType.Name.ToLower(),
                        DataType = Method.DeclaringType,
                        Description = "",
                        Kind = ParamKind.Input,
                        HasDefaultValue = false,
                        DefaultValue = System.DBNull.Value
                    });
                }
            }
        }

        /*************************************/

        public virtual void SetDelegate()
        {
            Delegate compiledFunc = Engine.Reflection.Create.Delegate(Method);
            if (compiledFunc is Func<object, object[], object>)
            {
                m_CompiledInstanceFunc = (Func<object, object[], object>)compiledFunc;
            }
            else if (compiledFunc is Func<object[], object>)
            {
                m_CompiledFunc = (Func<object[], object>)compiledFunc;
            }
            else if (compiledFunc is Action<object[]>)
            {
                m_CompiledAction = (Action<object[]>)compiledFunc;
            }
            else if (compiledFunc is Action<object, object[]>)
            {
                m_CompiledInstanceAction = (Action<object, object[]>)compiledFunc;
            }
            else
            {
                throw new ArgumentException($"Delegate must of type " +
                    $"{m_CompiledFunc.GetType()}, " +
                    $"{m_CompiledAction.GetType()}, " +
                    $"{m_CompiledInstanceFunc.GetType()} or " +
                    $"{m_CompiledInstanceAction.GetType()}, " +
                    $"not {compiledFunc.GetType()}");
            }
        }

        public virtual void SetOutputParams()
        {
            if (Method == null)
                OutputParams = new List<ParamInfo>();
            else
            {
                if (Method.IsMultipleOutputs())
                {
                    Type[] subTypes = Method.OutputType().GenericTypeArguments;
                    List<OutputAttribute> attributes = Method.OutputAttributes();
                    if (subTypes.Length == attributes.Count)
                    {
                        OutputParams = Method.OutputAttributes().Select((x, i) => new ParamInfo
                        {
                            Name = x.Name,
                            DataType = subTypes[i],
                            Description = x.Description,
                            Kind = ParamKind.Output
                        }).ToList();
                    }
                    else
                    {
                        OutputParams = subTypes.Select(x => new ParamInfo
                        {
                            Name = x.UnderlyingType().Type.Name.Substring(0, 1),
                            DataType = x,
                            Description = "",
                            Kind = ParamKind.Output
                        }).ToList();
                    }
                }
                else
                {
                    Type nameType = Method.OutputType().UnderlyingType().Type;
                    if (nameType == typeof(void))
                        return;
                    string name = Method.OutputName();
                    OutputParams = new List<ParamInfo> {
                        new ParamInfo
                        {
                            Name = (name == "") ? nameType.Name.Substring(0, 1) : name,
                            DataType = Method.OutputType(),
                            Description = Method.OutputDescription(),
                            Kind = ParamKind.Output
                        }
                    };
                }
            }
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        protected Func<object[], object> m_CompiledFunc = null;

        protected Action<object[]> m_CompiledAction = null;

        protected Func<object, object[], object> m_CompiledInstanceFunc = null;

        protected Action<object, object[]> m_CompiledInstanceAction = null;

        /*************************************/
    }
}
