//using BH.Engine.Reflection;
//using BH.oM.Reflection;
//using BH.Engine.UI;
//using BH.oM.Reflection.Attributes;
//using BH.oM.UI;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using BH.oM.DataStructure;
//using System.Windows.Forms;
//using BH.Engine.Serialiser;

//namespace BH.UI.Templates
//{
//    public abstract class DynamicMethodCaller_old : MethodCaller
//    {
//        /*************************************/
//        /**** Events                      ****/
//        /*************************************/

//        public event EventHandler<MethodBase> MethodSelected;


//        /*************************************/
//        /**** Constructors                ****/
//        /*************************************/

//        public DynamicMethodCaller_old() : base(null)
//        {
//            string key = Name;
//            if (!m_MethodTreeStore.ContainsKey(key) || !m_MethodListStore.ContainsKey(key))
//            {
//                Output<List<Tuple<string, MethodBase>>, Tree<MethodBase>> organisedMethod = GetPossibleMethods().Where(x => !x.IsNotImplemented() && !x.IsDeprecated()).OrganiseMethods();
//                m_MethodListStore[key] = organisedMethod.Item1;
//                m_MethodTreeStore[key] = organisedMethod.Item2;
//            }
//        }

//        /*************************************/
//        /**** Public Methods              ****/
//        /*************************************/

//        public abstract IEnumerable<MethodBase> GetPossibleMethods();

//        /*************************************/

//        public void AddToMenu(ToolStripDropDown menu)
//        {
//            if (m_Menu_WinForm == null)
//            {
//                string key = Name;
//                m_Menu_WinForm = new SelectorMenu_WinForm<MethodBase>(m_MethodListStore[key], m_MethodTreeStore[key]);
//                m_Menu_WinForm.ItemSelected += M_Menu_ItemSelected;
//            }

//            if (Method == null)
//                m_Menu_WinForm.FillMenu(menu);
//        }

//        /*************************************/

//        public void AddToMenu(System.Windows.Controls.ContextMenu menu)
//        {
//            if (m_Menu_Wpf == null)
//            {
//                string key = Name;
//                m_Menu_Wpf = new SelectorMenu_Wpf<MethodBase>(m_MethodListStore[key], m_MethodTreeStore[key]);
//                m_Menu_Wpf.ItemSelected += M_Menu_ItemSelected;
//            }

//            if (Method == null)
//                m_Menu_Wpf.FillMenu(menu);
//        }

//        /*************************************/

//        public override string Write()
//        {
//            try
//            {
//                if (Method == null)
//                    return "";
//                else
//                    return Method.ToJson();
//            }
//            catch
//            {
//                return "";
//            }
//        }

//        /*************************************/

//        public override bool Read(string json)
//        {
//            try
//            {
//                MethodBase method = BH.Engine.Serialiser.Convert.FromJson(json) as MethodBase;

//                if (method != null)
//                {
//                    SetMethod(method);
//                    return true;
//                }
//                else
//                    return false;
//            }
//            catch
//            {
//                return false;
//            }
                
//        }


//        /*************************************/
//        /**** Private Methods             ****/
//        /*************************************/

//        private void M_Menu_ItemSelected(object sender, MethodBase e)
//        {
//            SetMethod(e);

//            if (MethodSelected != null)
//                MethodSelected(this, e);
//        }


//        /*************************************/
//        /**** Private Fields              ****/
//        /*************************************/

//        protected SelectorMenu_WinForm<MethodBase> m_Menu_WinForm = null;
//        protected SelectorMenu_Wpf<MethodBase> m_Menu_Wpf = null;

//        private static Dictionary<string, Tree<MethodBase>> m_MethodTreeStore = new Dictionary<string, Tree<MethodBase>>();
//        private static Dictionary<string, List<Tuple<string, MethodBase>>> m_MethodListStore = new Dictionary<string, List<Tuple<string, MethodBase>>>();


//        /*************************************/
//    }
//}
