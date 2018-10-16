using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.UI.Global
{
    public abstract class SearchMenu
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

        public abstract bool SetParent(object parent);


        /*************************************/
        /**** Protected Methods           ****/
        /*************************************/

        protected void NotifySelection(string methodName)
        {
            ItemSelected?.Invoke(this, PossibleItems[methodName]);
        }

        /*************************************/

        protected List<KeyValuePair<string, MethodInfo>> GetHits(string search)
        {
            string text = search.Trim().ToLower();
            string[] parts = text.Split(' ');

            List<KeyValuePair<string, MethodInfo>> hits = new List<KeyValuePair<string, MethodInfo>>();
            if (text.Length > 0)
                hits = PossibleItems.Where(x => parts.All(y => x.Key.Substring(0, x.Key.IndexOf('(') + 1).ToLower().Contains(y))).Take(20).OrderBy(x => x.Key).ToList();

            return hits;
        }

        /*************************************/

        protected Bitmap GetIcon(MethodInfo method)
        {
            if (method == null)
                return Properties.Resources.Empty;

            switch (method.DeclaringType.Name)
            {
                case "Compute":
                    return Properties.Resources.Compute;
                case "Convert":
                    return Properties.Resources.Convert;
                case "Create":
                    return Properties.Resources.CreateBHoM;
                case "Modify":
                    return Properties.Resources.Modify;
                case "Query":
                    return Properties.Resources.Query;
                default:
                    return Properties.Resources.Empty;
            }
        }

        /*************************************/
    }
}
