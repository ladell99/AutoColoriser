using System;
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
        private static string _downloadsFolder;
        private static string _outputPath;

        public ImageProcesser(ChromeDriver chromeDriver, string downloadsFolder, string outputPath)
        {
            _chromeDriver = chromeDriver;
            _downloadsFolder = downloadsFolder;
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
            Console.WriteLine("Uploading Files");
            
            // construct file path string for file selection dialog
            var uploadFilesPaths = String.Join(" ", filePaths.Select(fp => $"\"{fp}\""));
            
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

            submitBtn.Click();
            Console.WriteLine("Processing");
        }
        
        private static string DownloadProcessedFiles()
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
            var downloadFileName = fileNameContent.Substring(0, fileNameContent.LastIndexOf('.'));
            
            element.Click();

            var downloadFilePath = _downloadsFolder + $"\\{downloadFileName}.zip";
            
            Console.WriteLine("Downloading zip file...");
            // wait for download to finish
            while (!File.Exists(downloadFilePath))
            {
                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
            
            Console.WriteLine("Zip file Downloaded");

            return downloadFilePath;
        }
        
        private static void ExtractProcessedFiles(string downloadFilePath)
        {
            ZipFile.ExtractToDirectory(downloadFilePath, _outputPath);
            Console.WriteLine("Extracted Successfully");
            File.Delete(downloadFilePath);
        }
    }
}