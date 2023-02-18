using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
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
        
        private static readonly string SourceFolderName = ConfigurationManager.AppSettings["SourceFolderName"];
        private static readonly string DestinationFolderName = ConfigurationManager.AppSettings["DestinationFolderName"];
        #endregion
        
        #region Logging
        private static List<string> _processingLogs;
        #endregion

        public AutoColoriser(List<string> processingLogs)
        {
            _processingLogs = processingLogs;
        }

        public void Run()
        {
            var filePathHandler = new FilePathHandler(SourceFolderName, DestinationFolderName, _processingLogs);
            var (rootPath, inputPath) = filePathHandler.ReadPath();
            var outputPath = filePathHandler.GetOrCreateOutputPath(rootPath);
            
            InitializeChromeDriver();

            try
            {
                var imageProcesser = new ImageProcesser(_chromeDriver, outputPath, _processingLogs);
                var filePaths = Directory.GetFiles(inputPath);
                for (var i = 0; i < filePaths.Length; i += 3)
                {
                    if (i >= 3)
                    {
                        ClearConsoleLines();
                    }
                    
                    _chromeDriver.Navigate().GoToUrl(BaseUrl);

                    string[] processSet;
                    var remainingFiles = filePaths.Length - i;
                    if (remainingFiles >= 3)
                    {
                        processSet = new string[3];
                        Array.Copy(filePaths, i, processSet, 0, 3);
                    }
                    else
                    {
                        processSet = new string[remainingFiles];
                        Array.Copy(filePaths, i, processSet, 0, remainingFiles);
                    }

                    WriteLog($"Processing {processSet.Length + i} / {filePaths.Length}");

                    imageProcesser.ProcessImages(processSet);
                }
            }
            catch (Exception)
            {
                _chromeDriver.Quit();
                throw;
            }
            finally
            {
                _chromeDriver.Quit();
            }
                                                                   
            WriteLog("\nFinished processing all files!");
        }

        private void ClearConsoleLines()
        {
            int numOfLines = 7;
            for (int i = 0; i < numOfLines; i++)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.SetCursorPosition(0, Console.CursorTop - 1);
            }
            Console.SetCursorPosition(0, Console.CursorTop);
        }

        private static void InitializeChromeDriver()
        {
            var (screenWidth, screenHeight) = GetScreenSize();
            
            ChromeDriverService service = ChromeDriverService.CreateDefaultService();
            service.EnableVerboseLogging = false;
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;
            
            var options = new ChromeOptions();
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--start-minimized");
            // options.AddArgument("--headless");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-crash-reporter");
            options.AddArgument("--disable-extensions");
            options.AddArgument("--disable-in-process-stack-traces");
            options.AddArgument("--disable-logging");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--log-level=3");
            options.AddArgument("--output=/dev/null");
            
            _chromeDriver = new ChromeDriver(service, options);
            _chromeDriver.Manage().Window.Position = new System.Drawing.Point(screenWidth, screenHeight);
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new SupressSeleniumLogs());
            _chromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMinutes(10);
            _chromeDriver.Manage().Timeouts().PageLoad = TimeSpan.FromMinutes(10);
        }

        private static (int screenWidth, int screenHeight) GetScreenSize()
        {
            Screen primaryScreen = Screen.PrimaryScreen;
            var screenSize = primaryScreen.Bounds.Size;
            return (screenSize.Width, screenSize.Height);
        }

        private static void WriteLog(string log)
        {
            _processingLogs.Add(log);
            
            Console.WriteLine(log);
        }
    }
}