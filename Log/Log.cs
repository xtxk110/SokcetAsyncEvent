using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LogLib
{
    public class Log
    {
        public static void WriteLog(string message, LogType logType)
        {
            string dirPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + @"log";
            string logPath = string.Empty;

            dirPath += @"\" + DateTime.Now.ToString("yyyyMMdd")+@"\";
            CreateDir(dirPath);

            int hour = DateTime.Now.Hour;
            string prefix = string.Empty;
            switch (logType)
            {
                case LogType.INFO:
                    prefix = "INFO_";
                    break;
                case LogType.DATA:
                    prefix = "DATA_";
                    break;
                case LogType.ERROR:
                    prefix = "ERROR_";
                    break;
            }
            logPath = dirPath + prefix + (hour % 2 == 0 ? hour.ToString().PadLeft(2, '0') : (hour - 1).ToString().PadLeft(2, '0')) + @".txt";

            try
            {
                using (FileStream fs = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "：" + message);
                    }
                }
            }
            catch (Exception e) { }
            dirPath = null; logPath = null;
        }
        private static void CreateDir(string dirPath)
        {
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
        }

    }
}
