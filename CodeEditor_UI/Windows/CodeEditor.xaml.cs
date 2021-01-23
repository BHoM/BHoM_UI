/*
 * This file is part of the Buildings and Habitats object Model (BHoM)
 * Copyright (c) 2015 - 2021, the respective contributors. All rights reserved.
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

 using BH.Engine.Reflection;
using ICSharpCode.AvalonEdit.CodeCompletion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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

namespace BH.UI.Base.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class CodeEditor : Window
    {
        /***************************************************/
        /**** Events                                    ****/
        /***************************************************/

        public event EventHandler<MethodInfo> MethodRebuilt;

        /***************************************************/
        /**** Constructors                              ****/
        /***************************************************/

        public CodeEditor()
        {
            InitializeComponent();

            //CodeView.TextArea.TextEntering += TextArea_TextEntering;   // Something for later :-)
            //CodeView.TextArea.TextEntered += TextArea_TextEntered;
            RebuildButton.Click += RebuildButton_Click;
            AddReferenceButton.MouseUp += AddReferenceButton_MouseUp;
        }


        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public void SetMethod(MethodInfo method)
        {
            if (method == null)
                return;

            string code = BH.Engine.CSharp.Convert.ToCSharpText(method, true);
            CodeView.Text = BH.Engine.CSharp.Compute.MakeStandAlone(code, method);

            m_RefFiles = method.DeclaringType.Assembly.UsedAssemblies(false, true).Select(x => x.Location).ToList();
            m_RefFiles.Add(method.DeclaringType.Assembly.Location);

            foreach (string file in m_RefFiles.OrderBy(x => x))
                ReferenceView.Items.Add(new ListViewItem { Content = System.IO.Path.GetFileName(file) });

            RebuildCode(CodeView.Text, m_RefFiles);
        }

        /***************************************************/

        public void SetCode(string code, List<string> refFiles)
        {
            CodeView.Text = code;
            m_RefFiles = refFiles;
            RebuildCode(CodeView.Text, m_RefFiles);
        }

        /***************************************************/

        public string GetCode()
        {
            return CodeView.Text;
        }

        /***************************************************/

        public List<string> GetReferenceFiles()
        {
            return m_RefFiles;
        }


        /***************************************************/
        /**** CodeView Methods                          ****/
        /***************************************************/

        private void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == ".")
            {
                // Open code completion after the user has pressed dot:
                m_CompletionWindow = new CompletionWindow(CodeView.TextArea);
                IList<ICompletionData> data = m_CompletionWindow.CompletionList.CompletionData;
                // TODO: Insert list of items extracted with Roslyn
                m_CompletionWindow.Show();
                m_CompletionWindow.Closed += delegate {
                    m_CompletionWindow = null;
                };
            }
        }

        /***************************************************/

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && m_CompletionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    m_CompletionWindow.CompletionList.RequestInsertion(e);
                }
            }
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }


        /***************************************************/
        /**** Button Methods                            ****/
        /***************************************************/

        private void RebuildButton_Click(object sender, RoutedEventArgs e)
        {
            MethodInfo method = RebuildCode(CodeView.Text, m_RefFiles);
            if (method != null)
                MethodRebuilt?.Invoke(this, method);
        }

        /***************************************************/

        private MethodInfo RebuildCode(string code, List<string> refFiles)
        {
            BH.Engine.Reflection.Compute.ClearCurrentEvents();
            MethodInfo result = BH.Engine.CSharp.Compute.CompileMethod(code, refFiles);

            if (result == null)
                ConsoleView.Text = BH.Engine.Reflection.Query.CurrentEvents().Select(x => "\n" + x.Message).Aggregate((a, b) => a + b);
            else
                ConsoleView.Text = "\nCompiled successfully.";

            return result;
        }

        /***************************************************/

        private void AddReferenceButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "Assembly files (*.dll) | *.dll",
                InitialDirectory = @"C:\ProgramData\BHoM\Assemblies"
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file = openFileDialog.FileName;
                m_RefFiles.Add(file);
                ReferenceView.Items.Add(new ListViewItem { Content = System.IO.Path.GetFileName(file) });
            }
        }

        /***************************************************/
        /**** Private Fields                            ****/
        /***************************************************/

        CompletionWindow m_CompletionWindow;
        List<string> m_RefFiles = new List<string>();

        /***************************************************/
    }
}
