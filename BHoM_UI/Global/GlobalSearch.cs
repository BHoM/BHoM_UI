using BH.Engine.Reflection;
using BH.Engine.Reflection.Convert;
using BH.oM.UI;
using BH.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BH.UI.Global
{
    public static class GlobalSearch
    {
        /*************************************/
        /**** Public Events               ****/
        /*************************************/

        public static event EventHandler<ComponentRequest> ItemSelected;


        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static bool Activate(ContentControl container)
        {
            container.KeyDown += (sender, e) =>
            {
                if (e.Key == Key.B && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    if (m_SearchMenu == null)
                    {
                        m_SearchMenu = new SearchMenu_Wpf { PossibleItems = m_MethodList };
                        m_SearchMenu.ItemSelected += M_SearchMenu_ItemSelected;
                    }

                    m_SearchMenu.SetParent(container.Content);
                }
            };

            EventManager.RegisterClassHandler(typeof(Window), Keyboard.KeyDownEvent, new KeyEventHandler(OnKeyPressed), true);

            return Activate();
        }

        /*************************************/

        public static bool Activate(System.Windows.Forms.ContainerControl container)
        {
            container.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == System.Windows.Forms.Keys.B && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    if (m_SearchMenu == null)
                    {
                        m_SearchMenu = new SearchMenu_WinForm { PossibleItems = m_MethodList };
                        m_SearchMenu.ItemSelected += M_SearchMenu_ItemSelected;
                    }

                    m_SearchMenu.SetParent(container);
                }
            };
            return Activate();
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private static bool Activate()
        {
            if (!m_Activated)
            {
                m_MethodList = new Dictionary<string, MethodInfo>();
                foreach (MethodInfo method in BH.Engine.Reflection.Query.BHoMMethodList().Where(x => !x.IsNotImplemented() && !x.IsDeprecated()))
                {
                    string key = method.DeclaringType.Name + '.' + method.ToText(true);
                    m_MethodList[key] = method;
                }

                //EventManager.RegisterClassHandler(typeof(Window), Keyboard.KeyDownEvent, new KeyEventHandler(OnKeyPressed), true);
                m_Activated = true;
            }

            return true;
        }

        /*************************************/

        private static void OnKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.B && (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                Console.WriteLine("hey");
            }
        }

        /*************************************/

        //private static void CreateMenu(ContentControl parent)
        //{
        //    if (m_SearchMenu == null)
        //    {
        //        m_SearchMenu = new SearchMenu_Wpf { PossibleItems = m_MethodList };
        //        m_SearchMenu.ItemSelected += M_SearchMenu_ItemSelected;
        //    }

        //    m_SearchMenu.SetParent(parent.Content);
        //}

        /*************************************/

        private static void M_SearchMenu_ItemSelected(object sender, MethodInfo method)
        {
            Type callerType = null;

            if (method.DeclaringType.Namespace.StartsWith("BH.Engine"))
            {
                switch(method.DeclaringType.Name)
                {
                    case "Compute":
                        callerType = typeof(ComputeCaller);
                        break;
                    case "Convert":
                        callerType = typeof(ConvertCaller);
                        break;
                    case "Create":
                        callerType = typeof(CreateObjectCaller);
                        break;
                    case "Modify":
                        callerType = typeof(ModifyCaller);
                        break;
                    case "Query":
                        callerType = typeof(QueryCaller);
                        break;
                }
            }

            if (callerType != null)
                ItemSelected?.Invoke(sender, new ComponentRequest { CallerType = callerType, SelectedItem = method });
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private static ISearchMenu m_SearchMenu = null;
        private static bool m_Activated = false;
        private static Dictionary<string, MethodInfo> m_MethodList = new Dictionary<string, MethodInfo>();

        /*************************************/
    }

}
