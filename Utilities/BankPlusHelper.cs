using _1Pay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Utilities
{
    public class BankPlusHelper
    {
        public class BankPlusOrderResponse : BankBaseResponse
        {
            public override void SetResponse(string cardType, string response)
            {

            }
            public override string GetTransactionStatus(string code = "")
            {
                if(!string.IsNullOrEmpty(code))
                {
                    this.ResponseCode = code;
                }
                if (this.ResponseCode == "0" || this.ResponseCode == "3")
                {
                    return PENDING;
                }
                if (this.ResponseCode == "1")
                {
                    return SUCC;
                }
                return FAIL;
            }
            public BankPlusOrderResponse()
            {
                RStatus = new Dictionary<string, string>()
                {
                    {"0","Giao dịch đang xử lý"},
                    {"1","Giao dịch thành công."},
                    {"2","Giao dịch thất bại"},
                    {"3","Giao dịch chưa rõ kết quả"},
                    {"9","Thông tin gửi lên không chính xác"},
                    {"99","Sai chữ ký"}
                };
            }
            
            public void SetResponse(string response)
            {
                try
                {
                    Dictionary<string, object> jObj = JsonHelper.JsonString2Dic(response);
                    //this.AccessKey = JsonHelper.GetValue(jObj, "access_key");
                    //this.Amt = NumberHelper.ConvertToInt(JsonHelper.GetValue(jObj, "amount"));
                    this.TransactionId = JsonHelper.GetValue(jObj, "order_id");
                    //this.OrderInfo = JsonHelper.GetValue(jObj, "order_info");
                    //this.OrderType = JsonHelper.GetValue(jObj, "order_type");
                    //this.RequestTime = JsonHelper.GetValue(jObj, "request_time");
                    this.ResponseCode = JsonHelper.GetValue(jObj, "response_code");
                    //this.ResponseMessage = JsonHelper.GetValue(jObj, "response_message");
                    //this.ResponseTime = JsonHelper.GetValue(jObj, "response_time");
                    //this.ProviderTransactionId = JsonHelper.GetValue(jObj, "trans_ref");
                    //this.TransactionStatus = JsonHelper.GetValue(jObj, "trans_status");
                    this.Status = JsonHelper.GetValue(jObj, "status");
                    //this.PayURL = JsonHelper.GetValue(jObj, "pay_url");
                    //this.Command = JsonHelper.GetValue(jObj, "command");
                    this.Version = JsonHelper.GetValue(jObj, "version");
                }
                catch { }
            }
            public void SetResponse(HttpRequestBase request)
            {
                try
                {
                    //this.AccessKey = request["access_key"];
                    //this.Amt = NumberHelper.ConvertToInt(request["amount"]);
                    this.TransactionId = request["order_id"];
                    //this.OrderInfo = request["order_info"];
                    //this.OrderType = request["order_type"];
                    //this.RequestTime = request["request_time"];
                    this.ResponseCode = request["response_code"];
                    //this.ResponseMessage = request["response_message"];
                    //this.ResponseTime = request["response_time"];
                    //this.ProviderTransactionId = request["trans_ref"];
                    //this.TransactionStatus = request["trans_status"];
                    this.Status = request["status"];
                    //this.PayURL = request["pay_url"];
                    //this.Command = request["command"];
                    this.Version = request["version"];
                }
                catch { }
            }
            public void SetResponseNotify(string response)
            {
                try
                {
                    Dictionary<string, object> jObj = JsonHelper.JsonString2Dic(response);
                    //this.AccessKey = JsonHelper.GetValue(jObj, "access_key");
                    this.Amt = NumberHelper.ConvertToInt(JsonHelper.GetValue(jObj, "trans_amount"));
                    this.TransactionId = JsonHelper.GetValue(jObj, "merchant_trans_id");
                    //this.OrderInfo = JsonHelper.GetValue(jObj, "order_info");
                    //this.OrderType = JsonHelper.GetValue(jObj, "order_type");
                    //this.RequestTime = JsonHelper.GetValue(jObj, "request_time");
                    this.ResponseCode = JsonHelper.GetValue(jObj, "status");
                    //this.ResponseMessage = JsonHelper.GetValue(jObj, "response_message");
                    //this.ResponseTime = JsonHelper.GetValue(jObj, "response_time");
                    //this.ProviderTransactionId = JsonHelper.GetValue(jObj, "trans_ref");
                    //this.TransactionStatus = JsonHelper.GetValue(jObj, "trans_status");
                    this.Status = JsonHelper.GetValue(jObj, "status");
                    //this.PayURL = JsonHelper.GetValue(jObj, "pay_url");
                    //this.Command = JsonHelper.GetValue(jObj, "command");
                    //this.Version = JsonHelper.GetValue(jObj, "version");
                }
                catch { }
            }
        }
        public static string CreateOrder(string trans_amount, string merchant_trans_id, string desc)
        {
            String payUrl = ConfigHelper.GetConfig("BankPlusPayUrl");
            String return_url = ConfigHelper.GetConfig("BankPlusReturnUrl");
            String queryUrl = ConfigHelper.GetConfig("BankPlusQueryUrl");
            String secureKey = ConfigHelper.GetConfig("BankPlusSecureKey");
            String accessCode = ConfigHelper.GetConfig("BankPlusAccessCode");
            String verion = ConfigHelper.GetConfig("BankPlusVerssion");
            String keyOf789 = ConfigHelper.GetConfig("BankPlus789Key");
            String merchant_code = ConfigHelper.GetConfig("BankPlusMerchantCode");
            string command = "PAYMENT";
            string raw = string.Format("{0}{1}{2}{3}{4}{5}{6}", accessCode, command, merchant_code, merchant_trans_id, return_url, trans_amount, verion);

            String signature = CalculateRFC2104HMAC(raw, secureKey); //create signature
            string getParam = string.Format("version={0}&command={1}&merchant_code={2}&merchant_trans_id={3}&trans_amount={4}&secure_hash={5}&return_url={6}&desc={7}",
                verion, command, merchant_code, merchant_trans_id, trans_amount, signature, return_url, desc);
            return payUrl + getParam;
        }
        public static string QueryOrder(string orderId)
        {
            String result = "";
            String payUrl = ConfigHelper.GetConfig("BankPlusPayUrl");
            String queryUrl = ConfigHelper.GetConfig("BankPlusQueryUrl");
            String secureKey = ConfigHelper.GetConfig("BankPlusSecureKey");
            String accessCode = ConfigHelper.GetConfig("BankPlusAccessCode");
            String verion = ConfigHelper.GetConfig("BankPlusVerssion");
            String keyOf789 = ConfigHelper.GetConfig("BankPlus789Key");
            String merchantCode = ConfigHelper.GetConfig("BankPlusMerchantCode");
            string command = "TRANS_INQUIRY";
            string raw = string.Format("{0}{1}{2}{3}{4}", accessCode, command, merchantCode, orderId, verion);

            String signature = CalculateRFC2104HMAC(raw, secureKey); //create signature
            String urlParameter = String.Format("version={0}&cmd={1}&merchant_code{2}&order_id={3}&secure_hash={4}",
                verion, command, merchantCode, orderId, signature);
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(queryUrl);
                request.KeepAlive = false;
                request.ProtocolVersion = HttpVersion.Version10;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0";
                WebHeaderCollection headerReader = request.Headers;
                headerReader.Add("Accept-Language", "en-US,en;q=0.5");
                var data = Encoding.ASCII.GetBytes(urlParameter);
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
        public static string VerifyTransaction(string merchant_trans_id, string merchant_code, string trans_amount, string check_sum, out string code)
        {
            String resultCode = "00";
            String accessCode = ConfigHelper.GetConfig("BankPlusAccessCode");
            String secureKey = ConfigHelper.GetConfig("BankPlusSecureKey");
            String keyOf789 = ConfigHelper.GetConfig("BankPlus789Key");
            String merchantCode = ConfigHelper.GetConfig("BankPlusMerchantCode");

            if(string.IsNullOrEmpty(merchant_trans_id) || string.IsNullOrEmpty(merchant_code) || string.IsNullOrEmpty(trans_amount) || string.IsNullOrEmpty(check_sum))
            {
                resultCode = "01";
            }

            string raw = string.Format("{0}{1}{2}{3}", accessCode, merchant_trans_id, merchantCode, trans_amount);

            String myChecksum = CalculateRFC2104HMAC(raw, secureKey);
            if (myChecksum != check_sum)
            {
                resultCode = "01";
            }
            code = resultCode;
            return CreateResponse(merchant_trans_id, trans_amount, resultCode);
        }
        public static string ReceivedNotify(string merchant_trans_id, string merchant_code, string trans_amount, string status, string check_sum, out string code)
        {
            //strData = AccessCode + merchant_trans_id + merchant_code + trans_amount + status
            String resultCode = "00";
            String accessCode = ConfigHelper.GetConfig("BankPlusAccessCode");
            String verion = ConfigHelper.GetConfig("BankPlusVerssion");
            String keyOf789 = ConfigHelper.GetConfig("BankPlus789Key");
            String merchantCode = ConfigHelper.GetConfig("BankPlusMerchantCode");
            //strData = “ACVNTxxxM12330000001”

            if (string.IsNullOrEmpty(merchant_trans_id) || string.IsNullOrEmpty(merchant_code) || string.IsNullOrEmpty(trans_amount) || string.IsNullOrEmpty(status) || string.IsNullOrEmpty(check_sum))
            {
                resultCode = "01";
            }

            string raw = string.Format("{0}{1}{2}{3}{4}", accessCode, merchant_trans_id, merchantCode, trans_amount, status);

            String myChecksum = CalculateRFC2104HMAC(raw, keyOf789);
            if (myChecksum != check_sum)
            {
                resultCode = "01";
            }

            code = resultCode;
            //do update data
            return CreateResponse(merchant_trans_id, trans_amount, resultCode);


        }
        public static string CreateResponse(string merchant_trans_id, string trans_amount, string resultCode)
        {
            String accessCode = ConfigHelper.GetConfig("BankPlusAccessCode");
            String keyOf789 = ConfigHelper.GetConfig("BankPlus789Key");
            String merchantCode = ConfigHelper.GetConfig("BankPlusMerchantCode");

            string strData = accessCode + merchant_trans_id + merchantCode + trans_amount + resultCode;

            string rsChecksum = CalculateRFC2104HMAC(strData, keyOf789);
            return "{" + string.Format("\"response_code\":\"{0}\",\"merchant_trans_id\":\"{1}\",\"trans_amount\":\"{2}\" ,\"merchant_code\":\"{3}\",\"check_sum\":\"{4}\"",
                resultCode, merchant_trans_id, trans_amount, merchantCode, rsChecksum) + "}";
        }

        public static string CalculateRFC2104HMAC(String data, String key)
        {
            String result = null;

            KeyedHashAlgorithm algorithm = new HMACSHA1();

            Encoding encoding = new UTF8Encoding();

            algorithm.Key = encoding.GetBytes(key);

            result = Convert.ToBase64String(algorithm.ComputeHash(encoding.GetBytes(data.ToCharArray())));

            return result;
        }
    }
}
