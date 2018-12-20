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
using BH.oM.Reflection;
using BH.Engine.UI;
using BH.oM.Reflection.Attributes;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BH.Engine.Serialiser;

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

        public MethodCaller(Type methodDeclaringType, string methodName, List<Type> paramTypes) : base()
        {
            SetItem(BH.Engine.UI.Create.MethodInfo(methodDeclaringType, methodName, paramTypes));
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

            if (Method.ContainsGenericParameters)
            {
                if (Method is MethodInfo)
                {
                    Type[] types = Method.GetGenericArguments().Select(x => GetConstructedType(x)).ToArray();
                    Method = ((MethodInfo)Method).MakeGenericMethod(types);
                }
            }

            SetName();
            SetCategory();
            SetDescription();

            SetInputParams();
            SetOutputParams();

            CompileFunction();
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
            else if (inputs.Length <= 0)
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

        protected virtual void CompileFunction()
        {
            if (Method == null)
                return;

            ParameterExpression lambdaInput = Expression.Parameter(typeof(object[]), "x");
            Expression[] inputs = Method.GetParameters().Select((x, i) => Expression.Convert(Expression.ArrayIndex(lambdaInput, Expression.Constant(i)), x.ParameterType)).ToArray();

            if (Method is MethodInfo)
            {
                MethodCallExpression methodExpression = Expression.Call(Method as MethodInfo, inputs);
                m_CompiledFunc = Expression.Lambda<Func<object[], object>>(Expression.Convert(methodExpression, typeof(object)), lambdaInput).Compile();
            }
            else if (Method is ConstructorInfo)
            {
                NewExpression constructorExpression = Expression.New(Method as ConstructorInfo, inputs);
                m_CompiledFunc = Expression.Lambda<Func<object[], object>>(Expression.Convert(constructorExpression, typeof(object)), lambdaInput).Compile();
            }
            
        }

        /*************************************/

        protected Type GetConstructedType(Type type)
        {
            if (type.IsGenericParameter)
            {
                Type[] constrains = type.GetGenericParameterConstraints();
                if (constrains.Length == 0)
                    return typeof(object);
                else
                    return constrains[0];
            }
            else if (type.ContainsGenericParameters)
            {
                Type[] constrains = type.GetGenericArguments().Select(x => GetConstructedType(x)).ToArray();
                return type.GetGenericTypeDefinition().MakeGenericType(constrains);
            }
            else
                return type;
        }

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
            }
        }

        /*************************************/

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

        /*************************************/
    }
}
