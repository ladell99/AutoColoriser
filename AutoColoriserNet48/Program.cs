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
        private const string BaseUrl = "https://www.img2go.com/colorize-image";
        // private const string ChromeDriver = "C:\\Users\\AdamLadell\\Desktop\\coloriser_test\\chromedriver.exe";
        private static readonly string RootFolder = ConfigurationManager.AppSettings["RootFolder"];
        private static readonly string DownloadsFolder = ConfigurationManager.AppSettings["DownloadsFolder"];

        private static string _inputPath;
        private static string _outputPath;
        
        private static string _downloadFileName;
        private static string _downloadFilePath;

        private static ChromeDriver _chromeDriver;
        
        // TODO: 3 at a time
        
        static void Main(string[] args)
        {
            try
            {
                _inputPath = ReadPath("input");
                _outputPath = GetOrCreateOutputPath();

                var filePaths = Directory.GetFiles(_inputPath);
            
                _chromeDriver = new ChromeDriver();
                Trace.Listeners.Clear();
                Trace.Listeners.Add(new SupressSeleniumLogs());
            
                _chromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.MaxValue;
                _chromeDriver.Manage().Timeouts().PageLoad = TimeSpan.MaxValue;
            
                UploadFiles(filePaths);
                DownloadProcessedFiles();
                ExtractProcessedFiles();
                Console.WriteLine("Thanks for using my app! - Adam <3");
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception occured! Please contact Adam to resolve this");
                WriteLogFile(e);
                Environment.Exit(500);
            }   
        }

        private static void WriteLogFile(Exception exception)
        {
            var logFolderPath = $"{RootFolder}\\Logs";
            if (!Directory.Exists(logFolderPath))
            {
                Directory.CreateDirectory(logFolderPath);
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Input Path: {_inputPath}");
            sb.AppendLine($"Output Path: {_outputPath}");
            sb.AppendLine($"Download File Name: {_downloadFileName}");
            sb.AppendLine($"Download File Path: {_downloadFilePath}");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("-----------------------------------");
            sb.AppendLine(JsonConvert.SerializeObject(exception, Formatting.Indented));

            var currentTime = DateTime.Now.ToString("dd-MM-yy_hhmmss");
            var filePath = $"{logFolderPath}\\{currentTime}.txt";
            File.WriteAllText(filePath, sb.ToString());
        }

        private static string GetOrCreateOutputPath()
        {
            var outputPath = $"{_inputPath}\\output";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            return outputPath;
        }

        private static void ExtractProcessedFiles()
        {
            ZipFile.ExtractToDirectory(_downloadFilePath, _outputPath);
            Console.WriteLine("Extracted Successfully");
            File.Delete(_downloadFilePath);
        }

        private static string ReadPath(string type)
        {
            Console.Write($"Enter customer name: ");
            var customerFolder = Console.ReadLine();
            if (String.IsNullOrEmpty(customerFolder))
            {
                throw new ApplicationException("Customer can't be empty!");
            }

            var path = $"{RootFolder}\\{customerFolder}";
            
            GetDirectory(path);

            return path;
        }

        private static void GetDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var fileCount = Directory.GetFiles(path).Length;
            string consoleFileSuffix = fileCount == 1 ? "file" : "files";
            Console.WriteLine($"Found {fileCount} {consoleFileSuffix}");

            if (fileCount == 0)
            {
                Console.WriteLine("Exiting");
                Environment.Exit(400);
            }
        }

        private static void UploadFiles(string[] filepaths)
        {
            Console.WriteLine("Uploading Files");
            _chromeDriver.Navigate().GoToUrl(BaseUrl);
            
            // construct file path string for file selection dialog
            var uploadFilesPaths = String.Join(" ", filepaths.Select(fp => $"\"{fp}\""));
            
            // wait for Upload button to be loaded on page
            var wait = new WebDriverWait(_chromeDriver, TimeSpan.FromSeconds(100));
            wait.Until(driver => driver.FindElement(By.ClassName("vue-uploadbox-file-button")));
            
            var fileInput = _chromeDriver.FindElement(By.ClassName("vue-uploadbox-file-button"));
            var submitBtn = _chromeDriver.FindElement(By.ClassName("submit-btn"));
            
            fileInput.Click();

            // wait for file input dialog to open
            AutoItX.WinWait("Open", "", 10);
            AutoItX.ControlSetText("Open", "", "Edit1", uploadFilesPaths);
            AutoItX.ControlClick("Open", "", "Button1");
            // wait for file input dialog to close
            Thread.Sleep(TimeSpan.FromSeconds(2));

            // get titles of uploaded files
            /*var uploadDivs = _chromeDriver.FindElements(By.CssSelector("span.overflow-hidden.text-overflow-ellipsis.white-space-nowrap"));
            var fileNameContent = uploadDivs[0].Text;
            _downloadFileName = fileNameContent.Substring(0, fileNameContent.LastIndexOf('.'));
            Console.WriteLine($"Download File Name: {_downloadFileName}");*/
            
            submitBtn.Click();
        }

        private static void DownloadProcessedFiles()
        {
            WebDriverWait wait = new WebDriverWait(_chromeDriver, TimeSpan.FromMinutes(10));
            var element = wait.Until(driver =>
            {
                var el = driver.FindElement(By.Id("downloadZipFile"));
                while (!el.Displayed) { }

                return el;
            });

            var nameDivs = _chromeDriver.FindElements(By.CssSelector("div.file-name>a"));
            var fileNameContent = nameDivs[0].Text;
            _downloadFileName = fileNameContent.Substring(0, fileNameContent.LastIndexOf('.'));
            Console.WriteLine($"Download File Name: {_downloadFileName}");
            
            element.Click();

            _downloadFilePath = DownloadsFolder + $"\\{_downloadFileName}.zip";
            
            Console.WriteLine("Downloading zip file...");
            // wait for download to finish
            while (!File.Exists(_downloadFilePath))
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            
            Console.WriteLine("Zip file Downloaded");
        }

        class SupressSeleniumLogs : ConsoleTraceListener
        {
            public override void Write(string message) {}
            public override void WriteLine(string message) {}
        }
    }
}