using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using AutoIt;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace AutoColoriserNet48
{
    public class ImageProcesser
    {
        private static ChromeDriver _chromeDriver;
        private static string _outputPath;
        
        private static readonly string DownloadsFolderPath = ConfigurationManager.AppSettings["DownloadsFolderPath"];
        
        private static readonly int DPI = Int16.Parse(ConfigurationManager.AppSettings["DPI"]);

        private static List<string> _processingLogs;

        public ImageProcesser(ChromeDriver chromeDriver, string outputPath, List<string> processingLogs)
        {
            _chromeDriver = chromeDriver;
            _outputPath = outputPath;

            _processingLogs = processingLogs;
        }

        public void ProcessImages(string[] filePaths)
        {
            UploadFiles(filePaths);
            var downloadFilePath = DownloadProcessedFiles();
            ExtractProcessedFiles(downloadFilePath);
        }
        
        private void UploadFiles(string[] filePaths)
        {
            WriteLog("Uploading Files");
            
            // construct file path string for file selection dialog
            var uploadFilesPaths = String.Join(" ", filePaths.Select(fp => $"\"{fp}\""));
            
            // wait for Upload button to be loaded on page
            var wait = new WebDriverWait(_chromeDriver, TimeSpan.FromSeconds(100));
            var fileInput = wait.Until(driver =>
            {
                var element = driver.FindElement(By.ClassName("vue-uploadbox-file-button"));
                while (!element.Enabled) {};
                
                return element;
            });
            
            var submitBtn = _chromeDriver.FindElement(By.ClassName("submit-btn"));
            
            // Change DPI before submitting

            var dpiInput = _chromeDriver.FindElement(By.Id("inputColorRenderFactor"));
            dpiInput.SendKeys(DPI.ToString());
            
            fileInput.Click();

            // wait for file input dialog to open
            AutoItX.WinWait("Open", "", 10);
            AutoItX.ControlSetText("Open", "", "Edit1", uploadFilesPaths);
            AutoItX.ControlClick("Open", "", "Button1");
            // wait for file input dialog to close
            Thread.Sleep(TimeSpan.FromSeconds(1));

            submitBtn.Click();
        }
        
        private static string DownloadProcessedFiles()
        {
            
            // TODO: Find better way to do this
            // Thread.Sleep(TimeSpan.FromMinutes(1));
            IWebElement element;
            try
            {
                element = GetDownloadZipFileElement();
            }
            catch (WebDriverTimeoutException)
            {
                WriteLog("Timeout occurred waiting for processing to finish. Trying again..");
                element = GetDownloadZipFileElement();
            }

            var nameDivs = _chromeDriver.FindElements(By.CssSelector("div.file-name>a"));
            var fileNameContent = nameDivs[0].Text;
            var downloadFileName = fileNameContent.Substring(0, fileNameContent.LastIndexOf('.'));
            
            element.Click();

            var downloadFilePath = DownloadsFolderPath + $"\\{downloadFileName}.zip";
            
            WriteLog("Downloading zip file");
            // wait for download to finish
            var sw = new Stopwatch();
            sw.Start();
            while (!File.Exists(downloadFilePath))
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
                WriteLog($"Downloading zip file ({Math.Truncate(sw.Elapsed.TotalSeconds)}s)", true);
            }
            
            WriteLog("Zip file Downloaded!");

            return downloadFilePath;
        }

        private static IWebElement GetDownloadZipFileElement()
        {
            WriteLog($"Processing files using {DPI} dpi");
            IWebElement downloadElement = null;
            var sw = new Stopwatch();
            sw.Start();
            for (var i = 0; i <= 5; i++)
            {
                var el = _chromeDriver.FindElement(By.Id("downloadZipFile"));

                if (el != null && el.Displayed)
                {
                    WriteLog("Processing Finished!");
                    downloadElement = el;
                    break;
                }

                // use this instead of while loop
                if (i == 5)
                    i = 0;

                Thread.Sleep(TimeSpan.FromSeconds(1));
                WriteLog($"Processing files using {DPI} dpi ({Math.Truncate(sw.Elapsed.TotalSeconds)}s)", true);
            }

            return downloadElement;
        }

        private static void ExtractProcessedFiles(string downloadFilePath)
        {
            ZipFile.ExtractToDirectory(downloadFilePath, _outputPath);
            WriteLog("Extracted Successfully");
            File.Delete(downloadFilePath);
        }

        private static void WriteLog(string log, bool overwriteLast = false)
        {
            if (overwriteLast)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }

            var message = "\t" + log;
            if (overwriteLast && _processingLogs.Count > 0)
            {
                _processingLogs[_processingLogs.Count - 1] = message;
            }
            else
            {
                _processingLogs.Add(message);
            }
            
            Console.WriteLine(message);
        }
    }
}