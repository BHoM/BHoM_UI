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
using BH.Engine.UI;

namespace BH.UI.Base
{
    public abstract partial class Caller
    {
        /*************************************/
        /**** Protected Methods           ****/
        /*************************************/

        protected virtual void SetInputs()
        {
            SetInputs(SelectedItem as dynamic);
        }


        /*************************************/
        /**** Targeted Methods            ****/
        /*************************************/

        protected virtual void SetInputs(MethodBase method)
        {
            if (method == null)
                InputParams = new List<ParamInfo>();
            else
            {
                Dictionary<string, string> descriptions = method.InputDescriptions();
                InputParams = method.GetParameters().Where(x => !x.IsOut)
                    .Select(x => Engine.UI.Create.ParamInfo(x, descriptions.ContainsKey(x.Name) ? descriptions[x.Name] : ""))
                    .ToList();

                if (method is MethodInfo && !method.IsStatic)
                {
                    InputParams.Insert(0, new ParamInfo
                    {
                        Name = method.DeclaringType.Name.ToLower(),
                        DataType = method.DeclaringType,
                        Description = "",
                        Kind = ParamKind.Input,
                        HasDefaultValue = false,
                        DefaultValue = System.DBNull.Value,
                        IsRequired = true
                    });
                }
            }
        }

        /*************************************/

        protected virtual void SetInputs(Type type)
        {
            object instance = Activator.CreateInstance(type);
            string[] excluded = new string[] { "BHoM_Guid", "Fragments", "Tags", "CustomData" };
            IEnumerable<ParamInfo> properties = type.GetProperties().Select(x => Engine.UI.Create.ParamInfo(x, instance));
            InputParams = properties.Where(x => !excluded.Contains(x.Name)).ToList();
        }

        /*************************************/

        protected virtual void SetInputs(object item)
        {
            // Nothing to do
        }

        /*************************************/
    }
}

