/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2019, the respective contributors. All rights reserved.
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
using BH.oM.Reflection.Attributes;
using BH.UI.Templates;
using System;
using System.ComponentModel;
using System.Reflection;

namespace BH.UI.Components
{
    public class SetPropertyCaller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.BHoM_SetProperty;

        public override Guid Id { get; protected set; } = new Guid("A186D4F1-FC80-499B-8BBF-ECDD49BF6E6E");

        public override string Name { get; protected set; } = "SetProperty";

        public override int GroupIndex { get; protected set; } = 2;

        public override string Category { get; protected set; } = "Engine";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public SetPropertyCaller() : base(typeof(SetPropertyCaller).GetMethod("SetProperty")) { }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        [Description("Set the value of a property with a given name for an object")]
        [Output("New object with its property changed to the new value")]
        public static BHoMObject SetProperty(BHoMObject obj, string propName, object value)
        {
            return Engine.Reflection.Modify.PropertyValue(obj, propName, value);
        }


        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        protected override object[] CollectInputs()
        {
            // EP. I am not sure why this code is here. It seems like it does nothing more than what the
            // RemoveInput and UpdateInput methods are doing. 
            // Maybe it has something to do with generics, but I will leave this here, since it works.
            object[] inputs = new object[] { };
            try
            {
                if (m_CompiledGetters.Count == 3)
                {
                    object obj = m_CompiledGetters[0](DataAccessor);
                    string propName = m_CompiledGetters[1](DataAccessor) as string;

                    if (propName != m_CurrentProperty && obj != null)
                    {
                        m_CurrentProperty = propName;

                        Type objType = obj.GetType();
                        if (objType != null)
                        {
                            PropertyInfo propInfo = objType.GetProperty(propName);
                            if (propInfo != null)
                                m_CompiledGetters[2] = CreateInputAccessor(propInfo.PropertyType, 2);
                        }
                    }

                    inputs = new object[] { obj, propName, m_CompiledGetters[2](DataAccessor) };
                }
            }
            catch (Exception e)
            {
                RecordError(e, "This component failed to run properly. Inputs cannot be collected properly.\n");
                inputs = null;
            }

            return inputs;
        }

        /*************************************/

        //public override string Write()
        //{
        //    try
        //    {
        //        CustomObject component = new CustomObject();
        //        component.CustomData["SelectedItem"] = SelectedItem;
        //        component.CustomData["InputParams"] = InputParams;
        //        return component.ToJson();
        //    }
        //    catch
        //    {
        //        BH.Engine.Reflection.Compute.RecordError($"{this} failed to serialise itself.");
        //        return "";
        //    }
        //}

        ///*************************************/

        //public override bool Read(string json)
        //{
        //    if (json == "")
        //        return true;

        //    try
        //    {
        //        CustomObject component = BH.Engine.Serialiser.Convert.FromJson(json) as CustomObject;

        //        object item;
        //        if (component.CustomData.TryGetValue("SelectedItem", out item))
        //            SelectedItem = item;

        //        object inputParams;
        //        if (component.CustomData.TryGetValue("InputParams", out inputParams))
        //        {
        //            InputParams = (inputParams as IEnumerable).OfType<ParamInfo>().ToList();
        //            CompileInputGetters();
        //        }
        //        return true;
        //    }
        //    catch
        //    {
        //        BH.Engine.Reflection.Compute.RecordError($"{this} failed to deserialise itself.");
        //        return false;
        //    }
        //}


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private string m_CurrentProperty = "";

        /*************************************/
    }
}
