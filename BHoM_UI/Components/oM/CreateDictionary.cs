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

using BH.Adapter;
using BH.oM.Base;
using BH.oM.Data.Requests;
using BH.oM.Reflection.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Components
{
    public class CreateDictionaryCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Dictionary;

        public override Guid Id { get; protected set; } = new Guid("2DF2A7FA-55D5-4BA6-8A1C-19BF2C555B04");

        public override string Category { get; protected set; } = "oM";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateDictionaryCaller() : base(typeof(CreateDictionaryCaller).GetMethod("CreateDictionary")) { }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Description("Create a dictionary")]
        [Input("keys", "List of keys for the dictionary")]
        [Input("values", "List of values for the dictionary")]
        [Input("keyType", "Type of the keys (default: auto detect)")]
        [Input("valueType", "Type of the values (default: auto detect)")]
        [Output("Resulting dictionary")]
        public static IDictionary CreateDictionary(List<object> keys, List<object> values, Type keyType = null, Type valueType = null)
        {
            if (keys.Count > 0 && values.Count == keys.Count)
            {
                if (keyType == null)
                    keyType = keys.First().GetType();
                if (valueType == null)
                    valueType = values.First().GetType();

                Type dicType = typeof(Dictionary<,>).MakeGenericType(new Type[] { keyType, valueType });
                IDictionary dic = (IDictionary)Activator.CreateInstance(dicType);
                for (int i = 0; i < keys.Count; i++)
                    dic.Add(keys[i], values[i]);

                return dic;
            }
            else
                return new Dictionary<string, object>();
        }

        /*************************************/
    }
}

