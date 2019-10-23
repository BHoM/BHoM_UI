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

using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BH.oM.Base;
using BH.oM.UI;
using BH.Engine.Reflection;
using BH.Engine.Serialiser;
using System.Collections;
using BH.Engine.UI;

namespace BH.UI.Components
{
    public class CreateObject2Caller : MethodCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.CreateBHoM;

        public override Guid Id { get; protected set; } = new Guid("D1B418EF-4CBB-4A7E-AC73-67FA78453F1A");

        public override string Name { get; protected set; } = "CreateObject2";

        public override string Category { get; protected set; } = "oM";

        public override string Description { get; protected set; } = "Creates an instance of a selected type of BHoM object";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateObject2Caller() : base()
        {
            List<MemberInfo> possibleItems = new List<MemberInfo>();
            possibleItems.AddRange(Engine.Reflection.Query.BHoMTypeList().Where(t => t?.GetInterface("IImmutable") == null));
            possibleItems.AddRange(Engine.UI.Query.CreateItems());

            SetPossibleItems(possibleItems);

            InputParams = new List<ParamInfo>();
            OutputParams = new List<ParamInfo>() { new ParamInfo { DataType = typeof(IObject), Kind = ParamKind.Output, Name = "object", Description = "New Object with properties set as per the inputs." } };
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public void SetInputs(List<string> names, List<Type> types = null)
        {
            if (types == null || names.Count != types.Count)
            {
                Engine.Reflection.Compute.RecordWarning("The list length for names and types does not match. Inputs are set, but <types> variable will be ignored.");
                types = new List<Type>(new Type[names.Count]);
            }

            for (int i = 0; i < names.Count; i++)
                AddInput(i, names[i], types[i]);

            CompileInputGetters();
            CompileOutputSetters();
        }

        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        public override object Run(object[] inputs)
        {
            if (SelectedItem is MethodBase)
                return base.Run(inputs);

            IObject obj = new CustomObject();
            if (SelectedItem is Type)
                obj = Activator.CreateInstance((Type)SelectedItem) as IObject;

            if (inputs.Length == InputParams.Count)
            {
                for (int i = 0; i < inputs.Length; i++)
                    BH.Engine.Reflection.Modify.SetPropertyValue(obj, InputParams[i].Name, inputs[i]);
            }

            return obj;
        }

        /*************************************/

        public override bool SetItem(object item)
        {
            if (item is MethodBase)
            {
                return base.SetItem(item);
            }
            else if (item is Type)
            {
                SelectedItem = item;
                Type type = item as Type;
                Name = type.Name;
                Description = type.Description();
                InputParams.AddRange(type.GetProperties().Select(x => x.ToBHoM()).ToList());
            }
            return true;
        }

        /*************************************/

        public override bool AddInput(int index, string name, Type type = null)
        {
            if (SelectedItem is Type)
            {
                PropertyInfo info = ((Type)SelectedItem).GetProperty(name);
                if (info != null)
                    type = info.PropertyType;
            }

            return base.AddInput(index, name, type);
        }

        /*************************************/
    }
}
