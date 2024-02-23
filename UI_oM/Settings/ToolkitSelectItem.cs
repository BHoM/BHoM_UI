using BH.oM.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.UI
{
    [Description("For each toolkit in BHoM, allocate a boolean flag for whether it should appear in search results or not.")]
    public class ToolkitSelectItem : BHoMObject
    {
        [Description("The name of the toolkit that this item refers to.")]
        public virtual string Toolkit { get; set; }

        [Description("Determine whether to include the toolkit in search results or not.")]
        public virtual bool Include { get; set; } = true;
    }
}