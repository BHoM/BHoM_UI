using BH.Engine.Reflection;
using BH.oM.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        /***************************************************/
        /**** Public Methods - Interfaces               ****/
        /***************************************************/

        public static bool IIsToKeepInMenu(this object obj)
        {
            return IsToKeepInMenu(obj as dynamic);
        }


        /***************************************************/
        /**** Public Methods                            ****/
        /***************************************************/

        public static bool IsToKeepInMenu(this object obj)
        {
            return true;
        }

        /***************************************************/

        public static bool IsToKeepInMenu(this MethodBase method)
        {
            return !method.IsNotImplemented() && !method.IsDeprecated();
        }

        /***************************************************/

        public static bool IsToKeepInMenu(this Type type)
        {
            return !type.IsNotImplemented() && !type.IsDeprecated();
        }

        /***************************************************/
    }
}
