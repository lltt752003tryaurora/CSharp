using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Utilities
{
    public class JsonHelper
    {
        public static string GetValue(Dictionary<string, object> dic, string key, string defauleValue = "")
        {
            if (dic == null) return string.Empty;
            if (dic.ContainsKey(key))
            {
                if(dic[key] == null)
                {
                    return string.Empty;
                }
                return dic[key].ToString();
            }
            return defauleValue;

        }
        public static string Dic2JsonString(Dictionary<string, object> dicData)
        {
            try
            {
                return JsonConvert.SerializeObject(dicData);
            }
            catch
            {
                return "";
            }

        }
        public static Dictionary<string, object> JsonString2Dic(string jsonParams)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonParams)) return new Dictionary<string, object>();
                Dictionary<string, object> dicData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonParams);
                return dicData;
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }
        public static List<Dictionary<string, object>> JsonString2List(string jsonParams)
        {
            try
            {
                List<Dictionary<string, object>> dicData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonParams);
                return dicData;

            }
            catch
            {
                return new List<Dictionary<string, object>>();
            }
        }
        public static string ToJsonString(object o)
        {
            try
            {
                return JsonConvert.SerializeObject(o);
            }
            catch
            {
                return "";
            }

        }
    }
}
