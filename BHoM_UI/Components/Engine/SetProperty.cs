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

using BH.oM.Base;
using BH.oM.Base.Attributes;
using System;
using System.ComponentModel;
using System.Reflection;
using BH.Engine.Base;
using System.Collections.Generic;
using BH.Engine.Reflection;
using System.Linq;
using System.Collections;

namespace BH.UI.Base.Components
{
    public class SetPropertyCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.BHoM_SetProperty;

        public override Guid Id { get; protected set; } = new Guid("A186D4F1-FC80-499B-8BBF-ECDD49BF6E6E");

        public override string Name { get { return "SetProperty";  } protected set { } }

        public override int GroupIndex { get; protected set; } = 2;

        public override string Category { get; protected set; } = "Engine";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public SetPropertyCaller() : base(typeof(Engine.Base.Modify).GetMethod("SetPropertyValue")) { }


        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        protected override List<object> CollectInputs()
        {
            // This makes sure that the output type is always matching the property type.
            // This is especially important to differentiate between items, lists and trees

            List<object> inputs = new List<object>();
            try
            {
                if (m_CompiledGetters.Count == 3)
                {
                    object obj = m_CompiledGetters[0](m_DataAccessor, 0);
                    string propName = m_CompiledGetters[1](m_DataAccessor, 1) as string;

                    if (propName != m_CurrentProperty && obj != null)
                    {
                        m_CurrentProperty = propName;

                        Type objType = obj.GetType();
                        if (objType != null)
                        {
                            Type propType = GetPropertyType(objType, propName);
                            if (propType == null && propName.Contains("."))
                            {
                                // If property type is null, it might be because the user is trying to access properties of objects exposed only through an interface (and therefore not available)
                                // (e.g. SectionProperty.Reinforcement of a Bar with SectionProperty being an ISectionProperty interface that doesn't have a Reinforcement property)
                                // So let's try to get the type directly from the value of the target property if it exists
                                propType = GetPropertyType(obj, propName);
                            }
                            if (propType == null)
                                propType = typeof(object); // Fallback to object tpye in case of properties set on CustomData for example
                            if (propType.IsValueType)
                                propType = typeof(object);
                            m_CompiledGetters[2] = Engine.UI.Create.InputAccessor(m_DataAccessor.GetType(), propType);  
                        }
                    }

                    inputs = new List<object> { obj, propName, m_CompiledGetters[2](m_DataAccessor, 2) };
                }
            }
            catch (Exception e)
            {
                Engine.UI.Compute.RecordError(e, "This component failed to run properly. Inputs cannot be collected properly.\n");
                inputs = null;
            }

            return inputs;
        }

        /*************************************/

        public override object Run(List<object> inputs)
        {
            if (inputs != null && inputs.Count >= 1 && inputs[0] != null)
            {
                // Deepclone must be done before the properties are set to ensure immutability
                // TODO: We need to shallow clone only. Let's not forget the case of properties setting like "X.Y.Z"
                // TODO: DeepClone should ignore fragments and CustomData
                inputs[0] = inputs[0].DeepClone();
            }
            return base.Run(inputs);
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private Type GetPropertyType(Type objType, string propName)
        {
            string[] props = propName.Split('.');
            for (int i = 0; i < props.Length; i++)
            {
                PropertyInfo propInfo = objType?.GetProperty(props[i]);
                if (propInfo != null)
                    objType = propInfo.PropertyType;
                else 
                    return null;

                if (objType.GetInterfaces().Any(x => x.Name == "IEnumerable`1") && i < props.Length - 1)
                   objType = objType.GetGenericArguments().FirstOrDefault(); // Get the type inside the list for intermediate properties
            }

            return objType;
        }

        /*************************************/

        private Type GetPropertyType(object obj, string propName)
        {
            if (obj == null || propName == null)
                return null;

            string[] props = propName.Split('.');
            for (int i = 0; i < props.Length; i++)
            {
                Type objType = obj.GetType();
                PropertyInfo propInfo = objType.GetProperty(props[i]);
                if (propInfo != null)
                {
                    obj = propInfo.GetValue(obj);
                    if (obj is IEnumerable && i < props.Length - 1)
                        obj = FirstOrDefault(obj as dynamic); // Get the type inside the list for intermediate properties
                } 
                else
                    return null;
            }

            return obj?.GetType();
        }

        /*************************************/

        private T FirstOrDefault<T>(IEnumerable<T> list)
        {
            return list.FirstOrDefault();
        }

        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private string m_CurrentProperty = "";

        /*************************************/
    }
}






