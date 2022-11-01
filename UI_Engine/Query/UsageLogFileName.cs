using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        public static string UsageLogFileName(string uiName)
        {
            if (!string.IsNullOrEmpty(m_usageLogFileName))
                return m_usageLogFileName;

            string logFolder = UsageLogFolder();

            m_usageLogFileName = Path.Combine(logFolder, "Usage_" + uiName + "_" + DateTime.UtcNow.Ticks + ".log");

            return m_usageLogFileName;
        }

        private static string m_usageLogFileName;
    }
}
