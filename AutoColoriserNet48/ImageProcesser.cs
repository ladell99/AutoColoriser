using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            var sw = new Stopwatch();
            sw.Start();
            using (new PeriodicProgressTracking(() =>
                   {
                       WriteLog($"Uploading Files ({sw.Elapsed.ToString("mm':'ss")})", true);
                       return Task.CompletedTask;
                   }))
            {
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

                bool fileInputCompleted = false;
                while (!fileInputCompleted)
                {
                    AutoItX.WinWait("Open", "", 10);
                    AutoItX.ControlSetText("Open", "", "Edit1", uploadFilesPaths);
                    AutoItX.ControlClick("Open", "", "Button1");
                    // wait for file input dialog to close
                    Thread.Sleep(200);
                    if (AutoItX.WinWait("Open", "", 1) == 0)
                    {
                        fileInputCompleted = true;
                    }
                }

                bool uploadFinished = false;
                while (!uploadFinished)
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    ReadOnlyCollection<IWebElement> uploadSpinners = null;
                    try
                    {
                        uploadSpinners = _chromeDriver.FindElements(By.ClassName("badge-info"));
                    }
                    catch (WebDriverTimeoutException) { }
                    catch (NoSuchElementException) {}
                    if (uploadSpinners == null || uploadSpinners.Count == 0)
                        uploadFinished = true;
                }

                submitBtn.Click();
            }
        }
        
        private static string DownloadProcessedFiles()
        {
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
            using (new PeriodicProgressTracking(() =>
                   {
                       WriteLog($"Downloading zip file ({sw.Elapsed.ToString("mm':'ss")})", true);
                       return Task.CompletedTask;
                   }))
            {
                while (!File.Exists(downloadFilePath))
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }  
            }
            WriteLog("Zip file Downloaded!");

            return downloadFilePath;
        }

        private static IWebElement GetDownloadZipFileElement()
        {
            WriteLog($"Processing files using {DPI} dpi");
            var sw = new Stopwatch();
            sw.Start();
            using (new PeriodicProgressTracking(() =>
                   {
                       WriteLog($"Processing files using {DPI} dpi ({sw.Elapsed.ToString("mm':'ss")})", true);
                       return Task.CompletedTask;
                   }))
            {
                while (true)
                {
                    IWebElement el = null;
                    try
                    {
                        el = _chromeDriver.FindElement(By.Id("downloadZipFile"));
                    } 
                    catch(WebDriverTimeoutException) {}
                    catch(NoSuchElementException) {}

                    if (el != null && el.Displayed)
                    {
                        return el;
                    }
                }
            }
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