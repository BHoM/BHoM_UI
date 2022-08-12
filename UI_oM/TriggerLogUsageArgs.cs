using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.oM.UI
{
    public class TriggerLogUsageArgs : EventArgs
    {
        public virtual object SelectedItem { get; set; }
        public virtual string CallerName { get; set; }
        public virtual string UIName { get; set; }
        public virtual string UIVersion { get; set; }
        public virtual Guid ComponentID { get; set; }
    }
}
