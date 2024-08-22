using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Utilities
{
    [Serializable]
    public class SSOPGD
    {
        public List<Dictionary<string,object>> Items { get; set; }
    }
    public class SSOHelper
    {
        public static string CreateURL(Dictionary<string, string> paras)
        {

            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> pair in paras)
            {
                sb.Append(pair.Key + "=" + pair.Value + "&");
            }
            string url = sb.ToString();
            url = url.Remove(url.LastIndexOf("&"));
            return url;
        }
        public static string HttpPost(string url, string Parameters)
        {
            HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(url);
            //Add these, as we're doing a POST
            req.ContentType = "application/x-www-form-urlencoded";//"application/json";// "application/x-www-form-urlencoded";
            //req.Accept = "application/json";
            req.Method = "POST";
            //We need to count how many bytes we're sending. Post'ed Faked Forms should be name=value&
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(Parameters);
            req.ContentLength = bytes.Length;
            System.IO.Stream os = req.GetRequestStream();
            os.Write(bytes, 0, bytes.Length); //Push it out there
            os.Close();
            System.Net.WebResponse resp = req.GetResponse();
            if (resp == null) return null;
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            return sr.ReadToEnd().Trim();
        }
        public static string HttpRequest(string url, string action, Dictionary<string, string> headers, Dictionary<string, string> parameters, string contentType = "")
        {
            string sParam = CreateURL(parameters);

            if(url.Contains("?"))
            {
                url += "&" + sParam;
            }
            else
            {
                url += "?" + sParam;
            }
            HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(url);
            //Add these, as we're doing a POST
            if(!string.IsNullOrEmpty(contentType))
            {
                req.ContentType = contentType;//"application/json";// "application/x-www-form-urlencoded";
                                              //req.Accept = "application/json";
            }
            else
            {
                req.ContentType = null;
                req.Accept = null;
            }

            req.Method = action;
            foreach(KeyValuePair<string, string> kv in headers)
            {
                req.Headers.Add(kv.Key, kv.Value);
            }

            //We need to count how many bytes we're sending. Post'ed Faked Forms should be name=value&
            if(action.ToLower() == "post")
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(sParam);
                req.ContentLength = bytes.Length;
                System.IO.Stream os = req.GetRequestStream();
                os.Write(bytes, 0, bytes.Length); //Push it out there
                os.Close();
            }
            System.Net.WebResponse resp = req.GetResponse();
            if (resp == null) return null;
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            return sr.ReadToEnd().Trim();
        }
    }
}
