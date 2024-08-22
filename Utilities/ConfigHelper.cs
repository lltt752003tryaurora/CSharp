using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Utilities
{
    public class ConfigHelper
    {
        /// <summary>
        /// Gets the config.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static int GetConfig(string key, int defaultValue)
        {
            try
            {
                string value = ConfigurationManager.AppSettings[key];
                return NumberHelper.ConvertToInt(value);
            }
            catch
            {
                return defaultValue;
            }
        }
        public static string GetConfig(string key)
        {
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch
            {
                return string.Empty;
            }
        }
        /// <summary>
        /// Gets the config.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        public static string GetConfig(string key, string defaultValue)
        {
            try
            {
                return ConfigurationManager.AppSettings[key];
            }
            catch
            {
                return defaultValue;
            }
        }

        public static long GetConfig(string key, long defaultValue)
        {
            try
            {
                return NumberHelper.ConvertToLong(ConfigurationManager.AppSettings[key]);
            }
            catch
            {
                return defaultValue;
            }
        }
        public static decimal GetConfig(string key, decimal defaultValue)
        {
            try
            {
                return NumberHelper.ConvertToDecimal(ConfigurationManager.AppSettings[key]);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
