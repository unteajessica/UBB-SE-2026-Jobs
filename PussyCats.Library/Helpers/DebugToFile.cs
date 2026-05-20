using System;
using System.Collections.Generic;
using System.Text;

namespace PussyCats.Library.Helpers
{
    public static class DebugToFile
    {
        public static void Write(string type, string message)
        {
            try
            {
                string path = "C:\\PERSONALE\\Desktop\\iss.log";

                if (!System.IO.File.Exists(path))
                {
                    return;
                }

                System.IO.File.AppendAllText(path, $"{type}  -  {message}{Environment.NewLine}");
            }
            catch
            {
                // Ignore any exceptions that occur while writing to the file
            }
        }
    }
}