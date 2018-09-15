using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BH.UI.Templates
{
    public interface ISelector
    {
        event EventHandler<object> ItemSelected;

        void AddToMenu(ToolStripDropDown menu);

        void AddToMenu(System.Windows.Controls.ContextMenu menu);

        string Write();

        bool Read(string json);

        object GetSelectedItem();
    }
}
