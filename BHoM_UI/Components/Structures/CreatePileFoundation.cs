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
    public class CreatePileFoundationCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/
        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.CreatePileFoundation;
        public override Guid Id { get; protected set; } = new Guid("527B0F8D-A806-4427-A275-8E7EFAC9AFDC");

        public override string Name { get; protected set; } = "CreatePileFoundation";

        public override string Category { get; protected set; } = "Structures";  // <-- This puts it in the "Structures" tab

        public override string Description { get; protected set; } = "Creates a typical PileFoundation based on the centre of the pile cap and a nominal Pile.";

        public override int GroupIndex { get; protected set; } = 1;  // Controls position within the tab (1 = primary)

        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreatePileFoundationCaller() : base()
        {

            MethodInfo pileFoundationMethod = typeof(BH.Engine.Structure.Create)
                .GetMethods()
                .Where(m => m.Name == "PileFoundation")
                .Where(m => m.GetParameters().FirstOrDefault()?.ParameterType == typeof(BH.oM.Structure.Elements.Pile))
                .FirstOrDefault();

            if (pileFoundationMethod != null)
            {
                SetItem(pileFoundationMethod);
            }
        }

        /*************************************/
    }
}
