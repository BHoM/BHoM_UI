/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2024, the respective contributors. All rights reserved.
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using BH.Engine.Settings;
using BH.oM.UI;
using BH.UI.Base.Windows.ViewModel;

namespace BH.UI.Base.Windows.Settings
{

    public partial class SearchSettingsWindow : Window
    {

        public SearchSettingsWindow(ISettingsWindow parent)
        {
            InitializeComponent();
            LoadSettings();
            LoadFromLoadedAssemblies();

            m_ToolkitItems = m_ToolkitItems.OrderBy(x => x.Toolkit).ToList();
            m_ToolkitItems.ForEach(x => ConvertToCheckbox(x));

            UIParent = parent;

            this.ShowDialog();
        }

        /*************************************/

        private void LoadSettings()
        {
            var existingSettings = Query.GetSettings(typeof(BH.oM.UI.SearchSettings)) as SearchSettings;
            if (existingSettings != null)
            {
                m_Settings = existingSettings;
                m_ToolkitItems.AddRange(existingSettings.Toolkits.Select(x => new ToolkitSelectItemModel(x)));
            }
            else
                m_Settings = new SearchSettings();
        }

        /*************************************/

        private void LoadFromLoadedAssemblies()
        {
            var allTypes = BH.Engine.Base.Query.BHoMTypeList();
            var fullNames = allTypes.Select(x => x.FullName).ToList();

            var toolkits = fullNames.Select(x =>
            {
                var split = x.Split('.');
                if (split.Length < 3)
                    return null;

                var toolkit = split[2];
                if (toolkit == "Adapters" || toolkit == "Revit" && split.Length > 3)
                    toolkit = split[3];

                return toolkit;
            }).Where(x => !string.IsNullOrEmpty(x)).ToList();

            var toolkitItems = toolkits.Distinct().Select(x =>
            {
                return new ToolkitSelectItem()
                {
                    Toolkit = x,
                    Include = true,
                };
            }).ToList();

            toolkitItems = toolkitItems.Where(x => !m_ToolkitItems.Any(y => y.Toolkit == x.Toolkit)).ToList();
            m_ToolkitItems.AddRange(toolkitItems.Select(x => new ToolkitSelectItemModel(x)));
        }

        /*************************************/

        private void ConvertToCheckbox(ToolkitSelectItemModel toolkitItem)
        {
            CheckBox checkBox = new CheckBox();
            checkBox.IsChecked = toolkitItem.Include;
            checkBox.HorizontalAlignment = HorizontalAlignment.Right;
            checkBox.Margin = new Thickness(10, 0, 0, 0);

            var bind = new Binding();
            bind.Source = toolkitItem;
            bind.Path = new PropertyPath("Include");
            bind.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            bind.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(checkBox, CheckBox.IsCheckedProperty, bind);
            
            TextBlock textBlock = new TextBlock();
            textBlock.Text = toolkitItem.Toolkit;
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.Margin = new Thickness(2, 0, 0, 0);

            StackPanel childPanel = new StackPanel();
            childPanel.Orientation = Orientation.Horizontal;
            childPanel.Children.Add(checkBox);
            childPanel.Children.Add(textBlock);
            childPanel.Margin = new Thickness(0, 0, 0, 10);

            if(m_PanelIndex == 0)
            {
                ToolkitCheckboxGridLeft.Children.Add(childPanel);
                m_PanelIndex++;
            }
            else if(m_PanelIndex == 1)
            {
                ToolkitCheckboxGridRight.Children.Add(childPanel);
                m_PanelIndex = 0;
            }
        }

        /*************************************/

        private void LoadSettings(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.DefaultExt = ".json";
            openFileDlg.Filter = "JSON Files (*json)|*json";

            bool? result = openFileDlg.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var filePath = openFileDlg.FileName;
                try
                {
                    BH.Engine.Settings.Compute.LoadSettings(filePath);
                    var existingSettings = Query.GetSettings(typeof(BH.oM.UI.SearchSettings)) as SearchSettings;
                    if (existingSettings != null)
                    {
                        m_Settings = existingSettings;
                        existingSettings.Toolkits.ForEach(x =>
                        {
                            m_ToolkitItems.Where(y => y.Toolkit == x.Toolkit).FirstOrDefault().Include = x.Include;
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred in loading that settings file. The error recorded was {ex.Message}. Settings have not been loaded.", "Error loading settings file.", MessageBoxButton.OK);
                }
            }
        }

        /*************************************/

        private void SaveAll(object sender, EventArgs e)
        {
            m_Settings.Toolkits = m_ToolkitItems.Select(x => x.ToItem()).ToList();
            BH.Engine.Settings.Compute.SaveSettings(m_Settings, true);
            this.Close();
        }

        /*************************************/

        private void SelectAll(object sender, EventArgs e)
        {
            m_SelectAll = !m_SelectAll;
            m_ToolkitItems.ForEach(x => x.Include = m_SelectAll);

            if (m_SelectAll)
            {
                SelectAllBtn.Content = "Unselect all";
                SelectAllBtn.ToolTip = "Clicking this will uncheck all toolkits currently selected.";
            }
            else
            {
                SelectAllBtn.Content = "Select all";
                SelectAllBtn.ToolTip = "Clicking this will check all toolkits currently not selected.";
            }
        }

        /*************************************/

        protected override void OnClosed(EventArgs e)
        {
            UIParent.OnPopUpClose();
            base.OnClosed(e);
        }

        /***************************************************/
        /**** Private Properties                        ****/
        /***************************************************/

        private int m_PanelIndex = 0;
        private bool m_SelectAll = true;

        private List<ToolkitSelectItemModel> m_ToolkitItems = new List<ToolkitSelectItemModel>();
        private SearchSettings m_Settings = null;
        private ISettingsWindow UIParent { get; set; }
    }
}
