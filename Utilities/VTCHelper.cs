using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class BasePaymentResponse
    {


        public Dictionary<string, string> DicCode = new Dictionary<string, string>();
        public Dictionary<string, string> DicFail = new Dictionary<string, string>()
        {
        };
        public Dictionary<string, string> DicPending = new Dictionary<string, string>()
        {
        };
        public Dictionary<string, string> DicSucc = new Dictionary<string, string>()
        {
        };
    }
    public class VTCResponse
    {
        public const string SUCC = "succ";
        public const string FAIL = "fail";
        public const string PENDING = "pending";
        public Dictionary<string, string> RStatus = new Dictionary<string, string>()
        {
            {"-1","Thẻ đã sử dụng"},
            {"-2","Thẻ đã bị khóa"},
            {"-3","Thẻ hết hạn sử dụng"},
            {"-4","Thẻ chưa kích hoạt"},
            {"-5","TransID không hợp lệ"},
            {"-6","Mã thẻ và số Serial không khớp"},
            {"-8","Cảnh báo số lần giao dịch lỗi của một tài khoản"},
            {"-9"," Thẻ thử quá số lần cho phép"},
            {"-10","CardID không hợp lệ"},
            {"-11","CardCode không hợp lệ"},
            {"-12","Thẻ không tồn tại"},
            {"-13","Sai cấu trúc Description"},
            {"-14","Mã dịch vụ không tồn tại"},
            {"-15","Thiếu thông tin khách hàng"},
            {"-16","Mã giao dịch không hợp lệ"},
            {"-90","Sai tên hàm"},
            {"-98","Giao dịch thất bại do Lỗi hệ thống"},
            {"-99","Giao dịch thất bại do Lỗi hệ thống"},
            {"-999","Hệ thống Telco tạm ngừng"}
            // {"-100","Giao dịch nghi vấn (xác minh kết quả qua kênh đối soát)"}
        };
        public string GetVTCTransactionStatus(ref int amt)
        {
            if (this.ResponseStatus == "-100")
            {
                return PENDING;
            }
            if (RStatus.ContainsKey(this.ResponseStatus))
            {
                return FAIL;
            }
            try
            {
                amt = Convert.ToInt32(this.ResponseStatus);
                return SUCC;
            }
            catch {
                return PENDING;
            }
        }
        public string GetMessage()
        {
            try
            {
                return this.RStatus[this.ResponseStatus];
            }
            catch
            {
                return "";
            }
        }

        public string ResponseStatus
        {
            get; set;
        }
        public string Descripton
        {
            get; set;
        }
        public string TelcoCode
        {
            get; set;
        }
        public string VTCTranId
        {
            get; set;
        }
        public string AccountName
        {
            get; set;
        }
        public string VTCDescription
        {
            get; set;
        }
        public void SetResponse(string desc)
        {
            try
            {
                List<string> lst = desc.Split('|').ToList<string>();
                this.TelcoCode = lst[0];
                this.VTCTranId = lst[1];
                this.AccountName = lst[2];
                this.VTCDescription = lst[3];
            }
            catch { }
        }
    }
    public class VTCHelper
    {
        public static VTCResponse UseCard(string provider, string transid, string cardid, string cardcode, string nameuser)
        {
            VTCResponse rs = new VTCResponse();
            string fun = "UseCard";
            string telco = provider;

            string description = telco + "|" + transid + "|" + nameuser;
            string urlpost = ConfigHelper.GetConfig("VTCAPI.url");
            string key = ConfigHelper.GetConfig("VTCAPI.key");
            string partnerID = ConfigHelper.GetConfig("VTCAPI.partnerID");

            string xmlusercard = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n" +
            "<CardRequest>\n" +
            "<Function>" + fun + "</Function>\n" +
            "<CardID>" + cardid + "</CardID>\n" +
            "<CardCode>" + cardcode + "</CardCode>\n" +
            "<Description>" + description + "</Description>\n" +
            "</CardRequest>";

            string requestData = Encrypt(key, xmlusercard);
            var response = SendPostUtf8(xmlcardinvock(partnerID, requestData), urlpost);
            var xmlresultnotde = getValue(response, "RequestResult");
            var xmlresultde = Decrypt(key, xmlresultnotde);
            rs.ResponseStatus = getValue(xmlresultde, "ResponseStatus");
            rs.Descripton = getValue(xmlresultde, "Descripton");
            rs.SetResponse(rs.Descripton);
            CLogger.WriteInfo(xmlusercard + "|" + rs.ResponseStatus + "|" + rs.Descripton);
            return rs;
        }
        public static string xmlcardinvock(string PartnerID, string RequestData)
        {
            return "<?xml version=\"1.0\" encoding=\"utf-8\"?>"
                    + "<soap:Envelope xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">"
                    + "<soap:Body>"
                    + "<Request xmlns=\"VTCOnline.Card.WebAPI\">"
                    + "<PartnerID>" + PartnerID + "</PartnerID>"
                    + "<RequestData>" + RequestData + "</RequestData>"
                    + "</Request>"
                    + "</soap:Body>"
                    + "</soap:Envelope>";
        }
        public static string xmlvcoincard(string PartnerID, string RequestData)
        {
            return "<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:vtc=\"VTCOnline.Card.WebAPI\">" +
           "<soapenv:Header/>" +
           "<soapenv:Body>" +
              "<vtc:Request>" +
                 "<vtc:PartnerID>" + PartnerID + "</vtc:PartnerID>" +
                 "<vtc:RequestData>" + RequestData + "</vtc:RequestData>" +
              "</vtc:Request>" +
           "</soapenv:Body>" +
           "</soapenv:Envelope>";

        }
        public static string Encrypt(string key, string data)
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


        public static string Decrypt(string key, string data)
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
        public class TrustAllCertificatePolicy : System.Net.ICertificatePolicy
        {
            public TrustAllCertificatePolicy()
            { }

            public bool CheckValidationResult(ServicePoint sp,
              X509Certificate cert, WebRequest req, int problem)
            {
                return true;
            }
        }
        public static string getValue(string xml, string tagName)
        {
            string openTag = "<" + tagName + ">";
            string closeTag = "</" + tagName + ">";
            int f = xml.IndexOf(openTag) + openTag.Length;
            int l = xml.IndexOf(closeTag);
            return (f > l) ? "" : xml.Substring(f, l - f);
        }
        public static string SendPostUtf8(string postData, string url)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] data = encoding.GetBytes(postData);
            System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
            System.Net.ServicePointManager.Expect100Continue = false;
            CookieContainer cookie = new CookieContainer();
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(url);
            myRequest.Method = "POST";
            myRequest.ContentLength = data.Length;
            //myRequest.ContentType = "application/x-www-form-urlencoded";
            myRequest.ContentType = "text/xml; encoding='utf-8'";
            myRequest.UserAgent = "Jakarta Commons-HttpClient/3.1";
            myRequest.KeepAlive = false;
            myRequest.CookieContainer = cookie;
            myRequest.AllowAutoRedirect = false;


            using (Stream requestStream = myRequest.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
            }

            string responseXml = string.Empty;
            try
            {
                using (HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse())
                {
                    using (Stream respStream = myResponse.GetResponseStream())
                    {
                        using (StreamReader respReader = new StreamReader(respStream))
                        {
                            responseXml = respReader.ReadToEnd();
                        }
                    }
                }
            }
            catch (WebException webEx)
            {
                if (webEx.Response != null)
                {
                    using (HttpWebResponse exResponse = (HttpWebResponse)webEx.Response)
                    {
                        using (StreamReader sr = new StreamReader(exResponse.GetResponseStream()))
                        {
                            responseXml = sr.ReadToEnd();
                        }
                    }
                }
            }
            return responseXml;
        }
        /*
         protected void btnVcoin_Click(object sender, EventArgs e)
        {
            string fun = "UseCard";
            TextBox objcardid = (TextBox)txtcardidvcoin;
            string cardid = objcardid.Text;
            TextBox objcode = (TextBox)txtcardcodevcoin;
            string cardcode = objcode.Text;

            TextBox objusename = (TextBox)txtusernamevcoin;
            string username = objusename.Text;
            string description = username;
            string key = "868686@ABC";
            string partnerID = "868686";
            string urlpost = "http://api.vtcebank.vn:8888/api/card.asmx";
            string xmlusercard = "<?xml version=\"1.0\" encoding=\"utf-16\"?>"+
                                 "<CardRequest>"+
                                "<Function>"+fun+"</Function>"+
                                "<CardID>" + cardid + "</CardID>" +
                                "<CardCode>" + cardcode + "</CardCode>" +
                                "<Description>" + description + "</Description>" +
                                "</CardRequest>";
            string requestData = Encrypt(key, xmlusercard);
            var response = SendPostUtf8(xmlcardinvock(partnerID, requestData), urlpost);
            var xmlresultnotde = getValue(response, "RequestResult");
            var xmlresultde = Decrypt(key, xmlresultnotde);
            ltrresutlvcoin.Text = "<br/>" + "ResponseStatus:" + getValue(xmlresultde, "ResponseStatus") + "<br/>Descripton:" + getValue(xmlresultde, "Descripton");
        }
         */
    }
}
