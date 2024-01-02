/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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
using BH.oM.Base.Reflection;
using BH.oM.Base.Attributes;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BH.Engine.UI
{
    public static partial class Create
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static Func<IDataAccessor, int, object> InputAccessor(Type accessorType, Type dataType)
        {
            UnderlyingType subType = dataType.UnderlyingType();
            string methodName = (subType.Depth == 0) ? "GetDataItem" : (subType.Depth == 1) ? "GetDataList" : "GetDataTree";

            Type inputType = subType.Type;
            if (inputType.IsByRef && inputType.HasElementType)
                inputType = inputType.GetElementType();
            MethodInfo method = accessorType.GetMethod(methodName).MakeGenericMethod(inputType);

            ParameterExpression lambdaInput1 = Expression.Parameter(typeof(IDataAccessor), "accessor");
            ParameterExpression lambdaInput2 = Expression.Parameter(typeof(int), "index");
            ParameterExpression[] lambdaInputs = new ParameterExpression[] { lambdaInput1, lambdaInput2 };

            Expression[] methodInputs = new Expression[] { lambdaInput2 };
            MethodCallExpression methodExpression = Expression.Call(Expression.Convert(lambdaInput1, accessorType), method, methodInputs);
            Func<IDataAccessor, int, object> lambda = Expression.Lambda<Func<IDataAccessor, int, object>>(Expression.Convert(methodExpression, typeof(object)), lambdaInputs).Compile();

            if (dataType.IsArray)
            {
                // If dataType is an array type, the underlying method asks for an array type
                // Thus, we add a new node to the syntax tree that casts the List to an Array
                MethodInfo castMethod = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(subType.Type);
                ParameterExpression lambdaResult = Expression.Parameter(typeof(object), "lambdaResult");
                MethodCallExpression castExpression = Expression.Call(null, castMethod, Expression.Convert(lambdaResult, typeof(IEnumerable<>).MakeGenericType(subType.Type)));
                Func<object, object> castDelegate = Expression.Lambda<Func<object, object>>(castExpression, lambdaResult).Compile();

                return (accessor, index) => { return castDelegate(lambda(accessor, index)); };
            }
            else if (subType.Depth == 1 && !dataType.IsValueType && dataType.Name != "List`1" && dataType.Name != "IEnumerable`1")
            {
                // If we have a `DataList` that isn't actually a list, lets try to create it from teh list through the appropriate constructor
                foreach (ConstructorInfo constructor in dataType.GetConstructors())
                {
                    ParameterInfo[] parameters = constructor.GetParameters();
                    if (parameters.Count() == 1)
                    {
                        string paramType = parameters[0].ParameterType.Name;
                        if (paramType == "List`1" || paramType == "IEnumerable`1")
                        {
                            ParameterExpression lambdaResult = Expression.Parameter(typeof(object), "lambdaResult");
                            NewExpression castExpression = Expression.New(constructor, Expression.Convert(lambdaResult, parameters[0].ParameterType));
                            Func<object, object> castDelegate = Expression.Lambda<Func<object, object>>(castExpression, lambdaResult).Compile();
                            return (accessor, index) => { return castDelegate(lambda(accessor, index)); };
                        }
                    }
                }
                return lambda;
            }
            else
                return lambda;
        }

        /*************************************/
    }
}





