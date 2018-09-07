using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                BH.Engine.Reflection.Compute.LoadAllAssemblies();
            }
        }

        /*************************************/
        /**** Static Fields               ****/
        /*************************************/

        private static bool m_AssemblyLoaded = false;
    }
}
