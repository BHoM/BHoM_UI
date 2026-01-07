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
    public class CreateBarCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.CreateBar;
        public override Guid Id { get; protected set; } = new Guid("A5F01E77-07B6-4C3C-A9E7-D41B0374D1D2");

        public override string Name { get; protected set; } = "CreateBar";

        public override string Category { get; protected set; } = "Structures";  // <-- This puts it in the "Structures" tab

        public override string Description { get; protected set; } = "Creates a structural Bar element from start and end nodes";

        public override int GroupIndex { get; protected set; } = 1;  // Controls position within the tab (1 = primary)

        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateBarCaller() : base()
        {
            // Directly reference the Bar method from BH.Engine.Structure.Create
            // This is the method:  public static Bar Bar(Line line, ISectionProperty sectionProperty = null, double orientationAngle = 0, BarRelease release = null, BarFEAType feaType = BarFEAType.Flexural, string name = "")

            MethodInfo barMethod = typeof(BH.Engine.Structure.Create)
                .GetMethods()
                .Where(m => m.Name == "Bar")
                .Where(m => m.GetParameters().FirstOrDefault()?.ParameterType == typeof(BH.oM.Geometry.Line))
                .Where(m => m.GetParameters().Any(p => p.ParameterType == typeof(double) && p.Name == "orientationAngle"))
                .FirstOrDefault();

            if (barMethod != null)
            {
                SetItem(barMethod);
            }
        }

        /*************************************/
    }
}
