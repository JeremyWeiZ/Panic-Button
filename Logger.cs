using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public static class Logger
    {
        private static readonly string logFilePath = "AppLog.txt"; // Path to log file

        public static void Log(string message)
        {
            try
            {
                // Create a writer for the file, appending to it if it already exists
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions here, for example, by showing a message to the user
                Console.WriteLine($"{DateTime.Now}: Error writing to log: {ex.Message}");
            }
        }
    }
}
