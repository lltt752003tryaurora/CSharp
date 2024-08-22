using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class Card789Helper
    {
        public static string SHA512(string sToEncrypt)
        {
            System.Security.Cryptography.SHA512 sha = new SHA512CryptoServiceProvider();
            byte[] bytes = Encoding.UTF8.GetBytes(sToEncrypt);
            bytes = sha.ComputeHash(bytes);
            StringBuilder builder = new StringBuilder();
            foreach (byte num in bytes)
            {
                builder.Append(num.ToString("x2"));
            }
            return builder.ToString();
        }

        public static byte[] ToByte(string strIni)
        {
            return Encoding.UTF8.GetBytes(strIni);
        }
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
        public static string HttpPost(string URI, string Parameters)
        {
            HttpWebRequest req = (HttpWebRequest)System.Net.WebRequest.Create(URI);
            //Add these, as we're doing a POST
            req.ContentType = "application/x-www-form-urlencoded";
            req.Accept = "application/json";
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
        public static string CreateRawChecksum(Dictionary<string, string> parameters)
        {
            string str = "";
            foreach (var item in parameters)
            {
                str += item.Value.ToString().Trim() + "|";
            }

            return str.TrimEnd('|');
        }
    }
}
