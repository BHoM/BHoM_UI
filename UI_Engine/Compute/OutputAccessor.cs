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
    public static partial class Compute
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [PreviousVersion("8.2", "BH.Engine.UI.Compute.OutputAccessor(System.Type, System.Type)")]
        public static Func<IDataAccessor, object, int, bool> OutputAccessor(Type accessorType, Type dataType, bool quantitiesAsDouble = true)
        {
            UnderlyingType subType = dataType.UnderlyingType();
            string methodName = (subType.Depth == 0) ? "SetDataItem" : (subType.Depth == 1) ? "SetDataList" : "SetDataTree";

            Type type = subType.Type;
            if (quantitiesAsDouble && typeof(BH.oM.Quantities.IQuantity).IsAssignableFrom(type))
                type = typeof(double);

            MethodInfo method = accessorType.GetMethod(methodName).MakeGenericMethod(type.IsByRef ? type.GetElementType() : type);

            ParameterExpression lambdaInput1 = Expression.Parameter(typeof(IDataAccessor), "accessor");
            ParameterExpression lambdaInput2 = Expression.Parameter(typeof(object), "data");
            ParameterExpression lambdaInput3 = Expression.Parameter(typeof(int), "index");
            ParameterExpression[] lambdaInputs = new ParameterExpression[] { lambdaInput1, lambdaInput2, lambdaInput3 };

            Expression[] methodInputs = new Expression[] { lambdaInput3, Expression.Convert(lambdaInput2, method.GetParameters()[1].ParameterType) };
            MethodCallExpression methodExpression = Expression.Call(Expression.Convert(lambdaInput1, accessorType), method, methodInputs);

            return Expression.Lambda<Func<IDataAccessor, object, int, bool>>(methodExpression, lambdaInputs).Compile();
        }

        /*************************************/
    }
}






