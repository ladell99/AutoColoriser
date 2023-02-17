using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace AutoColoriserNet48
{
    public class AutoColoriser
    {
        #region Consts
        private const string BaseUrl = "https://www.img2go.com/colorize-image";
        #endregion
        
        #region Selenium
        private static ChromeDriver _chromeDriver;
        #endregion
        
        #region Paths
        
        private static readonly string RootFolder = ConfigurationManager.AppSettings["RootFolder"];
        private static readonly string DownloadsFolder = ConfigurationManager.AppSettings["DownloadsFolder"];
        #endregion

        public void Run()
        {
            var filePathHandler = new FilePathHandler(RootFolder);
            var inputPath = filePathHandler.ReadPath();
            var outputPath = filePathHandler.GetOrCreateOutputPath(inputPath);
            
            InitializeChromeDriver();

            var imageProcesser = new ImageProcesser(_chromeDriver, DownloadsFolder, outputPath);
            var filePaths = Directory.GetFiles(inputPath);
            for (var i = 0; i < filePaths.Length; i+= 3)
            {
                Console.WriteLine($"Processing {3 + i} / {filePaths.Length}");
                _chromeDriver.Navigate().GoToUrl(BaseUrl);
                    
                string[] setOfThree = new string[3];
                var remainingFiles = filePaths.Length - i;
                if (remainingFiles >= 3)
                {
                    Array.Copy(filePaths, i, setOfThree, 0, 3);
                }
                else
                {
                    Array.Copy(filePaths, i, setOfThree, 0, remainingFiles);
                }

                imageProcesser.ProcessImages(setOfThree);
            } 
        }

        private static void InitializeChromeDriver()
        {
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.EnableVerboseLogging = false;
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;
            
            var options = new ChromeOptions();
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-crash-reporter");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-in-process-stack-traces");
            options.AddArgument("--disable-logging");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--log-level=3");
            options.AddArgument("--output=/dev/null");
            
            _chromeDriver = new ChromeDriver(service, options);
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new SupressSeleniumLogs());
            _chromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.MaxValue;
            _chromeDriver.Manage().Timeouts().PageLoad = TimeSpan.MaxValue;
        }
    }
}