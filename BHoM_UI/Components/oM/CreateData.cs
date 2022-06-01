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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using BH.oM.UI;
using BH.oM.Data.Collections;
using BH.Engine.Reflection;
using BH.Engine.Data;
using BH.Engine.Serialiser;
using System.Windows.Forms;
using BH.oM.Base;
using System.Windows;

namespace BH.UI.Base.Components
{
    public class CreateDataCaller : MultiChoiceCaller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.BHoM_Data;

        public override Guid Id { get; protected set; } = new Guid("B7325A7F-0465-45A4-9537-24A96A5A2FEC");

        public override string Category { get; protected set; } = "oM";

        public override string Name { get; protected set; } = "CreateData";

        public override string Description { get; protected set; } = "Creates a BhoM object from the reference datasets";

        public string FileName
        {
            get
            {
                return SelectedItem as string;
            }
            protected set
            {
                SelectedItem = value;
            }
        }


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateDataCaller() : base()
        {
            SetPossibleItems(Engine.UI.Query.LibraryItems());

            InputParams = new List<ParamInfo>() { new ParamInfo { DataType = typeof(int), Kind = ParamKind.Input, Name = "index", Description = "index of the data reference" } };
            OutputParams = new List<ParamInfo>() { new ParamInfo { DataType = typeof(IObject), Kind = ParamKind.Output, Name = "data", Description = "selected reference data" } };
        }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        protected override void SetComponentDetails()
        {
            if (FileName != null)
            {
                Choices = BH.Engine.Library.Query.Library(FileName).ToList<object>();
                Name = FileName.Split(new char[] { '\\' }).Last();
                Description = BH.Engine.Library.Query.SourceAndDisclaimer(FileName);
                if (m_Menu != null)
                    AddSourceLinkToMenu(m_Menu as dynamic);
            }
        }

        /*************************************/

        public override List<string> GetChoiceNames()
        {
            return Choices.Cast<IBHoMObject>().Select(x => x.Name).ToList();
        }

        /*************************************/

        public override void AddToMenu(object menu)
        {
            base.AddToMenu(menu);
            m_Menu = menu;  //Store the menu to be able to add source link in case of presistent menu
            AddSourceLinkToMenu(menu as dynamic);  //Try add link now for volatile menu
        }

        /*************************************/
        /**** Private methods             ****/
        /*************************************/

        protected override bool RestoreItem(object selectedItem, List<ParamInfo> inputParams, List<ParamInfo> outputParams)
        {
            if (selectedItem is string)
                selectedItem = Engine.Library.Query.ValidatePath(selectedItem as string);
            return base.RestoreItem(selectedItem, inputParams, outputParams);
        }

        /*************************************/

        private void AddSourceLinkToMenu(ToolStripDropDown menu)
        {
            if (FileName != null && !menu.Items.ContainsKey("SourceLink"))
            {
                string sourceLink = Engine.Library.Query.Source(FileName)?.First()?.SourceLink;
                if (!string.IsNullOrWhiteSpace(sourceLink))
                {
                    ToolStripLabel linkLabel = new ToolStripLabel("Source link");
                    linkLabel.Name = "SourceLink";
                    linkLabel.IsLink = true;
                    linkLabel.Click += (object sender, EventArgs e) => System.Diagnostics.Process.Start(sourceLink);
                    menu.Items.Add(linkLabel);
                }
            }
        }

        /*************************************/

        private void AddSourceLinkToMenu(System.Windows.Controls.ContextMenu menu)
        {
            if (FileName != null && !menu.Items.SourceCollection.OfType<MenuItem>().Any(x => x.Name == "SourceLink"))
            {
                string sourceLink = Engine.Library.Query.Source(FileName)?.First()?.SourceLink;
                if (!string.IsNullOrWhiteSpace(sourceLink))
                {
                    System.Windows.Controls.MenuItem linkLabel = new System.Windows.Controls.MenuItem();
                    linkLabel.Header = "Source Link";
                    linkLabel.Name = "SourceLink";
                    linkLabel.Click += (object sender, RoutedEventArgs e) => System.Diagnostics.Process.Start(sourceLink);
                    menu.Items.Add(linkLabel);
                }
            }
        }

        /*************************************/

        private void AddSourceLinkToMenu(object menu)
        {
            //fallback method
        }

        /*************************************/
        /**** Private fields              ****/
        /*************************************/

        private object m_Menu = null;

        /*************************************/
    }
}



