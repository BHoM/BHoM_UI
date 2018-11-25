using BH.oM.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Templates
{
    public abstract class SelectorMenu<T, M>
    {
        /*************************************/
        /**** Public Events               ****/
        /*************************************/

        public event EventHandler<T> ItemSelected;


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public SelectorMenu(List<Tuple<string, T>> itemList, Tree<T> itemTree)
        {
            m_ItemList = itemList;
            m_ItemTree = itemTree;
        }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public void FillMenu(M menu)
        {
            AddTree(menu, m_ItemTree);
            AddSearchBox(menu, m_ItemList);
        }


        /*************************************/
        /**** Protected Methods           ****/
        /*************************************/

        protected abstract void AddTree(M menu, Tree<T> itemTree);

        /*************************************/

        protected abstract void AddSearchBox(M menu, List<Tuple<string, T>> itemList);

        /*************************************/

        protected void ReturnSelectedItem(T item)
        {
            if (ItemSelected != null)
                ItemSelected(this, item);
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        protected List<Tuple<string, T>> m_ItemList = new List<Tuple<string, T>>();

        protected Tree<T> m_ItemTree = new Tree<T>();


        /*************************************/
    }
}
