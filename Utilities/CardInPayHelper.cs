using _1Pay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class CardInPayHelper
    {
        public static int increment = 0;
        public class CardType
        {
            public const string Viettel = "VIETEL";
            public const string Mobifone = "VMS";
            public const string Vinaphone = "GPC";
        }
        public class CardInPayResponse : CardBaseResponse
        {
            public const string SUCC = "succ";
            public const string FAIL = "fail";
            public const string PENDING = "pending";
            public CardInPayResponse()
            {
                RStatus = new Dictionary<string, string>()
                {
                    {"0","Thành công"},
                    {"1","Dữ liệu sai định dạng"},//Ban tin request sai dinh dang
                    {"2","Giải mã dữ liệu không thành công"},//Giai ma du lieu khong thanh cong
                    {"3","IP không được phép truy cập"},//IP khong duoc phep ket noi
                    {"4","SubCPID khong chinh xac"},
                    {"5","Password khong hop le"},
                    {"6","TransId khong hop le hoac bi trung"},
                    {"7","Khong tim thay TransId nay"},
                    {"8","Sai user/pass vuot so lan cho phep"},
                    {"9","Tham so Telco khong hop le"},
                    {"10","Doi tac da bi lock"},
                    {"11","Loi ket noi DB"},
                    {"12","Timeout"},
                    {"13","Game code cua SubCP khong hop le"},
                    {"14","Telco khong hoi dap"},
                    {"49","Unknown"},
                    {"50","Card da duoc su dung truoc do"},
                    {"51","Card khong ton tai"},
                    {"52","Dinh dang card khong dung"},
                    {"53","Card/Serial khong hop le"},
                    {"54","Card da qua han"},
                    {"55","Card dang duoc xu ly"}
                };
            }
            public override string GetTransactionStatus(string code)
            {
                if (code == "12" || code == "14" || code == "49" || code == "55")
                {
                    return PENDING;
                }
                if (code == "0")
                {
                    return SUCC;
                }
                return FAIL;
            }
            public override void SetResponse(string cardType, string response)
            {
                try
                {
                    Dictionary<string, object> jObj = JsonHelper.JsonString2Dic(response);
                    this.TelcoCode = cardType;
                    this.ProviderTransactionId = "";// JsonHelper.GetValue(jObj, "transId");
                    this.TransactionId = "";// JsonHelper.GetValue(jObj, "transRef");
                    this.SerialNo = "";// JsonHelper.GetValue(jObj, "serial");
                    this.ResponseStatus = JsonHelper.GetValue(jObj, "errorCode");
                    this.Amt = NumberHelper.ConvertToInt(JsonHelper.GetValue(jObj, "amount"));
                    this.Description = JsonHelper.GetValue(jObj, "errorDesc");
                }
                catch { }
            }
        }
        public static string GetId()
        {
            string i = "";
            increment++;
            if (increment > 999) increment = 0;

            i = increment.ToString();
            if (i.Length == 1)
            {
                i = "00" + i;
            }
            else if (i.Length == 2)
            {
                i = "0" + i;
            }
            return i;
        }
        public static string CardTopup(string type, string gameCode, string serial, string code, string transId)
        {
            String result = "";
            String subcpId = ConfigHelper.GetConfig("CardInPayID");
            String url = ConfigHelper.GetConfig("CardInPayUrl");
            String accessKey = ConfigHelper.GetConfig("CardInPayAccessKey");//building password for request
            String secretKey = ConfigHelper.GetConfig("CardInPaySecretKey");//128 AES
            String signature = Common.MD5(serial + code + accessKey); //create signature

            serial = Encrypt(serial, secretKey);
            code = Encrypt(code, secretKey);

            Dictionary<string, object> datas = new Dictionary<string, object>();
            datas.Add("subcpId", subcpId);
            datas.Add("gameCode", gameCode);
            datas.Add("password", signature);
            datas.Add("serial", serial);
            datas.Add("code", code);
            datas.Add("telco", type);
            datas.Add("action", "CHARGE");
            datas.Add("extInfo", "");
            datas.Add("transId", transId);

            string sPost = JsonHelper.Dic2JsonString(datas);
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Accept = "application/json";
                request.UserAgent = "Mozilla/5.0";
                WebHeaderCollection headerReader = request.Headers;
                headerReader.Add("Accept-Language", "en-US,en;q=0.5");
                var data = Encoding.ASCII.GetBytes(sPost);
                request.ContentLength = data.Length;
                Stream requestStream = request.GetRequestStream();
                // send url param
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
            return result;
        }
        public static string CardQuery(string type, string gameCode, string serial, string code, string transId)
        {
            String result = "";
            String subcpId = ConfigHelper.GetConfig("CardInPayID");
            String url = ConfigHelper.GetConfig("CardInPayUrl");
            String accessKey = ConfigHelper.GetConfig("CardInPayAccessKey");//building password for request
            String secretKey = ConfigHelper.GetConfig("CardInPaySecretKey");//128 AES
            String signature = Common.MD5(serial + code + accessKey); //create signature

            serial = Encrypt(serial, secretKey);
            code = Encrypt(code, secretKey);

            Dictionary<string, object> datas = new Dictionary<string, object>();
            datas.Add("subcpId", subcpId);
            datas.Add("gameCode", gameCode);
            datas.Add("password", signature);
            datas.Add("serial", serial);
            datas.Add("code", code);
            datas.Add("telco", type);
            datas.Add("action", "CHARGE");
            datas.Add("extInfo", "");
            datas.Add("transId", transId);

            string sPost = JsonHelper.Dic2JsonString(datas);
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Accept = "application/json";
                request.UserAgent = "Mozilla/5.0";
                WebHeaderCollection headerReader = request.Headers;
                headerReader.Add("Accept-Language", "en-US,en;q=0.5");
                var data = Encoding.ASCII.GetBytes(sPost);
                request.ContentLength = data.Length;
                Stream requestStream = request.GetRequestStream();
                // send url param
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                result = new StreamReader(response.GetResponseStream()).ReadToEnd();
                response.Close();
            }
            catch (Exception e)
            {
                //result = e.GetBaseException().ToString();
                throw e;
            }
            return result;
        }
        static string Decrypt(string textToDecrypt, string key)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;

            rijndaelCipher.KeySize = 0x80;
            rijndaelCipher.BlockSize = 0x80;
            byte[] encryptedData = Convert.FromBase64String(textToDecrypt);
            byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
            byte[] keyBytes = new byte[0x10];
            int len = pwdBytes.Length;
            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }
            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
            byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            return Encoding.UTF8.GetString(plainText);
        }

        static string Encrypt(string textToEncrypt, string key)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;

            rijndaelCipher.KeySize = 0x80;
            rijndaelCipher.BlockSize = 0x80;
            byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
            byte[] keyBytes = new byte[0x10];
            int len = pwdBytes.Length;
            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }
            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
            byte[] plainText = Encoding.UTF8.GetBytes(textToEncrypt);
            return Convert.ToBase64String(transform.TransformFinalBlock(plainText, 0, plainText.Length));
        }
    }
}
