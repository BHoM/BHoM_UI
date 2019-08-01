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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Templates
{
    public abstract class DataAccessor
    {
        /*************************************/
        /**** Input Getter Methods        ****/
        /*************************************/

        public abstract T GetDataItem<T>(int index);

        /*************************************/

        public abstract List<T> GetDataList<T>(int index);

        /*************************************/

        public abstract T[] GetDataArray<T>(int index);

        /*************************************/

        public abstract List<List<T>> GetDataTree<T>(int index);


        /*************************************/
        /**** Output Setter Methods       ****/
        /*************************************/

        public abstract bool SetDataItem<T>(int index, T data);

        /*************************************/

        public abstract bool SetDataList<T>(int index, IEnumerable<T> data);

        /*************************************/

        public abstract bool SetDataTree<T>(int index, IEnumerable<IEnumerable<T>> data);

        /*************************************/
    }
}
