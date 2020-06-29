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

using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using BH.Engine.Reflection;
using System.Linq.Expressions;
using BH.oM.UI;
using BH.Engine.UI;
using System.Windows.Forms;
using BH.oM.Base;
using System.Collections;

namespace BH.UI.Components
{
    public class CreateObjectCaller : MethodCaller
    {
        /*************************************/
        /**** Public Events               ****/
        /*************************************/

        public event EventHandler<Tuple<ParamInfo, bool>> InputToggled;


        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.CreateBHoM;

        public override Guid Id { get; protected set; } = new Guid("76221701-C5E7-4A93-8A2B-D34E77ED9CC1");

        public override string Name { get; protected set; } = "CreateObject";

        public override string Category { get; protected set; } = "oM";

        public override string Description { get; protected set; } = "Creates an instance of a selected type of BHoM object";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateObjectCaller() : base()
        {
            List<MemberInfo> possibleItems = new List<MemberInfo>();
            possibleItems.AddRange(Engine.UI.Query.ConstructableTypeItems());
            possibleItems.AddRange(Engine.UI.Query.CreateItems());

            SetPossibleItems(possibleItems);
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

            InputParams = new List<ParamInfo>();
            for (int i = 0; i < names.Count; i++)
                AddInput(i, names[i], types[i]);

            if (SelectedItem is Type)
                m_CompiledFunc = Engine.UI.Compute.Constructor((Type)SelectedItem, InputParams);

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
            else if (SelectedItem is Type)
            {
                if (m_CompiledFunc == null)
                    m_CompiledFunc = Engine.UI.Compute.Constructor((Type)SelectedItem, InputParams);
                return m_CompiledFunc(inputs);
            }
            else
                return null;
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
                m_OriginalMethod = null;
                Type type = item as Type;
                Name = type.Name;
                Description = type.Description();

                if (type.IsGenericTypeDefinition)
                    type = type.MakeFromGeneric();

                object instance = Activator.CreateInstance(type);  
                string[] excluded = new string[] { "BHoM_Guid", "Fragments", "Tags", "CustomData" };
                IEnumerable<ParamInfo> properties = type.GetProperties().Select(x => x.FromProperty(instance));
                InputParams = properties.Where(x => !excluded.Contains(x.Name)).ToList();
                OutputParams = new List<ParamInfo>() { new ParamInfo { DataType = type, Kind = ParamKind.Output, Name = Name.Substring(0, 1), Description = type.Description() } };

                SetInputSelectionMenu(type, InputParams.Select(x => x.Name));

                m_CompiledFunc = Engine.UI.Compute.Constructor(type, InputParams);
                CompileInputGetters();
                CompileOutputSetters();
            }
            return true;
        }

        /*************************************/

        private void SetInputSelectionMenu(Type type, IEnumerable<string> selectedProperties)
        {
            object instance = Activator.CreateInstance(type);
            IEnumerable<ParamInfo> properties = type.GetProperties().Select(x => x.FromProperty(instance));

            m_InputSelector = new ParamSelectorMenu(properties.Select(x => new Tuple<ParamInfo, bool>(x, selectedProperties.Contains(x.Name))).ToList());
            m_InputSelector.ParamToggled += M_InputSelector_InputToggled;
        }

        /*************************************/

