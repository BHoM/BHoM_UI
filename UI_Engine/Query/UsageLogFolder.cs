using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using BH.oM.Base.Attributes;

namespace BH.Engine.UI
{
    public static partial class Query
    {
        [Description("Obtain the directory path to the logs folder.")]
        [Output("logFolder", "The directory path to the logs folder for the BHoM.")]
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
