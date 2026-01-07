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
using BH.oM.Geometry;

namespace BH.UI.Base.Components
{
    public class CreateTruss2DCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/
        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.CreateTruss2D;
        public override Guid Id { get; protected set; } = new Guid("42FF0FD0-FEE9-4F6F-94DD-D359F7D2A620");

        public override string Name { get; protected set; } = "CreateTruss2D";

        public override string Category { get; protected set; } = "Structures";  // <-- This puts it in the "Structures" tab

        public override string Description { get; protected set; } = "Constructs a 2D truss (planar or warping single-surface truss in any orientation) using input curves for chords, with target points to align bracing.";

        public override int GroupIndex { get; protected set; } = 1;  // Controls position within the tab (1 = primary)

        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateTruss2DCaller() : base()
        {

            MethodInfo trussMethod = typeof(BH.Engine.Structure.Create)
                .GetMethods()
                .Where(m => m.Name == "Truss2D")
                .Where(m => m.GetParameters().FirstOrDefault()?.ParameterType == typeof(ICurve))
                .FirstOrDefault();

            if (trussMethod != null)
            {
                SetItem(trussMethod);
            }
            /*************************************/
        }
    }
}