        private void M_InputSelector_InputToggled(object sender, Tuple<ParamInfo, bool> e)
        {
            if (e.Item2)
            {
                AddInput(InputParams.Count, e.Item1.Name, e.Item1.DataType);
                if (SelectedItem is Type)
                    m_CompiledFunc = Engine.UI.Compute.Constructor((Type)SelectedItem, InputParams);
            }   
            else
                RemoveInput(e.Item1.Name);

            InputToggled?.Invoke(this, e);
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

        public override bool RemoveInput(string name)
        {
            if (name == null)
                return false;

            bool success = InputParams.RemoveAll(p => p.Name == name) > 0;
            CompileInputGetters();

            if (m_InputSelector != null)
                m_InputSelector.SetParamCheck(name, false);

            if (SelectedItem is Type)
                m_CompiledFunc = Engine.UI.Compute.Constructor((Type)SelectedItem, InputParams);
            return success;
        }

        /*************************************/

        public override bool Read(string json)
        {
            if (!base.Read(json))
                return false;

            try
            {
                if (SelectedItem == null && OutputParams.Count == 1)
                {
                    CustomObject component = Engine.Serialiser.Convert.FromJson(json.Replace("\"_t\"", "_refType")) as CustomObject;
                    if (component != null && component.CustomData.ContainsKey("SelectedItem"))
                    {
                        CustomObject itemData = component.CustomData["SelectedItem"] as CustomObject;
                        if (itemData != null && itemData.CustomData.ContainsKey("_refType") && (string)itemData.CustomData["_refType"] == "System.Reflection.MethodBase")
                        {
                            Type type = OutputParams.First().DataType;
                            if (!type.IsNotImplemented() && !type.IsDeprecated() && type?.GetInterface("IImmutable") == null && !type.IsEnum && !type.IsAbstract) // Is type constructable ?
                            {
                                // Translate from method param name to property name
                                Dictionary<string, PropertyInfo> properties = type.GetProperties().ToDictionary(x => x.Name.ToLower(), x => x);
                                foreach (ParamInfo parameter in InputParams)
                                {
                                    string key = parameter.Name.ToLower();
                                    if (properties.ContainsKey(key) && parameter.DataType == properties[key].PropertyType)
                                        parameter.Name = properties[key].Name;
                                }

                                // Set the type item and the selection menu
                                SelectedItem = type;
                                SetInputSelectionMenu(type, InputParams.Select(x => x.Name));
                            }
                        }
                    }

                    if (SelectedItem is Type)
                        m_CompiledFunc = Engine.UI.Compute.Constructor((Type)SelectedItem, InputParams);
                }

                else if (SelectedItem is Type)
                {
                    CustomObject component = Engine.Serialiser.Convert.FromJson(json) as CustomObject;
                    if (component != null && component.CustomData.ContainsKey("InputParams"))
                    {
                        object inputParamsRecord;
                        List<ParamInfo> inputParams = new List<ParamInfo>();
                        if (component.CustomData.TryGetValue("InputParams", out inputParamsRecord))
                            InputParams = (inputParamsRecord as IEnumerable).OfType<ParamInfo>().ToList();

                        Type type = SelectedItem as Type;
                        SetInputSelectionMenu(type, InputParams.Select(x => x.Name));

                        m_CompiledFunc = Engine.UI.Compute.Constructor(type, InputParams);
                        CompileInputGetters();
                    }
                }
                
            }
            catch (Exception e)
            {
                Engine.Reflection.Compute.RecordError("Failed to deserialised the selected method. \nError: " + e.Message);
            }

            return true;
        }

        /*************************************/

        public override void AddToMenu(ToolStripDropDown menu)
        {
            if (SelectedItem != null && m_InputSelector != null)
                m_InputSelector.AddParamList(menu);
            else
                base.AddToMenu(menu);
        }

        /*************************************/

        public override void AddToMenu(System.Windows.Controls.ContextMenu menu)
        {
            if (SelectedItem != null && m_InputSelector != null)
                m_InputSelector.AddParamList(menu);
            else
                base.AddToMenu(menu);
        }

        /*************************************/

        public override void AddToMenu(object menu)
        {
            if (SelectedItem != null && m_InputSelector != null)
                m_InputSelector.AddParamList(menu);
            else
                base.AddToMenu(menu);
        }

        /*************************************/

        protected override bool AreMatching(List<ParamInfo> newList, List<ParamInfo> oldList, bool isInput)
        {
            if (isInput && SelectedItem is Type)
            {
                Type type = SelectedItem as Type;
                List<PropertyInfo> props = type.GetProperties().ToList();
                return oldList.All(x => props.Exists(p => p.Name == x.Name && p.PropertyType == x.DataType));
            }
            else
            {
                return base.AreMatching(newList, oldList, isInput);
            }
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        ParamSelectorMenu m_InputSelector;

        /*************************************/
    }

}

