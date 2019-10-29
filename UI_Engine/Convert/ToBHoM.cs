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

using BH.Engine.Reflection;
using BH.oM.Reflection.Attributes;
using BH.oM.UI;
using System.Reflection;

namespace BH.Engine.UI
{
    public static partial class Convert
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        [Input("property", "The system property to convert to bhom")]
        [Output("parameter", "The bhom parameter used in the bhom abstract syntax")]
        public static oM.UI.ParamInfo ToBHoM(this PropertyInfo property)
        {
            return new ParamInfo
            {
                Name = property.Name,
                DataType = property.PropertyType,
                Description = property.PropertyType.IDescription(),
                Kind = ParamKind.Input
            };
        }

        /*************************************/
    }
}
