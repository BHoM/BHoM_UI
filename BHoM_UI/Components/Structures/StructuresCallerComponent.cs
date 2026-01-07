using BH.UI.Base;
using BH.UI.Base.Components;
using BH.UI.Grasshopper.Templates;
using Grasshopper.Kernel;

namespace BH.UI.Grasshopper.Components.Structures
{
    public abstract class StructuresCallerComponent : CallerComponent
    {
        public StructuresCallerComponent() : base()
        {
            // Override to make "Structures" the main tab
            Category = "Structures";           // Main ribbon tab
            SubCategory = Caller.Category;     // Panel within Structures tab
        }
    }

    // Then use it like this:
    public class CreateBarComponent : StructuresCallerComponent
    {
        public override Caller Caller { get; } = new CreateBarCaller();
    }
}
