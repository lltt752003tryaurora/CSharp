using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Config;
using System.Configuration;

namespace PaymentChannel.Utility
{
    public class Logger
    {
        private static readonly ILog successLogger = LogManager.GetLogger("SuccessLog");
        private static readonly ILog errorLoger = LogManager.GetLogger("ErrorLog");
        private static readonly ILog exceptionLogger = LogManager.GetLogger("ExceptionLog");
        private static readonly ILog infoLogger = LogManager.GetLogger("InfoLog");
        static Logger()
        {
            XmlConfigurator.Configure();
        }
        /// <summary>
        /// Create log format by list of param
        /// </summary>
        /// <exception>
        /// do not use try catch
        /// </exception>
        public static string CreateDataMessage(params object[] list)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Length; i++)
            {
                string value = list[i].ToString();
                value = value.Replace(';', ' ');
                sb.Append(value);
                if (i < list.Length - 1)
                {
                    sb.Append(";");
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// Create log message for web service
        /// </summary>
        /// <exception>
        /// do not use try catch
        /// </exception>
        public static string CreateLogMessage(
            string serverId,
            string serviceName,
            string functionName,
            string productId,
            string code,
            double duration,
            string serverIp,
            string clientIp,
            string serviceAccount,
            string data)
        {
            string logFormat = "{0};{1};{2};{3};{4};{5};{6};{7};{8};{9}";
            return string.Format(logFormat, serverId, serviceName, functionName, productId, code, duration, serverIp, clientIp, serviceAccount, data);
        }
        /// <summary>
        /// Append all the rest string param to first string param
        /// </summary>
        /// <exception>
        /// do not use try catch
        /// </exception>
        public static string Append(params object[] list)
        {
            if (list == null || list.Length == 0) return "";
            if (list.Length == 1) return list[0].ToString();

            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < list.Length; i++)
            {
                string value = list[i].ToString();
                value = value.Replace(';', ' ');
                sb.Append(value);
                if (i < list.Length - 1)
                {
                    sb.Append(";");
                }
            }
            return list[0].ToString() + ";" + sb.ToString();
        }
        /// <summary>
        /// Create log message for windows service
        /// </summary>
        /// <exception>
        /// Try to write log info and return NULL string
        /// </exception> 
        public static string CreateLogMessage(DateTime startDate, string functionName, string code, string data)
        {
            try
            {
                string serverId = ConfigurationManager.AppSettings["serverId"];
                string serviceName = ConfigurationManager.AppSettings["serviceName"];
                DateTime endDate = DateTime.Now;
                TimeSpan t = endDate.Subtract(startDate);
                string serverIp = "";
                return Logger.CreateLogMessage(serverId, serviceName, functionName, "-1", code, t.TotalMilliseconds, serverIp, "", "", data);
            }
            catch (Exception ex)
            {
                Logger.WriteInfo("====================================================Create log data exception:" + ex.Message);
                return "NULL";
            }
        }
        /// <summary>
        /// Write log success
        /// </summary>
        /// <exception>
        /// Do nothing if has exception
        /// </exception> 
        public static void WriteLogSuccess(string message)
        {
            try
            {
                successLogger.Info(message);
            }
            catch
            { 
                //do nothing
            }
        }
        /// <summary>
        /// Write log when you get a business error
        /// </summary>
        /// <exception>
        /// Do nothing if has exception
        /// </exception> 
        public static void WriteLogError(string message)
        {
            try
            {
                errorLoger.Error(message);
            }
            catch
            {
                //do nothing
            }
        }
        /// <summary>
        /// Write log when you get an exception error
        /// </summary>
        /// <exception>
        /// Do nothing if has exception
        /// </exception> 
        public static void WriteLogException(string message)
        {
            try
            {
                message = message.Replace(Environment.NewLine, " ");
                exceptionLogger.Error(message);
            }
            catch
            {
                //do nothing
            }
        }
        /// <summary>
        /// Write log when you want to keep track info
        /// </summary>
        /// <exception>
        /// Do nothing if has exception
        /// </exception> 
        public static void WriteInfo(string message)
        {
            try
            {
                infoLogger.Info(message);
            }
            catch
            {
                //do nothing
            }
        }
    }
}
