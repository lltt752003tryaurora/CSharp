using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Utilities
{
    public class WebServiceAuthentication
    {
        /// <summary>
        /// Check authenticate webservice call
        /// Appsettings Config Key = wsAccount_wsPassword, 
        /// Value:
        /// item(0) = dataSignKey
        /// item(1-n)=method1,method2,....,method(n)
        /// </summary>
        /// <param name="wsAccount"></param>
        /// <param name="wsPassword"></param>
        /// <param name="method"></param>
        /// <param name="dataSignKey"></param>
        /// <param name="errorCode"></param>
        /// <returns>
        /// true = success, dataSignKey != empty, errorCode == empty
        /// false = false, errorCode != empty, dataSignKey == empty
        /// </returns>
        public static bool Authenticate(string wsAccount, string wsPassword, string method, out string dataSignKey, out string errorCode)
        {
            dataSignKey = string.Empty;
            errorCode = string.Empty;
            try
            {
                string configValue = ConfigHelper.GetConfig(string.Format("{0}_{1}", wsAccount, wsPassword), string.Empty);
                if (string.IsNullOrEmpty(configValue))
                {
                    errorCode = ErrorCode.AUTHENTICATE_FAIL.ToString();
                    return false;
                }
                else
                {
                    string[] methods = configValue.Split(',');
                    if (methods.Length == 0)
                    {
                        errorCode = ErrorCode.AUTHENTICATE_FAIL.ToString();
                        return false;
                    }
                    if (methods.Length == 1 || !methods.Contains(method))
                    {
                        errorCode = ErrorCode.NOT_HAVE_PERMISSION_CALL_METHOD.ToString();
                        return false;
                    }
                    dataSignKey = methods[0];
                    return true;
                }
            }
            catch (Exception)
            {
                errorCode = ErrorCode.UNHANDLE_ERROR.ToString();
                return false;
            }
        }

        public static bool CheckDataSign(params object[] list)
        {
            if (list.Length < 2) return false;

            string dataSign = list[0].ToString();
            string dataSignKey = list[1].ToString();
            StringBuilder sb = new StringBuilder();
            for (int i = 2; i < list.Length; i++)
            {
                string value = list[i].ToString();
                sb.Append(value);
            }
            sb.Append(dataSignKey);
            if (Common.MD5(sb.ToString()) != dataSign)
            {
                return false;
            }
            return true;
        }

        #region v1
        public static bool Authenticate(string aid, string method, out string ckKey, out string errorCode, out string ips)
        {
            ckKey = string.Empty;
            errorCode = string.Empty;
            ips = string.Empty;
            try
            {
                string configValue = ConfigHelper.GetConfig(string.Format("{0}", aid), string.Empty);
                if (string.IsNullOrEmpty(configValue))
                {
                    errorCode = APIEC.AUTHENTICATE_FAIL.ToString();
                    return false;
                }
                else
                {
                    string[] methods = configValue.Split(',');
                    if (methods.Length == 0)
                    {
                        errorCode = APIEC.AUTHENTICATE_FAIL.ToString();
                        return false;
                    }
                    if (methods.Length == 1 || !methods.Contains(method))
                    {
                        errorCode = APIEC.NOT_HAVE_PERMISSION_CALL_METHOD.ToString();
                        return false;
                    }
                    ckKey = methods[0];
                    ips = methods[1];
                    return true;
                }
            }
            catch (Exception)
            {
                errorCode = APIEC.UNHANDLE_ERROR.ToString();
                return false;
            }
        }

        public static bool ValidateChecksum(params object[] list)
        {
            if (list.Length < 2) return false;

            string dataSign = list[0].ToString();
            string dataSignKey = list[1].ToString();
            StringBuilder sb = new StringBuilder();
            for (int i = 2; i < list.Length; i++)
            {
                string value = list[i].ToString();
                sb.AppendFormat("{0}|", value);
            }
            sb.Append(dataSignKey);
            if (Common.SHA512(sb.ToString()) != dataSign)
            {
                return false;
            }
            return true;
        }
        public static string GetRawChecksum(params object[] list)
        {
            if (list == null || list.Length <= 0) return string.Empty;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < list.Length; i++)
            {
                string value = list[i].ToString();
                sb.AppendFormat("{0}|",value);
            }
            
            return sb.ToString().TrimEnd('|');
        }

        public static bool ValidationInput(List<object> lstParam, List<string> lstNameParam, out string sReturn)
        {
            bool bolRerult = true;
            StringBuilder sbReturn = new StringBuilder();
            if (lstParam.Count != lstNameParam.Count)
            {
                sReturn = "Wrapper error";
                return false;
            }
            Dictionary<string, int> typeDict = new Dictionary<string, int>{
                    {typeof(string).FullName,0},
                    {typeof(int).FullName,1},
                    {typeof(long).FullName,2},
                    {typeof(double).FullName,3},
                    {typeof(decimal).FullName,4},
                    {typeof(object).FullName,5}
            };
            for (int i = 0; i < lstParam.Count; i++)
            {
                Type type = lstParam[i].GetType();
                if (typeDict[lstParam[i].GetType().FullName] == 0 && string.IsNullOrEmpty(lstParam[i].ToString()))
                {
                    bolRerult = false;
                    sbReturn.Append(lstNameParam[i].Trim() + ",");
                }
                else if (typeDict[lstParam[i].GetType().FullName] == 1 && Convert.ToInt32(lstParam[i]) <= 0)
                {
                    bolRerult = false;
                    sbReturn.Append(lstNameParam[i].Trim() + ",");
                }
                else if (typeDict[lstParam[i].GetType().FullName] == 2 && Convert.ToInt64(lstParam[i]) <= 0)
                {
                    bolRerult = false;
                    sbReturn.Append(lstNameParam[i].Trim() + ",");
                }
                else if (typeDict[lstParam[i].GetType().FullName] == 3 && Convert.ToDouble(lstParam[i]) <= 0)
                {
                    bolRerult = false;
                    sbReturn.Append(lstNameParam[i].Trim() + ",");
                }
            }
            if (bolRerult == false)
                sReturn = sbReturn.ToString().TrimEnd(',');
            else sReturn = "";
            return bolRerult;
        }
        #endregion v1
    }
}
