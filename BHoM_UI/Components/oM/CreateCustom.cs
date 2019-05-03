/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2018, the respective contributors. All rights reserved.
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

namespace BH.UI.Components
{
    public class CreateCustomCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.CustomObject;

        public override Guid Id { get; protected set; } = new Guid("EB7B72E5-B4D8-4FF6-BCBD-833CDEC5D1A2");

        public override string Name { get; protected set; } = "CreateCustom";

        public override string Category { get; protected set; } = "oM";

        public override string Description { get; protected set; } = "Creates an instance of a selected type of BHoM object by manually defining its properties (default type is CustomObject)";

        public Type ForcedType
        {
            get
            {
                return SelectedItem as Type;
            }
            protected set
            {
                SelectedItem = value;
            }
        }

        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateCustomCaller() : base()
        {
            SetPossibleItems(Engine.Reflection.Query.BHoMTypeList().Where(t => t?.GetInterface("IImmutable") == null));

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
                InputParams = names.Select(x => GetParam(x)).ToList();
            }
            else
            {
                InputParams = names.Zip(types, (name, type) => GetParam(name, type)).ToList();
            }

            CompileInputGetters();
            CompileOutputSetters();
        }

        /*************************************/

        public bool AddInput(int index, string name, Type type)
        {
            if (name == null)
                return false;

            InputParams.Insert(index, GetParam(name, type));
            CompileInputGetters();
            return true;
        }

        /*************************************/

        public bool RemoveInput(string name)
        {
            if (name == null)
                return false;

            bool success = InputParams.RemoveAll(p => p.Name == name) > 0;
            CompileInputGetters();
            return success;
        }

        /*************************************/

        public bool UpdateInput(int index, string name, Type type = null)
        {
            if (InputParams.Count <= index)
                return AddInput(index, name, type);

            if (name != null)
                InputParams[index].Name = name;

            if (type != null)
                InputParams[index].DataType = type;

            CompileInputGetters();
            return true;
        }


        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        public override object Run(object[] inputs)
        {
            IObject obj = new CustomObject();
            if (ForcedType != null)
                obj = Activator.CreateInstance(ForcedType) as IObject;
            if (obj == null)
                obj = new CustomObject();

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
            if (!base.SetItem(item))
                return false;

            if (ForcedType != null)
            {
                Name = ForcedType.Name;
                Description = ForcedType.Description();
                InputParams.AddRange(ForcedType.GetProperties().Select(x => GetParam(x)).ToList());
            }
            return true;
        }

        /*************************************/

        public override string Write()
        {
            try
            {
                CustomObject component = new CustomObject();
                component.CustomData["SelectedItem"] = SelectedItem;
                component.CustomData["InputParams"] = InputParams;
                return component.ToJson();
            }
            catch
            {
                BH.Engine.Reflection.Compute.RecordError($"{this} failed to serialise itself.");
                return "";
            }
        }

        /*************************************/

        public override bool Read(string json)
        {
            if (json == "")
                return true;

            try
            {
                List<string> baseInputs = new List<string>();
                CustomObject component = BH.Engine.Serialiser.Convert.FromJson(json) as CustomObject;
                if (component.CustomData["SelectedItem"] != null)
                {
                    SelectedItem = component.CustomData["SelectedItem"];
                }

                if (ForcedType != null)
                {
                    this.Name = ForcedType.Name;
                    this.Description = ForcedType.Description();
                }

                if (component.CustomData["InputParams"] != null)
                {
                    IEnumerable inputs = component.CustomData["InputParams"] as IEnumerable;
                    InputParams = inputs.OfType<ParamInfo>().ToList();
                }
                CompileInputGetters();
                CompileOutputSetters();
                return true;
            }
            catch
            {
                BH.Engine.Reflection.Compute.RecordError($"{this} failed to deserialise itself.");
                return false;
            }
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        public oM.UI.ParamInfo GetParam(string name, Type type = null)
        {
            if (type == null)
                type = typeof(object);

            if (ForcedType != null)
            {
                PropertyInfo info = ForcedType.GetProperty(name);
                if (info != null)
                    type = info.PropertyType;
            }

            return new ParamInfo
            {
                Name = name,
                DataType = type,
                Kind = ParamKind.Input
            };
        }

        /*************************************/

        public oM.UI.ParamInfo GetParam(PropertyInfo info)
        {
            return new ParamInfo
            {
                Name = info.Name,
                DataType = info.PropertyType,
                Description = info.IDescription(),
                Kind = ParamKind.Input
            };
        }

        /*************************************/
    }
}
