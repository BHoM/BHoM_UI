using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        public static string UsageLogFolder()
        {
            string logFolder = @"C:\ProgramData\BHoM\Logs";

            //Make sure the folder exists
            if (!Directory.Exists(logFolder))
                Directory.CreateDirectory(logFolder);

            return logFolder;
        }
    }
}
