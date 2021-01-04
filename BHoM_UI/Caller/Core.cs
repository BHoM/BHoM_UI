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
using BH.UI.Base.Menus;

namespace BH.UI.Base
{
    public abstract partial class Caller
    {
        /*************************************/
        /**** Events                      ****/
        /*************************************/

        public event EventHandler<CallerUpdate> Modified;

        public event EventHandler SolutionExpired;


        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public virtual System.Drawing.Bitmap Icon_24x24 { get; protected set; }

        public virtual Guid Id { get; protected set; }

        public virtual string Name { get; protected set; } = "Undefined";

        public virtual string Category { get; protected set; } = "Undefined";

        public virtual string Description { get; protected set; } = "";

        public virtual int GroupIndex { get; protected set; } = 1;

        public List<ParamInfo> InputParams { get; protected set; } = new List<ParamInfo>();

        public List<ParamInfo> OutputParams { get; protected set; } = new List<ParamInfo>();

        public object SelectedItem { get; protected set; } = null;

        public bool HasPossibleItems { get; protected set; } = false;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public Caller() : base()
        {
            if (SelectedItem != null)
                SetItem(SelectedItem);
        }

        /*************************************/

        public Caller(object item) : base()
        {
            if (item != null)
                SetItem(item);
        }

        /*************************************/

        static Caller()
        {
            if (!m_Initialised)
            {
                m_Initialised = true;
                Engine.UI.Compute.LoadAssemblies();
                Global.Initialisation.Activate();
            }
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public virtual void SetDataAccessor(IDataAccessor accessor)
        {
            if (accessor == null)
                return;

            Type oldType = m_DataAccessor == null ? null : m_DataAccessor.GetType();
            m_DataAccessor = accessor;

            if (oldType != m_DataAccessor.GetType())
            {
                CompileInputGetters();
                CompileOutputSetters();
            }
        }

        /*************************************/

        protected void SetPossibleItems<T>(IEnumerable<T> items)
        {
            HasPossibleItems = true;
            m_ItemSelector = new ItemSelector(items.Cast<object>(), Name);
            m_ItemSelector.ItemSelected += (sender, e) => SetItem(e);
        }

        /*************************************/

        public virtual string GetFullName()
        {
            return "BH." + Category + "." + Name;
        }

        /*************************************/

        public virtual bool IsObsolete()
        {
            return SelectedItem == null ? false : SelectedItem.IIsDeprecated();
        }


        /*************************************/
        /**** Protected Methods           ****/
        /*************************************/

        protected void ExpireSolution()
        {
            if (SolutionExpired != null)
                SolutionExpired?.Invoke(this, new EventArgs());
        }

        /*************************************/

        protected void MarkAsModified(CallerUpdate update)
        {
            if (Modified != null)
                Modified?.Invoke(this, update);
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        protected Func<object[], object> m_CompiledFunc = null;
        protected List<Func<IDataAccessor, int, object>> m_CompiledGetters = new List<Func<IDataAccessor, int, object>>();
        protected List<Func<IDataAccessor, object, int, bool>> m_CompiledSetters = new List<Func<IDataAccessor, object, int, bool>>();

        protected ItemSelector m_ItemSelector = null;
        protected ParamSelectorMenu m_InputSelector = null;
        protected ParamSelectorMenu m_OutputSelector = null;

        protected object m_OriginalItem = null;
        protected IDataAccessor m_DataAccessor = null;
        protected bool m_IsMissingParamInfo = false;
        protected static bool m_Initialised = false;

        /*************************************/

    }
}


