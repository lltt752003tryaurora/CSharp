using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using System.Net.Mail;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Utilities
{
    public class Common
    {
        public static string MD5(string sToEncrypt)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] arrText = System.Text.Encoding.UTF8.GetBytes(sToEncrypt);
            arrText = md5.ComputeHash(arrText);
            StringBuilder sBuilder = new StringBuilder();
            foreach (byte bi in arrText)
            {
                sBuilder.Append(bi.ToString("x2"));
            }
            return sBuilder.ToString();
        }
        public static string PostToWeb(string postData, string url)
        {
            string responseFromServer = string.Empty;
            if (string.IsNullOrEmpty(url)) url = ConfigHelper.GetConfig("SynDataToWebLink", "");
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = byteArray.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                WebResponse response = request.GetResponse();
                dataStream = response.GetResponseStream();

                StreamReader reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();
                responseFromServer = responseFromServer.Trim();
                reader.Close();
                dataStream.Close();
                response.Close();
                return responseFromServer;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static string SHA1(string sToEncrypt)
        {
            System.Security.Cryptography.SHA1 sha = new SHA1CryptoServiceProvider();
            byte[] bytes = Encoding.UTF8.GetBytes(sToEncrypt);
            bytes = sha.ComputeHash(bytes);
            StringBuilder builder = new StringBuilder();
            foreach (byte num in bytes)
            {
                builder.Append(num.ToString("x2"));
            }
            return builder.ToString();
        }

        public static string SHA256(string sToEncrypt)
        {
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider();
            byte[] arrText = System.Text.Encoding.UTF8.GetBytes(sToEncrypt);
            arrText = sha256.ComputeHash(arrText);
            StringBuilder sBuilder = new StringBuilder();
            foreach (byte bi in arrText)
            {
                sBuilder.Append(bi.ToString("x2"));
            }
            return sBuilder.ToString();
        }

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

        public static string GetString(byte[] byteIni)
        {
            return Encoding.UTF8.GetString(byteIni);
        }
        public static string EncryptTripleDES(string key, string data)
        {
            data = data.Trim();
            byte[] keydata = Encoding.ASCII.GetBytes(key);
            string md5String = BitConverter.ToString(new
            MD5CryptoServiceProvider().ComputeHash(keydata)).Replace("-", "").ToLower();
            byte[] tripleDesKey = Encoding.ASCII.GetBytes(md5String.Substring(0, 24));
            TripleDES tripdes = TripleDESCryptoServiceProvider.Create();
            tripdes.Mode = CipherMode.ECB;
            tripdes.Key = tripleDesKey;
            tripdes.GenerateIV();
            MemoryStream ms = new MemoryStream();
            CryptoStream encStream = new CryptoStream(ms, tripdes.CreateEncryptor(),
            CryptoStreamMode.Write);
            encStream.Write(Encoding.ASCII.GetBytes(data), 0,
            Encoding.ASCII.GetByteCount(data));
            encStream.FlushFinalBlock();
            byte[] cryptoByte = ms.ToArray();
            ms.Close();
            encStream.Close();
            return Convert.ToBase64String(cryptoByte, 0, cryptoByte.GetLength(0)).Trim();
        }


        public static string DecryptTripleDES(string key, string data)
        {
            byte[] keydata = System.Text.Encoding.ASCII.GetBytes(key);
            string md5String = BitConverter.ToString(new
            MD5CryptoServiceProvider().ComputeHash(keydata)).Replace("-", "").ToLower();
            byte[] tripleDesKey = Encoding.ASCII.GetBytes(md5String.Substring(0, 24));
            TripleDES tripdes = TripleDESCryptoServiceProvider.Create();
            tripdes.Mode = CipherMode.ECB;
            tripdes.Key = tripleDesKey;
            byte[] cryptByte = Convert.FromBase64String(data);
            MemoryStream ms = new MemoryStream(cryptByte, 0, cryptByte.Length);
            ICryptoTransform cryptoTransform = tripdes.CreateDecryptor();
            CryptoStream decStream = new CryptoStream(ms, cryptoTransform,
            CryptoStreamMode.Read);
            StreamReader read = new StreamReader(decStream);
            return (read.ReadToEnd());
        }

        public static string Base46Decode(string encodedString)
        {
            byte[] data = Convert.FromBase64String(encodedString);
            return Encoding.UTF8.GetString(data);
        }
        public static string Base46Encode(string rawString)
        {
            byte[] data = System.Text.ASCIIEncoding.UTF8.GetBytes(rawString);
            return System.Convert.ToBase64String(data);
        }

        /// <summary>
        /// Parse a JSON object and return it as a dictionary of strings with keys showing the heirarchy.
        /// </summary>
        /// <param name = "token"></param>
        /// <param name = "nodes"></param>
        /// <param name = "parentLocation"></param>
        /// <returns></returns>
        public static bool ParseJson(JToken token, Dictionary<string, string> nodes, string parentLocation = "")
        {
            if (token.HasValues)
            {
                foreach (JToken child in token.Children())
                {
                    if (token.Type == JTokenType.Property)
                    {
                        if (parentLocation == "")
                        {
                            parentLocation = ((JProperty)token).Name;
                        }
                        else
                        {
                            parentLocation += "." + ((JProperty)token).Name;
                        }
                    }

                    ParseJson(child, nodes, parentLocation);
                }

                // we are done parsing and this is a parent node
                return true;
            }
            else
            {
                // leaf of the tree
                if (nodes.ContainsKey(parentLocation))
                {
                    // this was an array
                    nodes[parentLocation] += "|" + token.ToString();
                }
                else
                {
                    // this was a single property
                    nodes.Add(parentLocation, token.ToString());
                }

                return false;
            }
        }

        //public static string RemoveUnicode(string text)
        //{
        //    string[] arr1 = new string[] { "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
        //    "đ",
        //    "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
        //    "í","ì","ỉ","ĩ","ị",
        //    "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
        //    "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
        //    "ý","ỳ","ỷ","ỹ","ỵ",};
        //            string[] arr2 = new string[] { "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
        //    "d",
        //    "e","e","e","e","e","e","e","e","e","e","e",
        //    "i","i","i","i","i",
        //    "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
        //    "u","u","u","u","u","u","u","u","u","u","u",
        //    "y","y","y","y","y",};
        //    for (int i = 0; i < arr1.Length; i++)
        //    {
        //        text = text.Replace(arr1[i], arr2[i]);
        //        text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
        //    }
        //    return text;
        //}

        public static string RemoveUnicode(string s)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string temp = s.Normalize(NormalizationForm.FormD);
            return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

    }
}
