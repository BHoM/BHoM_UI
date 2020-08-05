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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using BH.oM.Reflection.Attributes;
using BH.oM.UI;
using BH.Adapter;
using BH.oM.Reflection;

namespace BH.UI.Components
{
    public class GetInfoCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.GetInfo;

        public override Guid Id { get; protected set; } = new Guid("90D428FE-BC49-4944-8E22-C3180FDD6A96");

        public override int GroupIndex { get; protected set; } = 2;

        public override string Description { get; protected set; } = "Get information about the BHoM";

        public override string Category { get; protected set; } = "Engine";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public GetInfoCaller() : base(typeof(GetInfoCaller).GetMethod("GetInfo")) { }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        [Description("Get information about this installed version of the BHoM.")]
        [MultiOutput(0, "description", "Introduction to what the BHoM is all about.")]
        [MultiOutput(1, "version", "Version of the BHoM currently running in this interface.")]
        [MultiOutput(2, "installer", "If you have used the BHoM installer, this gives you information about its exact version.")]
        [MultiOutput(3, "libraries", "List of libraries (dlls) available in this version of the BHoM.")]
        [MultiOutput(4, "types", "List of objects types available in this version of the BHoM.")]
        [MultiOutput(5, "methods", "List of methods available in this version of the BHoM.")]
        [MultiOutput(6, "adapters", "List of adapters available in this version of the BHoM.")]
        [MultiOutput(7, "website", "BHoM website.")]
        [MultiOutput(8, "github", "GitHub organisation containing all our open-source toolkits.")]
        [MultiOutput(9, "wiki", "Need more details on how to use the BHoM or how to contribute? This is the place to go.")]
        public static Output<string, string, string, List<string>, List<Type>, List<MethodInfo>, List<Type>, string, string, string> GetInfo()
        {
            string description = "This is the Buildings and Habitats object Model. A collaborative computational development project for the built environment.\n\n"
                + "It is crafted as transdisciplinary, software-agnostic and office/region/country independent, and therefore would be nothing without our active community and wide range of contributors.\n\n"
                + "To find out more about our collective experiment go to https://bhom.xyz";

            string version = Engine.Reflection.Query.BHoMVersion();
            string installer = ""; //TODO: assign properly when that information is made available by the installer

            List<string> libraries = Engine.Reflection.Query.BHoMAssemblyList()
                .Select(x => x.GetName().Name)
                .OrderBy(x => x).ToList();

            List<Type> types = Engine.Reflection.Query.BHoMTypeList();
            List<MethodInfo> methods = Engine.Reflection.Query.BHoMMethodList();
            List<Type> adapters = Engine.Reflection.Query.AdapterTypeList().ToList();

            string website = "https://bhom.xyz/";
            string github = "https://github.com/BHoM";
            string wiki = "https://github.com/BHoM/documentation/wiki";

            return Engine.Reflection.Create.Output
            (
                description,
                version,
                installer,
                libraries,
                types,
                methods,
                adapters,
                website,
                github,
                wiki
            );
        }

        /*************************************/
    }
}
