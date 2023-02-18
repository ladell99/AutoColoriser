using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Configuration;
using Newtonsoft.Json;

namespace AutoColoriserNet48
{
    internal class Program
    {
        private static readonly string LogFolderPath = ConfigurationManager.AppSettings["LogFolderPath"];
        
        
        #region logging
        private static List<string> _processingLogs = new List<string>();
        #endregion
        
        private static readonly AutoColoriser _autoColoriser = new AutoColoriser(_processingLogs);
        
        static void Main(string[] args)
        {
            try
            {
                Console.Clear();
                _autoColoriser.Run();
            }
            catch (Exception e)
            {
                for (int i = 0; i < 5; i++)
                {
                    Console.WriteLine();
                }
                
                Console.WriteLine("******************************************************");
                Console.WriteLine("Exception occured! Please contact Adam to resolve this");
                Console.WriteLine("******************************************************");
                Console.WriteLine(e.Message);
                
                WriteLogFile(e);
            }
            finally
            {
                for (int i = 0; i < 3; i++)
                {
                    Console.WriteLine();
                }
                Console.WriteLine("Thanks for using my app! - Adam <3");
                Thread.Sleep(Int32.MaxValue);
            }
        }

        private static void WriteLogFile(Exception exception)
        {
            Directory.CreateDirectory(LogFolderPath);

            var sb = new StringBuilder();
            sb.AppendLine("Processing Log:");
            _processingLogs.ForEach(pl => sb.AppendLine(pl));
            sb.AppendLine("---------------------------------------------------");
            sb.AppendLine("Exception:");
            sb.Append(JsonConvert.SerializeObject(exception, Formatting.Indented));

            var currentTime = DateTime.Now.ToString("dd-MM-yy_hhmmss");
            File.WriteAllText($"{LogFolderPath}\\{currentTime}.txt", sb.ToString());
        }
    }
}