/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2020, the respective contributors. All rights reserved.
 *
 * Each contributor holds copyright over their respective contributions.
 * The project versioning (Git) records all such contribution source information.
 *                                           
 *                                                                              
 * The BHoM is free software: you can redistribute it and/or modify         
 * it under the terms of the GNU Lesser General Public License as published by  
 * the Free Software Foundation, either version 3.0 of the License, or          
 * (at your option) any later version.                                          
 *                                                                              
 * The BHoM is distributed in the hope that it will be useful,              
 * but WITHOUT ANY WARRANTY; without even the implied warranty of               
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                 
 * GNU Lesser General Public License for more details.                          
 *                                                                            
 * You should have received a copy of the GNU Lesser General Public License     
 * along with this code. If not, see <https://www.gnu.org/licenses/lgpl-3.0.html>.      
 */

using BH.Engine.UI;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BH.UI.Global
{
    public class SearchMenu_Wpf : SearchMenu
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public override bool SetParent(object parent)
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

                m_SearchTextBox = new TextBox { MinWidth = m_MinWidth, MinHeight = 24, MaxLines = 1 };
                m_SearchTextBox.TextChanged += (o, e) => RefreshSearchResults(PossibleItems.Hits(m_SearchTextBox.Text, NbHits, HitsOnEmptySearch));
                m_SearchTextBox.LostFocus += M_SearchTextBox_LostFocus;
                m_SearchTextBox.Loaded += M_SearchTextBox_Loaded;
                m_SearchTextBox.Initialized += M_SearchTextBox_Initialized;
                Grid.SetRow(m_SearchTextBox, 0);
                grid.Children.Add(m_SearchTextBox);

                m_SearchResultGrid = new Grid { Background = System.Windows.Media.Brushes.White };
                m_SearchResultGrid.ColumnDefinitions.Add(new ColumnDefinition());
                Grid.SetRow(m_SearchResultGrid, 1);
                grid.Children.Add(m_SearchResultGrid);

                container.Children.Add(m_Popup);
                m_SearchTextBox.PreviewKeyDown += M_SearchTextBox_PreviewKeyDown;
            }

            m_Popup.Unloaded += M_Popup_Unloaded;

            m_SearchTextBox.Text = "";
            m_SearchResultGrid.Children.Clear();
            m_Popup.IsOpen = true;
            m_SearchTextBox.Focus();

            return true;
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private void M_Popup_Unloaded(object sender, RoutedEventArgs e)
        {
            m_Popup = null;
            NotifyDispose();

        }

        /*************************************/

        private void M_SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            switch (e.Key)
            {
                case Key.Up:
                    if (--m_selected < 0)
                        m_selected = m_hits - 1;
                    break;
                case Key.Down:
                    m_selected = (m_selected + 1) % m_hits;
                    break;
                case Key.Enter:
                    List<SearchItem> hits = PossibleItems.Hits(m_SearchTextBox.Text, NbHits, HitsOnEmptySearch);
                    if (m_selected < hits.Count)
                        NotifySelection(hits[m_selected], new BH.oM.Geometry.Point { X = m_Popup.PlacementRectangle.X, Y = m_Popup.PlacementRectangle.Y });
                    m_Popup.IsOpen = false;
                    return;
                case Key.Escape:
                    m_Popup.IsOpen = false;
                    return;
                default:
                    e.Handled = false;
                    return;
            }
            foreach (Label element in m_SearchResultGrid.Children.OfType<Label>())
            {
                int i = Grid.GetRow(element);
                element.Background = (i == m_selected) ? System.Windows.SystemColors.HighlightBrush : System.Windows.Media.Brushes.White;
            }
        }

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
            NotifySelection(null);
        }

        /*************************************/

        protected override void RefreshSearchResults(List<SearchItem> hits)
        {
            m_selected = 0;
            m_hits = hits.Count;

            m_SearchResultGrid.Children.Clear();
            m_SearchResultGrid.RowDefinitions.Clear();

            if (hits.Count == 0)
            {
                Label label = new Label { Content = "No result found...", Background = System.Windows.Media.Brushes.White, Width = m_MinWidth, HorizontalContentAlignment = HorizontalAlignment.Center };
                Grid.SetRow(label, 0);
                m_SearchResultGrid.Children.Add(label);
            }
            else
            {
                m_SearchResultGrid.ColumnDefinitions.Add(new ColumnDefinition());
                m_SearchResultGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (int i = 0; i < hits.Count; i++)
            {
                SearchItem hit = hits[i];
                m_SearchResultGrid.RowDefinitions.Add(new RowDefinition());

                System.Windows.Controls.Image icon = new System.Windows.Controls.Image { Source = GetImage(hit.Icon) };
                Grid.SetRow(icon, i);
                m_SearchResultGrid.Children.Add(icon);

                Label label = new Label { Content = hit.Text, Background = System.Windows.Media.Brushes.White };
                if (i == m_selected) label.Background = System.Windows.SystemColors.HighlightBrush;
                label.MouseUp += (a, b) =>
                {
                    NotifySelection(hit, new BH.oM.Geometry.Point { X = m_Popup.PlacementRectangle.X, Y = m_Popup.PlacementRectangle.Y });
                    m_Popup.IsOpen = false;
                };

                Grid.SetRow(label, i);
                Grid.SetColumn(label, 1);
                m_SearchResultGrid.Children.Add(label);
            }
        }

        /*************************************/

        protected override void SetSearchText(string searchText)
        {
            m_SearchTextBox.Text = searchText;
        }

        /*************************************/

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public static ImageSource GetImage(Bitmap bmp)
        {
            if (m_ImageSources.ContainsKey(bmp))
                return m_ImageSources[bmp];

            var handle = bmp.GetHbitmap();
            try
            {
                ImageSource newSource = Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                m_ImageSources[bmp] = newSource;
                DeleteObject(handle);
                return newSource;
            }
            catch
            {
                DeleteObject(handle);
                return null;
            }
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private static int m_MinWidth = 200;
        private static Popup m_Popup = null;
        private static TextBox m_SearchTextBox = null;
        private static Grid m_SearchResultGrid = null;

        private static Dictionary<Bitmap, ImageSource> m_ImageSources = new Dictionary<Bitmap, ImageSource>();
        private int m_selected;
        private int m_hits;

        /*************************************/
    }
}

