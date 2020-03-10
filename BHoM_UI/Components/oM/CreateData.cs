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
using BH.oM.UI;
using BH.oM.Data.Collections;
using BH.Engine.Reflection;
using BH.Engine.Data;
using BH.Engine.Serialiser;
using System.Windows.Forms;
using BH.oM.Base;

namespace BH.UI.Components
{
    public class CreateDataCaller : Templates.MultiChoiceCaller
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

        public override bool SetItem(object item)
        {
            if (!base.SetItem(item))
                return false;

            if (FileName == null)
                return false;

            Choices = BH.Engine.Library.Query.Library(FileName).ToList<object>();
            Name = FileName.Split(new char[] { '\\' }).Last();
            Description = BH.Engine.Library.Query.SourceAndDisclaimer(FileName);
            return true;
        }

        /*************************************/

        public override List<string> GetChoiceNames()
        {
            return Choices.Cast<IBHoMObject>().Select(x => x.Name).ToList();
        }

        /*************************************/

    }
}

