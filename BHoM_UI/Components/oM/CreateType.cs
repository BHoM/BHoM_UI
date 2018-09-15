using BH.UI.Templates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using BH.oM.UI;
using BH.oM.DataStructure;
using BH.Engine.Reflection;
using BH.Engine.DataStructure;
using BH.Engine.Serialiser;
using System.Windows.Forms;

namespace BH.UI.Components
{
    public class CreateTypeCaller : Templates.Caller
    {
        /*************************************/
        /**** Events                      ****/
        /*************************************/

        //public event EventHandler<Type> TypeSelected;


        /*************************************/
        /**** Properties                  ****/
        /*************************************/

        public override System.Drawing.Bitmap Icon_24x24 { get; protected set; } = Properties.Resources.Type;

        public override Guid Id { get; protected set; } = new Guid("D51978F0-6BEB-4832-9F65-DB00DE85C3B9");

        public override string Category { get; protected set; } = "oM";

        public override string Name { get; protected set; } = "CreateType";

        public override string Description { get; protected set; } = "Creates a selected type definition";


        /*************************************/
        /**** Constructors                ****/
        /*************************************/

        public CreateTypeCaller() : base()
        {
            /*if (m_TypeTree == null || m_TypeList == null)
            {
                IEnumerable<Type> types = Engine.Reflection.Query.BHoMTypeList();
                IEnumerable<string> paths = types.Select(x => x.ToText(true));

                List<string> ignore = new List<string> { "BH", "oM", "Engine" };
                m_TypeTree = Engine.DataStructure.Create.Tree(types, paths.Select(x => x.Split('.').Where(y => !ignore.Contains(y))), "select a type").ShortenBranches();
                m_TypeList = paths.Zip(types, (k, v) => new Tuple<string, Type>(k, v)).ToList();
            }*/

            //Selector = new Selector<Type>(Engine.Reflection.Query.BHoMTypeList(), Name);
            //Selector.ItemSelected += M_Menu_ItemSelected;
            SetPossibleItems(Engine.Reflection.Query.BHoMTypeList());

            InputParams = new List<ParamInfo>();
            OutputParams = new List<ParamInfo>() { new ParamInfo { DataType = typeof(Type), Kind = ParamKind.Output, Name = "type", Description = "type definition" } };
        }


        /*************************************/
        /**** Public Method               ****/
        /*************************************/

        public override object Run(object[] inputs)
        {
            return Selector.GetSelectedItem() as Type;
        }

        /*************************************/

        public override bool SetItem(object item)
        {
            return true;
        }

        ///*************************************/

        //public override string Write()
        //{
        //    if (m_Type == null)
        //        return "";
        //    else
        //        return m_Type.ToJson();
        //}

        ///*************************************/

        //public override bool Read(string json)
        //{
        //    if (json != "")
        //        m_Type = Engine.Serialiser.Convert.FromJson(json) as Type;

        //    return true;
        //}

        ///*************************************/

        //public void AddToMenu(ToolStripDropDown menu)
        //{
        //    if (m_Menu_WinForm == null)
        //    {
        //        m_Menu_WinForm = new SelectorMenu_WinForm<Type>(m_TypeList, m_TypeTree);
        //        m_Menu_WinForm.ItemSelected += M_Menu_ItemSelected;
        //    }

        //    m_Menu_WinForm.FillMenu(menu);
        //}

        ///*************************************/

        //public void AddToMenu(System.Windows.Controls.ContextMenu menu)
        //{
        //    if (m_Menu_Wpf == null)
        //    {
        //        m_Menu_Wpf = new SelectorMenu_Wpf<Type>(m_TypeList, m_TypeTree);
        //        m_Menu_Wpf.ItemSelected += M_Menu_ItemSelected;
        //    }

        //    m_Menu_Wpf.FillMenu(menu);
        //}


        /*************************************/
        /**** Private Methods             ****/
        /*************************************/

        //private void M_Menu_ItemSelected(object sender, Type e)
        //{
        //    if (TypeSelected != null)
        //        TypeSelected(this, e);
        //}


        /*************************************/
        /**** Private Fields              ****/
        /*************************************/

        //protected Type m_Type = null;
        //protected static Tree<Type> m_TypeTree = null;
        //protected static List<Tuple<string, Type>> m_TypeList = null;

        //protected SelectorMenu_WinForm<Type> m_Menu_WinForm = null;
        //protected SelectorMenu_Wpf<Type> m_Menu_Wpf = null;

        /*************************************/
    }
}
