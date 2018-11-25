using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using BH.oM.DataStructure;
using System.Windows;

namespace BH.UI.Templates
{
    public class SelectorMenu_Wpf<T> : SelectorMenu<T, ContextMenu>
    {
        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public SelectorMenu_Wpf(List<Tuple<string, T>> itemList, Tree<T> itemTree) : base(itemList, itemTree) { }


        /*************************************/
        /**** Override Methods            ****/
        /*************************************/

        protected override void AddTree(ContextMenu menu, Tree<T> itemTree)
        {
            menu.Items.Add(new Separator());

            MenuItem treeMenu = new MenuItem { Header = itemTree.Name };
            menu.Items.Add(treeMenu);
            foreach (Tree<T> childTree in itemTree.Children.Values.OrderBy(x => x.Name))
                AppendMenuTree(childTree, treeMenu);
        }

        /*************************************/

        protected override void AddSearchBox(ContextMenu menu, List<Tuple<string, T>> itemList)
        {
            m_ItemList = itemList;
            menu.SizeChanged += Menu_SizeChanged;
            menu.Items.Add(new Separator());

            MenuItem label = CreateMenuItem("Search");
            label.FontWeight = FontWeights.Bold;
            menu.Items.Add(label);

            m_Menu = menu;
            m_SearchBox = new TextBox { Text = "", HorizontalAlignment = HorizontalAlignment.Stretch };
            m_SearchBox.TextChanged += Search_TextChanged;
            menu.Items.Add(m_SearchBox);

            menu.Items.Add(new Separator());
        }

        /*************************************/
        /**** Protected Methods           ****/
        /*************************************/

        protected void AppendMenuTree(Tree<T> tree, MenuItem menu)
        {
            if (tree.Children.Count > 0)
            {
                MenuItem treeMenu = CreateMenuItem(tree.Name);
                menu.Items.Add(treeMenu);
                foreach (Tree<T> childTree in tree.Children.Values.OrderBy(x => x.Name))
                    AppendMenuTree(childTree, treeMenu);
            }
            else
            {
                T method = tree.Value;
                MenuItem methodItem = CreateMenuItem(tree.Name, Item_Click);
                menu.Items.Add(methodItem);
                m_ItemLinks[methodItem] = tree.Value;
            }
        }

        /*************************************/

        protected MenuItem CreateMenuItem(string text, EventHandler click = null, bool enabled = true, bool @checked = false)
        {
            MenuItem item = new MenuItem { Header = text, IsCheckable = @checked };
            if (click != null)
                item.Click += Item_Click;

            return item;
        }

        /*************************************/

        protected void Item_Click(object sender, EventArgs e)
        {
            MenuItem item = (MenuItem)sender;
            if (m_ItemLinks.ContainsKey(item))
                ReturnSelectedItem(m_ItemLinks[item]);
        }

        /*************************************/

        protected void Search_TextChanged(object sender, EventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box == null) return;

            // Clear the old items
            foreach (MenuItem item in m_SearchResultItems)
                m_Menu.Items.Remove(item);
            m_SearchResultItems.Clear();

            // Add the new ones
            string text = box.Text.ToLower();
            string[] parts = text.Split(' ');
            foreach (Tuple<string, T> tree in m_ItemList.Where(x => parts.All(y => x.Item1.ToLower().Contains(y))).Take(12).OrderBy(x => x.Item1))
            {
                MenuItem methodItem = CreateMenuItem(tree.Item1, Item_Click);
                m_Menu.Items.Add(methodItem);
                m_SearchResultItems.Add(methodItem);
                m_ItemLinks[methodItem] = tree.Item2;
            }
        }

        /*************************************/

        private void Menu_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // This is needed to force the TextBox to fill the menu (streatch property doesn't seem to do anything and I don't want to spend time finding out why)
            if (Double.IsNaN(m_SearchBox.Width) || m_SearchBox.Width == 0)
                m_SearchBox.Width = m_Menu.Items.OfType<FrameworkElement>().Where(x => x != null).Select(x => x.ActualWidth).Aggregate((a, b) => Math.Max(a, b));
        }


        /*************************************/
        /**** Protected Fields            ****/
        /*************************************/

        protected ContextMenu m_Menu;
        protected TextBox m_SearchBox;
        protected Dictionary<MenuItem, T> m_ItemLinks = new Dictionary<MenuItem, T>();
        protected List<MenuItem> m_SearchResultItems = new List<MenuItem>();

        /*************************************/
    }
}
