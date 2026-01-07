/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2026, the respective contributors.  All rights reserved. 
 */

using BH.Engine.Reflection;
using BH.Engine.UI;
using BH.Engine.Structure;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BH.UI.Base.Components
{
    public class CreatePanelCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/
        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.CreatePanel;
        public override Guid Id { get; protected set; } = new Guid("58E4B4F8-B3CC-40CA-B988-6EE127B061D9");

        public override string Name { get; protected set; } = "CreatePanel";

        public override string Category { get; protected set; } = "Structures";  // <-- This puts it in the "Structures" tab

        public override string Description { get; protected set; } = "Creates a structural Panel from a closed curve defining the outline, and any number of closed curves defining openings.";

        public override int GroupIndex { get; protected set; } = 1;  // Controls position within the tab (1 = primary)

        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreatePanelCaller() : base()
        {

            MethodInfo panelMethod = typeof(BH.Engine.Structure.Create)
                .GetMethods()
                .Where(m => m.Name == "Panel")
                .Where(m => m.GetParameters().FirstOrDefault()?.ParameterType == typeof(BH.oM.Geometry.ICurve))
                .FirstOrDefault();

            if (panelMethod != null)
            {
                SetItem(panelMethod);
            }
        }

        /*************************************/
    }
}
