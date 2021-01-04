/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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
using BH.oM.Reflection.Attributes;

namespace BH.UI.Base
{
    public abstract partial class Caller
    {
        /*************************************/
        /**** Protected Methods           ****/
        /*************************************/

        protected virtual void SetOutputs()
        {
            SetOutputs(SelectedItem as dynamic);
        }


        /*************************************/
        /**** Targeted Methods            ****/
        /*************************************/

        protected virtual void SetOutputs(MethodBase method)
        {
            if (method == null)
                OutputParams = new List<ParamInfo>();
            else
            {
                if (method.IsMultipleOutputs())
                {
                    Type[] subTypes = method.OutputType().GenericTypeArguments;
                    List<OutputAttribute> attributes = method.OutputAttributes();
                    if (subTypes.Length == attributes.Count)
                    {
                        OutputParams = attributes.Select((x, i) => new ParamInfo
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
                    Type nameType = method.OutputType().UnderlyingType().Type;
                    if (nameType == typeof(void))
                        return;
                    string name = method.OutputName();
                    OutputParams = new List<ParamInfo> {
                        new ParamInfo
                        {
                            Name = (name == "") ? nameType.Name.Substring(0, 1) : name,
                            DataType = method.OutputType(),
                            Description = method.OutputDescription(),
                            Kind = ParamKind.Output,
                            IsRequired = true
                        }
                    };
                }
            }
        }

        /*************************************/

        protected virtual void SetOutputs(Type type)
        {
            OutputParams = new List<ParamInfo>() { new ParamInfo { DataType = type, Kind = ParamKind.Output, Name = Name.Substring(0, 1), Description = type.Description(), IsRequired = true } };
        }

        /*************************************/

        protected virtual void SetOutputs(object item)
        {
            // Nothing to do
        }

        /*************************************/
    }
}


