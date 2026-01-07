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
    public class CreateOpeningCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/
        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.CreateOpening;
        public override Guid Id { get; protected set; } = new Guid("A713FABF-C0E8-4FB3-9D75-8FF99DEBBE50");

        public override string Name { get; protected set; } = "CreateOpening";

        public override string Category { get; protected set; } = "Structures";  // <-- This puts it in the "Structures" tab

        public override string Description { get; protected set; } = "Creates a structural Opening from a closed curve.";

        public override int GroupIndex { get; protected set; } = 1;  // Controls position within the tab (1 = primary)

        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateOpeningCaller() : base()
        {

            MethodInfo openingMethod = typeof(BH.Engine.Structure.Create)
                .GetMethods()
                .Where(m => m.Name == "Opening")
                .Where(m => m.GetParameters().FirstOrDefault()?.ParameterType == typeof(BH.oM.Geometry.ICurve))
                .FirstOrDefault();

            if (openingMethod != null)
            {
                SetItem(openingMethod);
            }
        }

        /*************************************/
    }
}
