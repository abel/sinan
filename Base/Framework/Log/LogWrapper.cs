using System;
using System.Collections.Generic;
using System.Text;
using log4net;

namespace Sinan.Log
{
    /// <summary>
    /// information level
    /// </summary>
    public enum Level
    {
        //Trace,
        Debug = 1,
        Info,
        Warn,
        Error,
        Fatal
    }

    /// <summary>
    /// log class
    /// </summary>
    public static class LogWrapper
    {
        private static ILog logger = LogManager.GetLogger("Server");
        //public static Logger Logger
        //{
        //    get { return logger; }
        //}

        ///// <summary>
        ///// log message, level is debug
        ///// </summary>
        ///// <param name="message"></param>
        //public static void Trace(string message)
        //{
        //    try
        //    {
        //        if (logger == null) return;
        //        logger.Info(message);
        //    }
        //    catch { }
        //}

        /// <summary>
        /// log message, level is debug
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            try
            {
                //Console.WriteLine(message); return;
                if (logger == null) return;
                logger.Debug(message);
            }
            catch { }
        }

        /// <summary>
        /// log message, level is debug
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(Exception ex)
        {
            try
            {
                if (logger == null) return;
                logger.Debug(ex.Message + ex.StackTrace);
            }
            catch { }
        }

        /// <summary>
        /// log message, level is error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Debug(string message, Exception exception)
        {
            try
            {
                //Console.WriteLine(message + exception.Message); return;
                if (logger == null) return;
                logger.Debug(message, exception);
            }
            catch { }
        }

        /// <summary>
        /// log message, level is Info
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            try
            {
                //Console.WriteLine(message); return;
                if (logger == null) return;
                logger.Info(message);
            }
            catch { }
        }

        /// <summary>
        /// log message, level is Info
        /// </summary>
        /// <param name="message"></param>
        public static void Info(Exception ex)
        {
            try
            {
                if (logger == null) return;
                logger.Info(ex.Message + ex.StackTrace);
            }
            catch { }
        }

        /// <summary>
        /// log message, level is error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Info(string message, Exception exception)
        {
            try
            {
                //Console.WriteLine(message + exception.Message); return;
                if (logger == null) return;
                logger.Info(message, exception);
            }
            catch { }
        }

        /// <summary>
        /// log message, level is warn
        /// </summary>
        /// <param name="message"></param>
        public static void Warn(string message)
        {
            try
            {
                //Console.WriteLine(message); return;
                if (logger == null) return;
                logger.Warn(message);
            }
            catch { }
        }

        /// <summary>
        /// log message, level is Info
        /// </summary>
        /// <param name="message"></param>
        public static void Warn(Exception ex)
        {
            try
            {
                if (logger == null) return;
                logger.Warn(ex.Message + ex.StackTrace);
            }
            catch { }
        }

        /// <summary>
        /// log message, level is error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Warn(string message, Exception exception)
        {
            try
            {
                //Console.WriteLine(message + exception.Message); return;
                if (logger == null) return;
                logger.Warn(message, exception);
            }
            catch { }
        }


        /// <summary>
        /// log message, level is error
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            try
            {
                //Console.WriteLine(message); return;
                if (logger == null) return;
                logger.Error(message);
            }
            catch { }
        }


        /// <summary>
        /// log message, level is Info
        /// </summary>
        /// <param name="message"></param>
        public static void Error(Exception ex)
        {
            try
            {
                if (logger == null) return;
                logger.Error(ex.Message + ex.StackTrace);
            }
            catch { }
        }

        /// <summary>
        /// log message, level is error
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Error(string message, Exception exception)
        {
            try
            {
                //Console.WriteLine(message + exception.Message); return;
                if (logger == null) return;
                logger.Error(message, exception);
            }
            catch { }
        }

        /// <summary>
        /// log message, level is fatal
        /// </summary>
        /// <param name="message"></param>
        public static void Fatal(string message)
        {
            try
            {
                //Console.WriteLine(message); return;
                if (logger == null) return;
                logger.Fatal(message);
            }
            catch { }
        }


        /// <summary>
        /// log message, level is Info
        /// </summary>
        /// <param name="message"></param>
        public static void Fatal(Exception ex)
        {
            try
            {
                if (logger == null) return;
                logger.Fatal(ex.Message + ex.StackTrace);
            }
            catch { }
        }

        /// <summary>
        /// log message, level is fatal
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Fatal(string message, Exception exception)
        {
            try
            {
                //Console.WriteLine(message + exception.Message); return;
                if (logger == null) return;
                logger.Fatal(message, exception);
            }
            catch { }

        }

        /// <summary>
        /// log message with specified level
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        public static void LogMessage(string message, Level level)
        {
            try
            {
                if (logger == null) return;
                if (string.IsNullOrEmpty(message)) return;

                #region logger message
                switch (level)
                {
                    //case Level.Trace:
                    //    logger.Info(message);
                    //    break;
                    case Level.Debug:
                        logger.Debug(message);
                        break;
                    case Level.Info:
                        logger.Info(message);
                        break;
                    case Level.Warn:
                        logger.Warn(message);
                        break;
                    case Level.Error:
                        logger.Error(message);
                        break;
                    case Level.Fatal:
                        logger.Fatal(message);
                        break;
                    default:
                        break;
                }
                #endregion
            }
            catch { }
        }

        /// <summary>
        /// log message with specified level
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        public static void LogMessage(Exception message, Level level)
        {
            try
            {
                if (logger == null || message == null) return;
                #region logger message
                switch (level)
                {
                    //case Level.Trace:
                    //    logger.Info(message);
                    //    break;
                    case Level.Debug:
                        logger.Debug(message);
                        break;
                    case Level.Info:
                        logger.Info(message);
                        break;
                    case Level.Warn:
                        logger.Warn(message);
                        break;
                    case Level.Error:
                        logger.Error(message);
                        break;
                    case Level.Fatal:
                        logger.Fatal(message);
                        break;
                    default:
                        break;
                }
                #endregion
            }
            catch { }
        }
    }
}
