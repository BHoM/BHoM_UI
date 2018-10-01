using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BH.UI.Global
{
    public class SearchMenu_Wpf : ISearchMenu
    {
        /*************************************/
        /**** Events                      ****/
        /*************************************/

        public event EventHandler<MethodInfo> ItemSelected;


        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public Dictionary<string, MethodInfo> PossibleItems { get; set; }


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public bool SetParent(object parent)
        {
            Panel container = parent as Panel;
            if (container == null)
                return false;

            if (m_Popup == null)
            {
                m_Popup = new Popup
                {
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    Placement = PlacementMode.MousePoint
                };

                Grid grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                m_Popup.Child = grid;

                m_SearchTextBox = new TextBox { MinWidth = 200, MinHeight = 24, MaxLines = 1 };
                m_SearchTextBox.TextChanged += TextBox_TextChanged;
                m_SearchTextBox.LostFocus += M_SearchTextBox_LostFocus;
                m_SearchTextBox.Loaded += M_SearchTextBox_Loaded;
                m_SearchTextBox.Initialized += M_SearchTextBox_Initialized;
                Grid.SetRow(m_SearchTextBox, 0);
                grid.Children.Add(m_SearchTextBox);

                m_SearchResultGrid = new Grid();
                m_SearchResultGrid.ColumnDefinitions.Add(new ColumnDefinition());
                Grid.SetRow(m_SearchResultGrid, 1);
                grid.Children.Add(m_SearchResultGrid);

                container.Children.Add(m_Popup);
            }

            m_SearchTextBox.Text = "";
            m_SearchResultGrid.Children.Clear();
            m_Popup.IsOpen = true;
            m_SearchTextBox.Focus();

            return true;
        }

        

        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        private void M_SearchTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            m_SearchTextBox.Focus();
        }

        /*************************************/

        private void M_SearchTextBox_Initialized(object sender, EventArgs e)
        {
            m_SearchTextBox.Focus();
        }

        /*************************************/

        private void M_SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            m_Popup.IsOpen = false;
        }

        /*************************************/

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = m_SearchTextBox.Text.Trim().ToLower();
            string[] parts = text.Split(' ');

            List<KeyValuePair<string, MethodInfo>> hits = PossibleItems.Where(x => parts.All(y => x.Key.Substring(0, x.Key.IndexOf('(') + 1).ToLower().Contains(y))).Take(20).OrderBy(x => x.Key).ToList();

            m_SearchResultGrid.Children.Clear();
            m_SearchResultGrid.RowDefinitions.Clear();
            for (int i = 0; i < hits.Count; i++)
            {
                m_SearchResultGrid.RowDefinitions.Add(new RowDefinition());
                Label label = new Label { Content = hits[i].Key, Background = System.Windows.Media.Brushes.White };
                label.MouseUp += Label_MouseUp;
                Grid.SetRow(label, i);
                m_SearchResultGrid.Children.Add(label);
            }

        }

        /*************************************/

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string methodName = ((Label)sender).Content as string;
            m_Popup.IsOpen = false;

            ItemSelected?.Invoke(this, PossibleItems[methodName]);
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private static Popup m_Popup = null;
        private static TextBox m_SearchTextBox = null;
        private static Grid m_SearchResultGrid = null;

        /*************************************/
    }
}
