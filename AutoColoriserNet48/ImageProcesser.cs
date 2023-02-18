using System;
using System.Configuration;
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
        
        private static string DownloadsFolderPath = ConfigurationManager.AppSettings["DownloadsFolderPath"];
        
        private static int DPI = Int16.Parse(ConfigurationManager.AppSettings["DPI"]);

        public ImageProcesser(ChromeDriver chromeDriver, string outputPath)
        {
            _chromeDriver = chromeDriver;
            _outputPath = outputPath;
        }

        public void ProcessImages(string[] filePaths)
        {
            UploadFiles(filePaths);
            var downloadFilePath = DownloadProcessedFiles();
            ExtractProcessedFiles(downloadFilePath);
        }
        
        private void UploadFiles(string[] filePaths)
        {
            Console.WriteLine($"Downloads folder path: {DownloadsFolderPath}");
        
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
            WriteLog($"Processing files using {DPI} dpi...");
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
            
            WriteLog("Downloading zip file...");
            // wait for download to finish
            while (!File.Exists(downloadFilePath))
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            
            WriteLog("Zip file Downloaded!");

            return downloadFilePath;
        }

        private static IWebElement GetDownloadZipFileElement()
        {
            WebDriverWait wait = new WebDriverWait(_chromeDriver, TimeSpan.FromMinutes(100));
            var element = wait.Until(driver =>
            {
                var el = driver.FindElement(By.Id("downloadZipFile"));
                while (!el.Displayed)
                {
                }

                return el;
            });
            return element;
        }

        private static void ExtractProcessedFiles(string downloadFilePath)
        {
            ZipFile.ExtractToDirectory(downloadFilePath, _outputPath);
            WriteLog("Extracted Successfully");
            File.Delete(downloadFilePath);
        }

        private static void WriteLog(string log)
        {
            Console.WriteLine("\t" + log);
        }
    }
}