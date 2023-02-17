using AutoIt;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace AutoColoriser
{
    class Program
    {
        private const string BaseUrl = "https://www.img2go.com/colorize-image";
        private const string ChromeDriver = "C:\\Users\\AdamLadell\\Desktop\\coloriser_test\\chromedriver.exe";
        private const string DebugFolder = "C:\\Users\\AdamLadell\\Desktop\\coloriser_test\\debug";
        
        static void Main(string[] args)
        {
            ProcessFiles();
            // var inputPath = ReadPath("input");
            // var outputPath = ReadPath("output");
        }

        private static string ReadPath(string type)
        {
            Console.Write($"Enter {type} path: ");
            var path = Console.ReadLine();
            if (String.IsNullOrEmpty(path))
            {
                Console.WriteLine("Path must not be empty");
                ReadPath(type);
            }
            
            GetDirectory(path);

            return path;
        }

        private static void GetDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Console.WriteLine($"'{path}' does not exist!");
                Environment.Exit(400);
            }

            var fileCount = Directory.GetFiles(path).Length;
            string consoleFileSuffix = fileCount == 1 ? "file" : "files";
            Console.WriteLine($"Found {fileCount} {consoleFileSuffix}");
        }

        private static void ProcessFiles()
        {
            var chromeDriver = new ChromeDriver();
            chromeDriver.Manage().Timeouts().ImplicitWait = TimeSpan.MaxValue;
            chromeDriver.Navigate().GoToUrl(BaseUrl);

            Thread.Sleep(10);
            
            var fileInput = chromeDriver.FindElement(By.ClassName("vue-uploadbox-file-button"));
            // fileInput.SendKeys("C:\\Users\\AdamLadell\\Desktop\\coloriser_test\\debug\\test.png");
            fileInput.Click();
            Thread.Sleep(10);

            AutoItX.WinWait("Open", "" 10);
        }
    }
}