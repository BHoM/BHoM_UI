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
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static string IToText(this IParamUpdate update)
        {
            return ToText(update as dynamic);
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        private static string ToText(ParamAdded update)
        {
            if (update != null)
                return "Parameter " + update.Name + " added";
            else
                return "Undefined param added";
        }

        /*************************************/

        private static string ToText(ParamMoved update)
        {
            if (update != null)
                return "Param " + update.Name + " moved to " + update.Index;
            else
                return "Undefined param moved";
        }

        /*************************************/

        private static string ToText(ParamRemoved update)
        {
            if (update != null)
                return "Parameter " + update.Name + " removed";
            else
                return "Undefined param removed";
        }

        /*************************************/

        private static string ToText(ParamUpdated update)
        {
            if (update != null && update.Param != null && update.OldParam != null)
            {
                string newTypeText = (update.Param.DataType != null) ? update.Param.DataType.ToText(true) : "unknow";
                string oldTypeText = (update.OldParam.DataType != null) ? update.OldParam.DataType.ToText(true) : "unknow";
                return "Parameter " + update.OldParam.Name + " of type " + oldTypeText + " replaced with parameter " + update.Name + " of type " + newTypeText;
            }
            else
                return "Undefined param replaced";
        }

        /*************************************/

        private static string ToText(IParamUpdate update)
        {
            return update.ToString();
        }

        /*************************************/
    }
}


