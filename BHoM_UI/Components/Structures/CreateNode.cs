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
    public class CreateNodeCaller : Caller
    {
        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.CreateNode;
        public override Guid Id { get; protected set; } = new Guid("F2E6239F-E8DC-4A40-9C1B-153E0D95EED0");

        public override string Name { get; protected set; } = "CreateNode";

        public override string Category { get; protected set; } = "Structures";  // <-- This puts it in the "Structures" tab

        public override string Description { get; protected set; } = "Creates a Node from a Cartesian coordinate system. The position of the Node will be the Orgin, and the Orientation of the node will match the axes of the Coordinate system.";

        public override int GroupIndex { get; protected set; } = 1;  // Controls position within the tab (1 = primary)

        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateNodeCaller() : base()
        {

            MethodInfo nodeMethod = typeof(BH.Engine.Structure.Create)
                .GetMethods()
                .Where(m => m.Name == "Node")
                .FirstOrDefault();

            if (nodeMethod != null)
            {
                SetItem(nodeMethod);
            }
        }

        /*************************************/
    }
}
