using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Global
{
    public interface ISearchMenu
    {
        event EventHandler<MethodInfo> ItemSelected; 

        Dictionary<string, MethodInfo> PossibleItems { get; set; }

        bool SetParent(object parent);
    }
}
