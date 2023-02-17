using System;
using System.IO;

namespace AutoColoriserNet48
{
    public class FilePathHandler
    {
        private string RootFolder;
        
        public FilePathHandler(string rootFolder)
        {
            RootFolder = rootFolder;
        }
        
        #region Public Methods
        public string ReadPath()
        {
            Console.WriteLine("Welcome to Adam's Auto Coloriser!");
            Console.Write($"Enter customer name: ");
            var customerFolder = Console.ReadLine();
            if (String.IsNullOrEmpty(customerFolder))
            {
                throw new ApplicationException("Customer can't be empty!");
            }

            Console.WriteLine();

            var path = $"{RootFolder}\\{customerFolder}";
            
            GetDirectory(path);

            return path;
        }
        
        public string GetOrCreateOutputPath(string inputPath)
        {
            var outputPath = $"{inputPath}\\output";
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            return outputPath;
        }
        #endregion
        
        private void GetDirectory(string path)
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
                throw new ApplicationException($"Directory does not contain any images. Please add images into {path} and run this program again");
            }
        }
    }
}