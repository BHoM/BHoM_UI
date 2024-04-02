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

using BH.Engine.Reflection;
using BH.oM.Base.Attributes;
using BH.oM.UI;
using System;
using System.Reflection;

namespace BH.Engine.UI
{
    public static partial class Create
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Input("name", "The name of the parameter.")]
        [Input("type", "The framework type of the parameter, e.g. BH.oM.Base.BHoMObject.")]
        [Input("kind", "Whether the parameter is an input of an output. Input is the default value.")]
        [Output("parameter", "The bhom parameter used in the bhom abstract syntax.")]
        public static ParamInfo ParamInfo(string name, Type type = null, ParamKind kind = ParamKind.Input)
        {
            if (type == null)
                type = typeof(object);

            return new ParamInfo
            {
                Name = name,
                DataType = type,
                Kind = kind
            };
        }

        /*************************************/

        [Input("property", "The system property to convert to bhom.")]
        [Output("parameter", "The bhom parameter used in the bhom abstract syntax.")]
        public static ParamInfo ParamInfo(this PropertyInfo property, object instance = null)
        {
            ParamInfo info = new ParamInfo
            {
                Name = property.Name,
                DataType = property.PropertyType,
                Description = property.IDescription(),
                Kind = ParamKind.Input,
                IsRequired = property.IsRequired(),
                DefaultValueWarning = property.DefaultValueWarning()
            };

            if (instance != null)
            {
                info.HasDefaultValue = true;
                info.DefaultValue = property.GetValue(instance);
            }

            return info;
        }

        /*************************************/

        [Input("parameter", "The system parameter to convert to bhom.")]
        [Output("parameter", "The bhom parameter used in the bhom abstract syntax.")]
        public static ParamInfo ParamInfo(this ParameterInfo parameter, string description = "")
        {
            Type paramType = parameter.ParameterType;
            if (paramType.IsByRef && paramType.HasElementType)
                paramType = paramType.GetElementType();

            return new ParamInfo
            {
                Name = parameter.Name,
                DataType = paramType,
                Description = description,
                Kind = ParamKind.Input,
                HasDefaultValue = parameter.HasDefaultValue,
                IsRequired = !parameter.HasDefaultValue,
                DefaultValue = parameter.DefaultValue
            };
        }

        /*************************************/
    }
}





