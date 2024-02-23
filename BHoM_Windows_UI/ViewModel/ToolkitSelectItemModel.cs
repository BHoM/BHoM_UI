using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BH.oM.UI;

namespace BH.UI.Base.Windows.ViewModel
{
    internal class ToolkitSelectItemModel : INotifyPropertyChanged
    {
        public ToolkitSelectItemModel(ToolkitSelectItem item)
        {
            Toolkit = item.Toolkit;
            Include = item.Include;
        }

        public ToolkitSelectItem ToItem()
        {
            return new ToolkitSelectItem()
            {
                Toolkit = this.Toolkit,
                Include = this.Include,
            };
        }

        private bool m_Include;

        public virtual string Toolkit { get; set; }
        public virtual bool Include
        { 
            get
            {
                return m_Include;
            }
            set
            {
                m_Include = value;
                NotifyPropertyChanged();
            }    
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
