using System;
using System.Collections.Generic;
using System.IO;

namespace AutoColoriserNet48
{
    public class FilePathHandler
    {
        private readonly string _sourceFolderName;
        private readonly string _destinationFolderName;

        private static List<string> _processingLogs;
        
        public FilePathHandler(string sourceFolderName, string destinationFolderName, List<string> processingLogs)
        {
            _sourceFolderName = sourceFolderName;
            _destinationFolderName = destinationFolderName;
            _processingLogs = processingLogs;
        }
        
        #region Public Methods
        public (string rootPath, string fullPath) ReadPath()
        {
            WriteLog("Welcome to Adam's Auto Coloriser!");
            Console.Write($"Enter customer directory: ");
            var rootPath = Console.ReadLine();
            if (String.IsNullOrEmpty(rootPath))
            {
                throw new ApplicationException("Customer can't be empty!");
            }

            var finalPath = $"{rootPath}\\{_sourceFolderName}";
            WriteLog($"Chosen source directory: {finalPath}");

            Console.WriteLine();
            
            GetDirectory(finalPath);

            return (rootPath, finalPath);
        }
        
        public string GetOrCreateOutputPath(string inputPath)
        {
            var outputPath = $"{inputPath}\\{_destinationFolderName}";
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
            WriteLog($"Found {fileCount} {consoleFileSuffix}");

            if (fileCount == 0)
            {
                throw new ApplicationException($"Directory does not contain any images. Please add images into {path} and run this program again");
            }
        }

        private static void WriteLog(string message)
        {
            _processingLogs.Add(message);
            
            Console.WriteLine(message);
        }
    }
}