/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2023, the respective contributors. All rights reserved.
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

using BH.oM.Base.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BH.UI.Base.Components
{
    public class GetPropertyCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.BHoM_GetProperty;

        public override Guid Id { get; protected set; } = new Guid("C0BCB684-80E5-4A67-BF0E-6B8C2C917312");

        public override string Name { get; protected set; } = "GetProperty";

        public override int GroupIndex { get; protected set; } = 2;

        public override string Category { get; protected set; } = "Engine";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public GetPropertyCaller() : base(typeof(GetPropertyCaller).GetMethod("GetProperty")) { }


        /*************************************/
        /**** Override Method             ****/
        /*************************************/

        public override object Run(List<object> inputs)
        {
            object result = base.Run(inputs);

            // Set the output type
            if (result != null && OutputParams.Count > 0)
            {
                Type type = result.GetType();
                if (OutputParams[0].DataType != type)
                {
                    OutputParams[0].DataType = type;
                    CompileOutputSetters();
                }
            }

            return result;
        }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        [Description("Get the value of a property or of a list of properties from an object.")]
        [Input("obj", "Object to extract the property value from. Can be a list of objects.")]
        [Input("propName", "Name of the property to extract. Can be a list of names."
            + "\nYou can use the dot notation to access nested properties: e.g. for e.g. for a BH.oM.Structure.Bar, you can input 'StartNode.Position' to get the BH.oM.Geometry.Point."
            + "\nTo know what property names you can specify for a given object, connect your object to a BHoM `Explode` component.")]
        [Output("Value", "Extracted values.")]
        public static object GetProperty(object obj, string propName)
        {
            return Engine.Base.Query.PropertyValue(obj, propName);
        }

        /*************************************/
    }
}




