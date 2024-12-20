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
using BH.oM.Base.Debugging;
using BH.oM.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Test.UI
{
    public class DummyAccessor : IDataAccessor
    {
        /***************************************************/
        /**** Properties                                ****/
        /***************************************************/

        public virtual List<object> Outputs { get; set; } = new List<object>();

        /***************************************************/
        /**** Interface Methods                         ****/
        /***************************************************/

        public List<object> GetAllData(int index)
        {
            return new List<object>();
        }

        /***************************************************/

        public T GetDataItem<T>(int index)
        {
            return GetDummyObject<T>();
        }

        /***************************************************/

        public List<T> GetDataList<T>(int index)
        {
            T item = GetDummyObject<T>();
            return new List<T> { item, item };
        }

        /***************************************************/

        public List<List<T>> GetDataTree<T>(int index)
        {
            T item = GetDummyObject<T>();

            return new List<List<T>>
            {
                new List<T> { item, item },
                new List<T> { item }
            };
        }

        /***************************************************/

        public bool SetDataItem<T>(int index, T data)
        {
            while (Outputs.Count <= index)
                Outputs.Add(null);

            Outputs[index] = data;
            return true;
        }

        /***************************************************/

        public bool SetDataList<T>(int index, IEnumerable<T> data)
        {
            return SetDataItem(index, data);
        }

        /***************************************************/

        public bool SetDataTree<T>(int index, IEnumerable<IEnumerable<T>> data)
        {
            return SetDataItem(index, data);
        }


        /***************************************************/
        /**** Private Methods                           ****/
        /***************************************************/

        private T GetDummyObject<T>()
        {
            Type type = typeof(T);

            if (!m_DummyObjects.ContainsKey(type))
                m_DummyObjects[type] = Engine.Test.Compute.DummyObject(type);

            return (T)m_DummyObjects[type];
        }


        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        Dictionary<Type, object> m_DummyObjects = new Dictionary<Type, object>();

        /***************************************************/
    }
}





