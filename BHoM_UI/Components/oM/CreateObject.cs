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
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using BH.Engine.Reflection;
using System.Linq.Expressions;
using BH.oM.UI;
using BH.Engine.UI;

namespace BH.UI.Components
{
    public class CreateObjectCaller : MethodCaller
    {
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
        /**** Override Methods            ****/
        /*************************************/

        public override object Run(object[] inputs)
        {
            if (SelectedItem is MethodBase)
                return base.Run(inputs);
            else if (SelectedItem is Type)
            {
                if (m_CompiledFunc == null)
                    m_CompiledFunc = CreateConstructor((Type)SelectedItem, InputParams);
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
                Type type = item as Type;
                Name = type.Name;
                Description = type.Description();
                InputParams = type.GetProperties().Select(x => x.ToBHoM()).ToList();
                OutputParams = new List<ParamInfo>() { new ParamInfo { DataType = type, Kind = ParamKind.Output, Name = Name.Substring(0, 1), Description = type.Description() } };
                m_CompiledFunc = CreateConstructor(type, InputParams);

                CompileInputGetters();
                CompileOutputSetters();
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

        public override bool RemoveInput(string name)
        {
            if (name == null)
                return false;

            bool success = InputParams.RemoveAll(p => p.Name == name) > 0;
            CompileInputGetters();

            if (SelectedItem is Type)
                m_CompiledFunc = CreateConstructor((Type)SelectedItem, InputParams);
            return success;
        }

        /*************************************/

        public override bool Read(string json)
        {
            if (!base.Read(json))
                return false;

            if (SelectedItem is Type)
                m_CompiledFunc = CreateConstructor((Type)SelectedItem, InputParams);

            return true;
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        protected Func<object[], object> CreateConstructor(Type type, List<ParamInfo> parameters)
        {
            ParameterExpression lambdaInput = Expression.Parameter(typeof(object[]), "x");
            Expression[] inputs = parameters.Select((x, i) => Expression.Convert(Expression.ArrayIndex(lambdaInput, Expression.Constant(i)), x.DataType)).ToArray();

            ParameterExpression instance = Expression.Variable(type, "instance");
            List<Expression> assignments = new List<Expression>();
            assignments.Add(Expression.Assign(instance, Expression.New(type)));

            for (int i = 0; i < parameters.Count; i++)
            {
                ParamInfo param = parameters[i];
                PropertyInfo property = type.GetProperty(param.Name);
                MethodInfo setMethod = property.GetSetMethod();

                MethodCallExpression methodCall = Expression.Call(instance, setMethod, inputs[i]);
                assignments.Add(methodCall);
            }
            assignments.Add(instance);

            BlockExpression block = Expression.Block(new[] { instance }, assignments);
            return Expression.Lambda<Func<object[], object>>(Expression.Convert(block, typeof(object)), lambdaInput).Compile();

        }

        /*************************************/
    }

}
