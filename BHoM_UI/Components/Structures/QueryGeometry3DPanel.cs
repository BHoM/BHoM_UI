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
using BH.oM.Structure.Elements;

namespace BH.UI.Base.Components
{
    public class QueryGeometry3DPanelCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/
        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Geo3DBar;
        public override Guid Id { get; protected set; } = new Guid("B4B378A9-A2B5-4896-9287-C0C07AA33931");

        public override string Name { get; protected set; } = "Geometry3DPanel";

        public override string Category { get; protected set; } = "Structures";  // <-- This puts it in the "Structures" tab

        public override string Description { get; protected set; } = "Gets a CompositeGeometry made of the boundary surfaces of the Panel envelope, or only its central Surface.";

        public override int GroupIndex { get; protected set; } = 2;  // Controls position within the tab (1 = primary)

        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public QueryGeometry3DPanelCaller() : base()
        {

            MethodInfo geometryMethod = typeof(BH.Engine.Structure.Query)
                .GetMethods()
                .Where(m => m.Name == "Geometry3D")
                .Where(m => m.GetParameters().FirstOrDefault()?.ParameterType == typeof(Panel))
                .FirstOrDefault();

            if (geometryMethod != null)
            {
                SetItem(geometryMethod);
            }
        }

        /*************************************/
    }
}
