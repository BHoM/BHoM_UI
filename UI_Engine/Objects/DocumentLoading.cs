using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Compute
    {
        private static bool m_documentOpening = false;

        public static void SetDocumentOpeningState(bool state)
        {
            m_documentOpening = state;
        }
    }
}
