using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Compute
    {
        /*************************************/
        /**** Public Methods              ****/
        /*************************************/

        public static void LoadAssemblies()
        {
            if (!m_AssemblyLoaded)
            {
                m_AssemblyLoaded = true;
                IEnumerable<Assembly> uiAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => x.FullName.Split(',').First() == "BHoM_UI");
                if (uiAssemblies.Count() == 1)
                {
                    string folder = Path.GetDirectoryName(uiAssemblies.First().Location);
                    BH.Engine.Reflection.Compute.LoadAllAssemblies(folder);
                }
            }
        }


        /*************************************/
        /**** Static Fields               ****/
        /*************************************/

        private static bool m_AssemblyLoaded = false;
    }
}
