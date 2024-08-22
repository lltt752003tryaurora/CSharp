using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class NumberHelper
    {
        public const int NULL_VALUE = -999999;
        public static int ConvertToInt(object input)
        {
            int output = 0;
            if (input == null) return output;
            int.TryParse(input.ToString(), out output);
            return output;
        }
        public static byte ConvertToByte(object input)
        {
            byte output = 0;
            if (input == null) return output;
            byte.TryParse(input.ToString(), out output);
            return output;
        }
        public static long ConvertToLong(object input)
        {
            long output = 0;
            if (input == null) return output;
            long.TryParse(input.ToString(), out output);
            return output;
        }
        public static decimal ConvertToDecimal(object input)
        {
            decimal output = 0;
            if (input == null) return output;
            decimal.TryParse(input.ToString(), out output);
            return output;
        }
        public static bool ConvertToDecimal(object input, out decimal output)
        {
            output = 0;
            if (input == null) return false;

            return decimal.TryParse(input.ToString(), out output);
        }
        public static string ViewNumber(object input, string formatNumber = "")
        {
            if (string.IsNullOrEmpty(formatNumber)) formatNumber = "#,##";
            decimal output = 0;
            if (input == null) return output.ToString();
            decimal.TryParse(input.ToString(), out output);
            output = Math.Round(output);
            string o = output.ToString(formatNumber, CultureInfo.InvariantCulture);
            o = o.Replace(' ', ',');
            o = o.Replace('.', ',');
            return o;
        }
        public static string ViewNumberInput(object input, string formatNumber)
        {
            if (string.IsNullOrEmpty(formatNumber)) formatNumber = "#,##0";
            decimal output = 0;
            if (input == null) return output.ToString();
            decimal.TryParse(input.ToString(), out output);
            output = Math.Round(output);
            return output.ToString();
        }
        public static double ConvertToDouble(object input)
        {
            double output = 0;
            if (input == null) return output;
            double.TryParse(input.ToString(), out output);
            return output;
        }
        public static bool IsNumber(string number)
        {
            try
            {
                Convert.ToInt32(number);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
