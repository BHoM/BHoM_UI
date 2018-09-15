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
//    public abstract class DynamicMethodCaller : MethodCaller
//    {
//        /*************************************/
//        /**** Events                      ****/
//        /*************************************/

//        //public event EventHandler<MethodBase> MethodSelected;


//        /*************************************/
//        /**** Properties                  ****/
//        /*************************************/

//        //public virtual Selector<MethodBase> Selector { get; set; }


//        /*************************************/
//        /**** Constructors                ****/
//        /*************************************/

//        public DynamicMethodCaller() : base(null)
//        {
//            Selector = new Selector<MethodBase>(GetPossibleMethods(), Name);
//            //Selector.ItemSelected += M_Menu_ItemSelected;
//        }

//        /*************************************/
//        /**** Public Methods              ****/
//        /*************************************/

//        public abstract IEnumerable<MethodBase> GetPossibleMethods();


//        /*************************************/
//        /**** Private Methods             ****/
//        /*************************************/

//        //private void M_Menu_ItemSelected(object sender, MethodBase e)
//        //{
//        //    SetMethod(e);

//        //    if (MethodSelected != null)
//        //        MethodSelected(this, e);
//        //}


//        /*************************************/
//        /**** Private Fields              ****/
//        /*************************************/



//        /*************************************/
//    }
//}
