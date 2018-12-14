using BH.Adapter;
using BH.Engine.Reflection;
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
                        m_SearchMenu = new SearchMenu_Wpf();
                        m_SearchMenu.ItemSelected += M_SearchMenu_ItemSelected;
                    }

                    m_SearchMenu.SetParent(container.Content);
                }
            };

            return true;
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
                        m_SearchMenu = new SearchMenu_WinForm();
                        m_SearchMenu.ItemSelected += M_SearchMenu_ItemSelected;
                    }

                    m_SearchMenu.SetParent(container);
                }
            };
            return true;
        }


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        private static void M_SearchMenu_ItemSelected(object sender, ComponentRequest request)
        {
            if (request != null)
                ItemSelected?.Invoke(sender,request);
        }


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        private static SearchMenu m_SearchMenu = null;

        /*************************************/
    }

}
