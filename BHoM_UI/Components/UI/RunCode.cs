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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.CodeDom.Compiler;
using BH.Engine.Reflection;
using System.Windows.Forms;
using System.Windows.Controls;

namespace BH.UI.Base.Components
{
    public class RunCodeCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.RunCode;

        public override Guid Id { get; protected set; } = new Guid("E2C57C77-6583-493E-96C7-8894FC109D58");

        public override string Name { get; protected set; } = "RunCode";

        public override string Category { get; protected set; } = "UI";

        public override string Description { get; protected set; } = "Allow to load code and edit it before executing it like any other component.";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public RunCodeCaller() : base()
        {
            SetPossibleItems(Engine.UI.Query.EngineItems());
        }


        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        public override void AddToMenu(ToolStripDropDown menu)
        {
            menu.Items.Add(new ToolStripMenuItem("Edit Code", null, (sender, e) => OpenEditor()));
            menu.Items.Add(new ToolStripSeparator());

            base.AddToMenu(menu);
        }

        /*************************************/

        public override void AddToMenu(System.Windows.Controls.ContextMenu menu)
        {
            if (menu.Items.OfType<System.Windows.Controls.MenuItem>().All(x => x.Header.ToString() != "Edit Code"))
            {
                System.Windows.Controls.MenuItem item = new System.Windows.Controls.MenuItem { Header = "EditCode" };
                item.Click += (sender, e) => OpenEditor();
                menu.Items.Add(item);
                menu.Items.Add(new Separator());
            }

            base.AddToMenu(menu);
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private void OpenEditor()
        {
            Windows.CodeEditor editor = new Windows.CodeEditor();
            editor.MethodRebuilt += (s, method) =>
            {
                if (method != null)
                    SetItem(method, true, false);
            };

            editor.Closed += (s, e) =>
            {
                m_Code = editor.GetCode();
                m_RefFiles = editor.GetReferenceFiles();
            };

            if (!string.IsNullOrEmpty(m_Code))
                editor.SetCode(m_Code, m_RefFiles);
            else if (SelectedItem is MethodInfo)
                editor.SetMethod(SelectedItem as MethodInfo);
            else
                editor.SetCode(CodeTemplate(), BasicReferences());

            editor.Show();
        }

        /*************************************/

        private string CodeTemplate()
        {
            return "using System;\n" 
                + "using System.Collections.Generic;\n"
                + "using System.Linq;\n"
                + "using BH.oM.Base;\n"
                + "using BH.Engine.Reflection;\n"
                + "using BH.oM.Geometry;\n"
                + "\n"
                + "namespace BH.Engine.Custom\n"
                + "{\n"
                + "\tpublic static partial class Compute\n"
                + "\t{\n"
                + "\t\tpublic static string NewMethod()\n"
                + "\t\t{\n"
                + "\t\t\treturn \"test\";\n"
                + "\t\t}\n"
                + "\t}\n"
                + "}\n";
        }

        /*************************************/

        private List<string> BasicReferences()
        {
            List<string> references = new List<string>
            {
                "C:\\ProgramData\\BHoM\\Assemblies\\Geometry_Engine.dll",
                "C:\\ProgramData\\BHoM\\Assemblies\\Reflection_Engine.dll"
            };

            List<string> assemblyNames = references.Select(x => AssemblyName.GetAssemblyName(x).FullName).ToList();
            return references.Concat(BH.Engine.Reflection.Query.UsedAssemblies(assemblyNames, false, true)).Distinct().ToList();
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private string m_Code = "";
        private List<string> m_RefFiles = new List<string>();

        /*************************************/
    }
}

