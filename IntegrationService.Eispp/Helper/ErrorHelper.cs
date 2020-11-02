// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace IntegrationService.Eispp.Helper
{
    public static class ErrorHelper
    {
        public static string ErrorLogPath
        {
            get
            {
                return "/ErrorLog/";
            }
        }
        public static string basePath
        {
            get
            {
                return "";
            }
        }

        /// <summary>
        /// Must be true to store messages in Event log
        /// </summary>
        public static bool WriteToEventLog { get; set; }

        /// <summary>
        /// Добавя запис в лога
        /// </summary>
        /// <param name="ErrorLocation">Местоположение на грешката</param>
        /// <param name="ErrorMessage">Съобщение за грешка</param>
        public static void WriteToLog(string ErrorLocation, string ErrorMessage)
        {
            if (WriteToEventLog)
            {
                EventLog.WriteEntry(Properties.Settings.Default.ServiceName, ErrorMessage,
                    EventLogEntryType.Error, 666);
            }
            else
            {
                WriteToFile(ErrorLocation, ErrorMessage, ErrorType.Error);
            }
        }

        /// <summary>
        /// Добавя запис JavaScript в лога
        /// </summary>
        /// <param name="ErrorLocation">Местоположение на грешката</param>
        /// <param name="ErrorMessage">Съобщение за грешка</param>
        public static void WriteToJsLog(string ErrorLocation, string ErrorMessage)
        {
            WriteToFile(ErrorLocation, ErrorMessage, ErrorType.JsError);
        }

        /// <summary>
        /// Добавя запис в лога за debug
        /// </summary>
        /// <param name="ErrorLocation">Местоположение на грешката</param>
        /// <param name="ErrorMessage">Съобщение за грешка</param>
        public static void WriteToDebugLog(string ErrorLocation, string ErrorMessage)
        {
            WriteToFile(ErrorLocation, ErrorMessage, ErrorType.Debug);
        }

        /// <summary>
        /// Добавя запис в лога.
        /// Конкатенира съобщенията от Exception и InnerException
        /// </summary>
        /// <param name="ErrorLocation">Местоположение на грешката</param>
        /// <param name="ex">Прихванат Exception</param>
        /// <param name="stackTrace">Да се отпечата ли StackTrace</param>
        public static void WriteToLog(string ErrorLocation, Exception ex, bool stackTrace = false)
        {
            string message = ExceptionToString(ex);

            if (stackTrace)
            {
                message = String.Format("{0}\nStackTrace: {1}", message, ex.StackTrace);
            }

            WriteToLog(ErrorLocation, message);
        }

        public static string ExceptionToString(Exception ex)
        {
            StringBuilder message = new StringBuilder(String.Format("Exception: {0}", ex.Message));

            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                message.AppendFormat("\nInner Exception: {0}", ex.Message);
            }

            return message.ToString();
        }

        private static void WriteToFile(string errorLocation, string errorMessage, ErrorType type)
        {
            string logLine = DateTime.Now.ToString("yyyy-MM-dd \\ HH:mm:ss") + @" -> " + errorLocation + @" \ " + errorMessage;
            string logFileName = String.Empty;
            switch (type)
            {
                case ErrorType.Error:
                    logFileName = "log_" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt";
                    break;
                case ErrorType.JsError:
                    logFileName = "JSlog_" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt";
                    break;
                case ErrorType.Debug:
                    logFileName = "Debug_" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt";
                    break;
                default:
                    break;
            }

            try
            {
                FileInfo _fi = new FileInfo(basePath + logFileName);
                if (!_fi.Exists)
                {
                    using (StreamWriter sw = File.CreateText(basePath + logFileName))
                    {
                        sw.WriteLine("start");
                    }
                }
                using (StreamWriter sw = File.AppendText(basePath + logFileName))
                {
                    sw.WriteLine(logLine);
                }
            }
            catch
            {
            }
        }
    }

    public enum ErrorType
    {
        Error = 1,
        JsError = 2,
        Debug = 3
    }
}
