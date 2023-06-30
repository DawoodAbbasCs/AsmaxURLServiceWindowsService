using System.IO;
using System;

namespace WindowsService1
{
    public class LogFileManager
    {
        private readonly string logDirectory;

        public LogFileManager(string logDirectory)
        {
            this.logDirectory = logDirectory;
        }

        public string CreateLogFile()
        {
            string logFileName = Path.Combine(logDirectory, $"log_{DateTime.Now.ToString("dd_MM_yyyy")}.txt");
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
            File.Create(logFileName).Close();
            return logFileName;
        }

        public void DeleteOldLogFiles()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(logDirectory);
            FileInfo[] logFiles = directoryInfo.GetFiles("log_*.txt");
            DateTime weekAgo = DateTime.Now.AddDays(-7);

            foreach (FileInfo file in logFiles)
            {
                if (file.LastWriteTime < weekAgo)
                {
                    file.Delete();
                }
            }
        }
    }
}
