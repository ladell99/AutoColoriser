using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        public IEnumerable<string> GetInputFiles(string outputPath, string inputPath)
        {
            var inputFiles = Directory.GetFiles(inputPath);
            var existingFiles = Directory.GetFiles(outputPath).Select(ef => ef.Substring(ef.LastIndexOf("\\", StringComparison.Ordinal))).ToArray();

            var conflictingFiles = new List<string>();
            var updatedInputFiles = new List<string>();

            if (existingFiles.Length == 0)
                return inputFiles;
            
            foreach (var i in inputFiles)
            {
                var iFileName = i.Substring(i.LastIndexOf("\\", StringComparison.Ordinal));
                
                if (existingFiles.Contains(iFileName))
                {
                    conflictingFiles.Add(i);
                }
                else
                {
                    updatedInputFiles.Add(i);
                }
            }

            if (conflictingFiles.Count > 0)
            {
                WriteLog($"Skipping {conflictingFiles.Count} files, as they have already been processed before");
            }

            return updatedInputFiles;
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