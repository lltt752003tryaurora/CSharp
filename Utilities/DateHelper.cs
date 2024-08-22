using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class DateHelper
    {
        public static DateTime ConvertToDate(string inputDate, string format, int h = 0, int m = 0, int s = 0)
        {
            try
            {
                DateTime date = DateTime.Now;
                switch (format)
                {
                    case "dd/MM/yyyy":
                    case "dd/mm/yyyy":
                        string[] sv1 = inputDate.Split("/".ToCharArray());
                        date = new DateTime(int.Parse(sv1[2]), int.Parse(sv1[1]), int.Parse(sv1[0]), h, m, s);
                        break;
                    case "dd/MM/yyyy hh:mm:ss":
                        string[] lst = inputDate.Split(" ".ToCharArray());
                        string[] sd = lst[0].Split("/".ToCharArray());
                        string[] st = lst[1].Split(":".ToCharArray());
                        h = NumberHelper.ConvertToInt(st[0]);
                        m = NumberHelper.ConvertToInt(st[1]);
                        s = NumberHelper.ConvertToInt(st[2]);
                        date = new DateTime(int.Parse(sd[2]), int.Parse(sd[1]), int.Parse(sd[0]), h, m, s);
                        break;
                    default:
                        throw new Exception("Format is invalid");
                }
                return date;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static DateTime ConvertToDate(object inputDate)
        {
            DateTime date = new DateTime(1900, 01, 01, 0, 0, 0);
            try
            {
                date = (DateTime)inputDate;
            }
            catch (Exception ex)
            {
                date = new DateTime(1900, 01, 01, 0, 0, 0);
            }
            return date;
        }
        public static bool ConvertTimeStampToDateTime(string inputTime, out DateTime expectedDate)
        {
            try
            {
                expectedDate = DateTime.MinValue;
                double convertInput = NumberHelper.ConvertToDouble(inputTime);
                if (convertInput == 0)
                {
                    return false;
                }
                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                if (inputTime.Length > 10)
                {
                    expectedDate = origin.AddMilliseconds(convertInput);
                }
                else
                {
                    expectedDate = origin.AddSeconds(convertInput).ToLocalTime();
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static long GetTimeStamp(DateTime date)
        {
            try
            {
                long unixTimestamp = date.Ticks - new DateTime(1970, 1, 1).Ticks;
                unixTimestamp /= TimeSpan.TicksPerSecond;
                return unixTimestamp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static long CurrentMilisecond()
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long ms = (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
            return ms;
        }
    }
}
