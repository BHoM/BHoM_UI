/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2022, the respective contributors. All rights reserved.
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

using BH.Engine.Base;
using BH.Engine.Reflection;
using BH.oM.Test.Results;
using BH.oM.UI;
using BH.UI.Base;
using BH.UI.Base.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Test.UI
{
    public static partial class Helpers
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static Caller InstantiateCaller(SearchItem item)
        {
            try
            {
                Type type = item.CallerType;

                if (!m_ItemInstances.ContainsKey(type))
                    m_ItemInstances[type] = Activator.CreateInstance(type as Type) as Caller;


                Caller caller = m_ItemInstances[type].DeepClone();
                caller.SetItem(item.Item);

                return caller;
            }
            catch (Exception e)
            {
                Engine.Base.Compute.RecordError($"Failed to instantiate {item.Text}.\nError: {e.Message}");
                return null;
            }
        }

        /*************************************/

        public static Caller InstantiateCaller(Type type)
        {
            try
            {
                if (!m_ItemInstances.ContainsKey(type))
                    m_ItemInstances[type] = Activator.CreateInstance(type as Type) as Caller;

                return m_ItemInstances[type].DeepClone();
            }
            catch (Exception e)
            {
                Engine.Base.Compute.RecordError($"Failed to instantiate {type.IToText()}.\nError: {e.Message}");
                return null;
            }
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private static Dictionary<Type, Caller> m_ItemInstances = new Dictionary<Type, Caller>();

        /*************************************/
    }
}

