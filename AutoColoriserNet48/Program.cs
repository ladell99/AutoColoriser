using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using AutoIt;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using Newtonsoft.Json;

namespace AutoColoriserNet48
{
    internal class Program
    {
        private static readonly string LogFolderPath = ConfigurationManager.AppSettings["LogFolderPath"];
        
        private static readonly AutoColoriser _autoColoriser = new AutoColoriser();
        
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

            var currentTime = DateTime.Now.ToString("dd-MM-yy_hhmmss");
            File.WriteAllText($"{LogFolderPath}\\{currentTime}.txt", JsonConvert.SerializeObject(exception, Formatting.Indented));
        }
    }
}