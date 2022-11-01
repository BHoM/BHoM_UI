using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        [Description("Obtain the file name of the current instance of a usage log file for the provided UI.")]
        [Input("uiName", "The UI currently using the usage log to obtain the log file name for.")]
        [Output("usageLogFileName", "The full file path to the current usage log file for the given UI.")]
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
