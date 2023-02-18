using System;
using System.IO;

namespace AutoColoriserNet48
{
    public class FilePathHandler
    {
        private string SourceFolderName;
        private string DestinationFolderName;
        
        public FilePathHandler(string sourceFolderName, string destinationFolderName)
        {
            SourceFolderName = sourceFolderName;
            DestinationFolderName = destinationFolderName;
        }
        
        #region Public Methods
        public (string rootPath, string fullPath) ReadPath()
        {
            Console.WriteLine("Welcome to Adam's Auto Coloriser!");
            Console.Write($"Enter customer directory: ");
            var rootPath = Console.ReadLine();
            if (String.IsNullOrEmpty(rootPath))
            {
                throw new ApplicationException("Customer can't be empty!");
            }

            var finalPath = $"{rootPath}\\{SourceFolderName}";
            Console.WriteLine($"Chosen source directory: {finalPath}");
            
            Console.WriteLine();
            
            GetDirectory(finalPath);

            return (rootPath, finalPath);
        }
        
        public string GetOrCreateOutputPath(string inputPath)
        {
            var outputPath = $"{inputPath}\\{DestinationFolderName}";
            Directory.CreateDirectory(outputPath);

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