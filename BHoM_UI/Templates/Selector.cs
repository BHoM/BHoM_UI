using BH.Engine.Reflection;
using BH.oM.Reflection;
using BH.Engine.UI;
using BH.oM.Reflection.Attributes;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BH.oM.DataStructure;
using System.Windows.Forms;
using BH.Engine.Serialiser;

namespace BH.UI.Templates
{
    public class Selector<T> : ISelector
    {
        /*************************************/
        /**** Events                      ****/
        /*************************************/

        public event EventHandler<object> ItemSelected;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public Selector(IEnumerable<T> possibleItems, string key) 
        {
            m_Key = key;

            if (!m_ItemTreeStore.ContainsKey(key) || !m_ItemListStore.ContainsKey(key))
            {
                List<T> toKeep = possibleItems.Where(x => x.IIsToKeepInMenu()).ToList();
                Output<List<SearchItem>, Tree<T>> organisedMethod = Engine.UI.Compute.OrganiseItems(toKeep);
                m_ItemListStore[key] = organisedMethod.Item1;
                m_ItemTreeStore[key] = organisedMethod.Item2;
            }
        }

        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public void AddToMenu(ToolStripDropDown menu)
        {
            if (m_Menu_WinForm == null)
            {
                m_Menu_WinForm = new SelectorMenu_WinForm<T>(m_ItemListStore[m_Key], m_ItemTreeStore[m_Key]);
                m_Menu_WinForm.ItemSelected += M_Menu_ItemSelected;
            }

            m_Menu_WinForm.FillMenu(menu);
        }

        /*************************************/

        public void AddToMenu(System.Windows.Controls.ContextMenu menu)
        {
            if (m_Menu_Wpf == null)
            {
                m_Menu_Wpf = new SelectorMenu_Wpf<T>(m_ItemListStore[m_Key], m_ItemTreeStore[m_Key]);
                m_Menu_Wpf.ItemSelected += M_Menu_ItemSelected;
            }

            m_Menu_Wpf.FillMenu(menu);
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private void M_Menu_ItemSelected(object sender, T e)
        {
            ItemSelected?.Invoke(this, e);
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        protected SelectorMenu_WinForm<T> m_Menu_WinForm = null;
        protected SelectorMenu_Wpf<T> m_Menu_Wpf = null;

        protected string m_Key = "";
        protected static Dictionary<string, Tree<T>> m_ItemTreeStore = new Dictionary<string, Tree<T>>();
        protected static Dictionary<string, List<SearchItem>> m_ItemListStore = new Dictionary<string, List<SearchItem>>();


        /*************************************/
    }
}
