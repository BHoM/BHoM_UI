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
using System.ComponentModel;
using BH.oM.Reflection.Attributes;
using BH.oM.UI;
using BH.Adapter;
using BH.oM.Reflection;

namespace BH.UI.Components
{
    public class GetInfoCaller : MethodCaller
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
        [MultiOutput(3, "toolkits", "List of toolkits available in this version of the BHoM.")]
        [MultiOutput(4, "types", "List of objects types available in this version of the BHoM.")]
        [MultiOutput(5, "methods", "List of methods available in the version of the BHoM.")]
        [MultiOutput(6, "adapters", "List of adapters available in the version of the BHoM.")]
        [MultiOutput(7, "website", "BHoM website.")]
        [MultiOutput(8, "github", "GitHub organisation containing all our open-source toolkits.")]
        [MultiOutput(9, "wiki", "Need more details on how to use the BHoM or how to contribute? This is the place to go.")]
        public static Output<string, string, string, List<string>, List<Type>, List<MethodInfo>, List<Type>, string, string, string> GetInfo()
        {
            string description = "This is the BHoM (Buildings and Habitats object Model), a collaborative computational development project for the built environment. "
                + "It is a collective effort to share code and standardise the data that we use to design, everyday – across all activities and all disciplines.\n\n"
                + "It is not an attempt to standardise exact processes – these must be flexible...\n\n"
                + "It is also not an attempt to standardise the software we use...\n\n"
                + "But in standardising the data but not the data-base, we provide great opportunities for efficiencies, for collaboration and most of all, to change the way we work.\n\n"
                + "It is crafted as transdisciplinary, software-agnostic and office/region/country-invariant, and therefore would be nothing without our active community and wide range of contributors.\n\n"
                + "The whole BHoM project uses an open-source model for project architecture, co-creation and planning. So explore, experiment and contribute to both the source code and the wiki. "
                + "Sharing and building our code together in this open-source type approach means we can feed off and pool our disparate knowledge, experience and expertise towards a common goal – better design.\n\n"
                + "Also, as well as creating a common language of BHoM objects - the large number of repositories contain a variety of different plugins and code to operate on BHoM objects and link the BHoM with our favourite software and tools. "
                + "Much of the core code is written in C#. But we also have code in JavaScript, C++, Python and visual programming languages such as Grasshopper User Objects and Dynamo Custom Nodes all forming part of the BHoM.";

            string version = Engine.Reflection.Query.BHoMVersion();
            string installer = ""; //TODO: assign properly when that information is made available by the installer

            List<string> toolkits = Engine.Reflection.Query.BHoMAssemblyList()
                .Select(x => x.GetName().Name.Split(new char[] { '_' }).First())
                .Distinct().ToList();

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
                toolkits,
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

