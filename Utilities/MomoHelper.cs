using Newtonsoft.Json.Linq;
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
    public class MomoHelper
    {
        private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public MomoHelper()
        {
            //encrypt and decrypt password using secure
        }
        public static string getHash(string partnerCode, string merchantRefId,
            string amount, string paymentCode, string storeId, string storeName, string publicKey)
        {
            string json = "{\"partnerCode\":\"" +
                partnerCode + "\",\"partnerRefId\":\"" +
                merchantRefId + "\",\"amount\":" +
                amount + ",\"paymentCode\":\"" +
                paymentCode + "\",\"storeId\":\"" +
                storeId + "\",\"storeName\":\"" +
                storeName + "\"}";
            log.Debug("Raw hash: " + json);
            byte[] data = Encoding.UTF8.GetBytes(json);
            string result = null;
            using (var rsa = new RSACryptoServiceProvider(4096)) // or 4096, base on key length
            {
                try
                {
                    // Client encrypting data with public key issued by server
                    // "publicKey" must be XML format, use https://superdry.apphb.com/tools/online-rsa-key-converter
                    // to convert from PEM to XML before hash
                    rsa.FromXmlString(publicKey);
                    var encryptedData = rsa.Encrypt(data, false);
                    var base64Encrypted = Convert.ToBase64String(encryptedData);
                    result = base64Encrypted;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }

            }

            return result;

        }
        public static string buildQueryHash(string partnerCode, string merchantRefId,
            string requestid, string publicKey)
        {
            string json = "{\"partnerCode\":\"" +
                partnerCode + "\",\"partnerRefId\":\"" +
                merchantRefId + "\",\"requestId\":\"" +
                requestid + "\"}";
            log.Debug("Raw hash: " + json);
            byte[] data = Encoding.UTF8.GetBytes(json);
            string result = null;
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    // client encrypting data with public key issued by server
                    rsa.FromXmlString(publicKey);
                    var encryptedData = rsa.Encrypt(data, false);
                    var base64Encrypted = Convert.ToBase64String(encryptedData);
                    result = base64Encrypted;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }

            }

            return result;

        }

        public static string buildRefundHash(string partnerCode, string merchantRefId,
            string momoTranId, long amount, string description, string publicKey)
        {
            string json = "{\"partnerCode\":\"" +
                partnerCode + "\",\"partnerRefId\":\"" +
                merchantRefId + "\",\"momoTransId\":\"" +
                momoTranId + "\",\"amount\":" +
                amount + ",\"description\":\"" +
                description + "\"}";
            log.Debug("Raw hash: " + json);
            byte[] data = Encoding.UTF8.GetBytes(json);
            string result = null;
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                try
                {
                    // client encrypting data with public key issued by server
                    rsa.FromXmlString(publicKey);
                    var encryptedData = rsa.Encrypt(data, false);
                    var base64Encrypted = Convert.ToBase64String(encryptedData);
                    result = base64Encrypted;
                }
                finally
                {
                    rsa.PersistKeyInCsp = false;
                }

            }

            return result;

        }
        public static string signSHA256(string message, string key)
        {
            byte[] keyByte = Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                string hex = BitConverter.ToString(hashmessage);
                hex = hex.Replace("-", "").ToLower();
                return hex;

            }
        }

        public static string sendPaymentRequest(string endpoint, string postJsonString)
        {

            try
            {
                HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(endpoint);

                var postData = postJsonString;

                var data = Encoding.UTF8.GetBytes(postData);

                httpWReq.ProtocolVersion = HttpVersion.Version11;
                httpWReq.Method = "POST";
                httpWReq.ContentType = "application/json";

                httpWReq.ContentLength = data.Length;
                httpWReq.ReadWriteTimeout = 30000;
                httpWReq.Timeout = 15000;
                Stream stream = httpWReq.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();

                HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse();

                string jsonresponse = "";

                using (var reader = new StreamReader(response.GetResponseStream()))
                {

                    string temp = null;
                    while ((temp = reader.ReadLine()) != null)
                    {
                        jsonresponse += temp;
                    }
                }


                //todo parse it
                return jsonresponse;
                //return new MomoResponse(mtid, jsonresponse);

            }
            catch (WebException e)
            {
                return e.Message;
            }
        }


        public static string CreateOrder(string trans_amount, string order_id, string desc, ref string log)
        {
            string momoUrl = "";
            //request params need to request to MoMo system
            string endpoint = ConfigHelper.GetConfig("momo_url");// textEndpoint.Text.Equals("") ? "https://test-payment.momo.vn/gw_payment/transactionProcessor" : textEndpoint.Text;
            string partnerCode = ConfigHelper.GetConfig("momo_partnerCode"); //textPartnerCode.Text;
            string accessKey = ConfigHelper.GetConfig("momo_accessKey"); //textAccessKey.Text;
            string serectkey = ConfigHelper.GetConfig("momo_serectkey"); //"gfnqSYlAW2qIww91nryPLkvfaQyKJ6nk";
            string orderInfo = desc;// textOrderInfo.Text;
            string returnUrl = ConfigHelper.GetConfig("momo_returnUrl"); //textReturn.Text;
            string notifyurl = ConfigHelper.GetConfig("momo_notifyurl"); //textNotify.Text;

            string amount = trans_amount;
            string orderid = order_id;
            string requestId = Guid.NewGuid().ToString();
            string extraData = "merchantName=;merchantId=";//pass empty value if your merchant does not have stores else merchantName=[storeName]; merchantId=[storeId] to identify a transaction map with a physical store

            //before sign HMAC SHA256 signature
            string rawHash = "partnerCode=" +
                partnerCode + "&accessKey=" +
                accessKey + "&requestId=" +
                requestId + "&amount=" +
                amount + "&orderId=" +
                orderid + "&orderInfo=" +
                orderInfo + "&returnUrl=" +
                returnUrl + "&notifyUrl=" +
                notifyurl + "&extraData=" +
                extraData;
            log = CLogger.Append(log, "rawHash = " + rawHash);

            //MoMoSecurity crypto = new MoMoSecurity();
            //sign signature SHA256
            string signature = signSHA256(rawHash, serectkey);
            
            log = CLogger.Append(log, "signature = " + signature);
            //build body json request
            JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "accessKey", accessKey },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderid },
                { "orderInfo", orderInfo },
                { "returnUrl", returnUrl },
                { "notifyUrl", notifyurl },
                { "requestType", "captureMoMoWallet" },
                { "extraData", extraData },
                { "signature", signature }

            };
            log = CLogger.Append(log, "rs = " + message.ToString());
            string responseFromMomo = sendPaymentRequest(endpoint, message.ToString());
            //textPubkey.Text = responseFromMomo;
            JObject jmessage = JObject.Parse(responseFromMomo);
            log = CLogger.Append("Return from MoMo: " + jmessage.ToString());
            //DialogResult result = MessageBox.Show(responseFromMomo, "Open in browser", MessageBoxButtons.OKCancel);
            //if (result == DialogResult.OK)
            //{
            //    //yes...
            //    System.Diagnostics.Process.Start(jmessage.GetValue("payUrl").ToString());
            //}
            //else if (result == DialogResult.Cancel)
            //{
            //    //no...
            //}
            momoUrl = jmessage.GetValue("payUrl").ToString();
            return momoUrl;
        }
        public static Dictionary<string, object> QueryStatus(string orderId, ref string sLog)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            try
            {
                sLog = CLogger.Append(sLog, "QueryStatus");
                string endpoint = ConfigHelper.GetConfig("momo_url_query");
                string partnerCode = ConfigHelper.GetConfig("momo_partnerCode");
                string merchantRefId = orderId;
                string accessKey = ConfigHelper.GetConfig("momo_accessKey"); //textAccessKey.Text;
                string serectkey = ConfigHelper.GetConfig("momo_serectkey"); //"gfnqSYlAW2qIww91nryPLkvfaQyKJ6nk";
                string requestId = Guid.NewGuid().ToString();
                string requestType = "transactionStatus";

                string rawHash = string.Format("partnerCode={0}&accessKey={1}&requestId={2}&orderId={3}&requestType={4}",
                    partnerCode, accessKey, requestId, orderId, requestType);
                sLog = CLogger.Append(sLog, rawHash);
                string signature = signSHA256(rawHash, serectkey);

                JObject message = new JObject
                {
                    { "partnerCode", partnerCode },
                    { "accessKey", accessKey },
                    { "requestId", requestId },
                    { "orderId", orderId },
                    { "requestType", requestType },
                    { "signature", signature }

                };

                sLog = CLogger.Append(sLog, message.ToString());

                //response from MoMo
                sLog = CLogger.Append(sLog, "sendPaymentRequest");
                string responseFromMomo = sendPaymentRequest(endpoint, message.ToString());
                sLog = CLogger.Append(sLog, responseFromMomo);
                //JObject jmessage = JObject.Parse(responseFromMomo);
                //log.Debug("Return from MoMo: " + jmessage.ToString());
                sLog = CLogger.Append(sLog, "JsonString2Dic");
                dic = JsonHelper.JsonString2Dic(responseFromMomo);
            }
            catch (Exception ex)
            {
                sLog = CLogger.Append(sLog, ex.Message);
            }
            return dic;
        }

        public static string IsValidLink(string query)
        {
            return "";
        }

        public class MomoPayOrderResponse : BankBaseResponse
        {
            public override void SetResponse(string cardType, string response)
            {

            }
            public override string GetTransactionStatus(string code = "")
            {
                if (!string.IsNullOrEmpty(code))
                {
                    this.ResponseCode = code;
                }
                //if (this.ResponseCode == "0" || this.ResponseCode == "3")
                //{
                //    return PENDING;
                //}
                if (this.ResponseCode == "0")
                {
                    return SUCC;
                }
                return FAIL;
            }
            public MomoPayOrderResponse()
            {
                RStatus = new Dictionary<string, string>()
                {
                    {"0","Thành công"},
                    {"3","Thông tin tài khoản không tồn tại"},
                    {"7","Không có quyền truy cập"},
                    {"17","Số tiền không đủ để giao dịch"},
                    {"45","Lỗi hệ thống. Request timeout"},
                    {"46","Số tiền vượt quá phạm vi"},
                    {"68","Không được hỗ trợ (refund)"},
                    {"103","Hủy giao dịch treo tiền"},
                    {"151","Số tiền thanh toán phải nằm trong khoảng 1,000 đến 20,000,000"},
                    {"153","Giải mã hash thất bại"},
                    {"161","Vướng hạn mức thanh toán. Yêu cầu cài đặt lại hạn mức trên app MoMo"},
                    {"162","Mã thanh toán (Payment Code) đã được sử dụng"},
                    {"204","Số tiền sai định dạng"},
                    {"208","Thông tin đối tác không tồn tại hoặc chưa kích hoạt"},
                    {"210","Hệ thống đang bảo trì"},
                    {"400","Giao dịch không tồn tại (khi gọi hoàn tất giao dịch)"},
                    {"403","Không có quyền truy cập (khi kết nối với MoMo)"},
                    {"404","Request không được hỗ trợ"},
                    {"1001","Tài khoản không đủ tiền đề giao dịch"},
                    {"1002","Giao dịch đã được khôi phục"},
                    {"1004","Tài khoản đã hết hạn mức giao dịch trong ngày"},
                    {"1006","Hệ thống xảy ra lỗi"},
                    {"1012","Tài khoản đã bị khóa"},
                    {"1013","Xác thực hết hạn"},
                    {"1014","Mật khẩu hoặc mã thanh toán không chính xác"},
                    {"2119","Không lấy được thông tin khuyến mãi"},
                    {"2125","Không thể hoàn tiền giao dịch"},
                    {"2126","Dữ liệu đối tác chưa được cấu hình"}
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
            public string PartnerCode { get; set; }
            public string RequestId { get; set; }
            public string TransId { get; set; }
            public void SetResponse(HttpRequestBase request)
            {
                /*partnerCode=$partnerCode&
                 * accessKey=$accessKey
                 * &requestId=$requestId
                &amount=$amount&
                orderId=$orderId&
                orderInfo=$orderInfo
                &orderType=$orderType&
                transId=$transId
                &message=$message&
                localMessage=$localMessage
                &responseTime=$responseTime&
                errorCode=$errorCode
                &payType=$payType&
                extraData=$extraData*/
                try
                {
                    this.PartnerCode = request["partnerCode"];
                    this.AccessKey = request["accessKey"];
                    this.RequestId = request["requestId"];
                    this.Amt = NumberHelper.ConvertToInt(request["amount"]);
                    this.TransactionId = request["orderId"];
                    this.OrderInfo = request["orderInfo"];
                    this.OrderType = request["orderType"];
                    this.TransId = request["transId"];
                    this.RequestTime = request["localMessage"];
                    this.ResponseCode = request["errorCode"];
                    this.ResponseMessage = request["message"];
                    this.ResponseTime = request["responseTime"];
                    this.ProviderTransactionId = request["transId"];
                    this.TransactionStatus = request["errorCode"];
                    this.Status = request["errorCode"];
                    this.PayURL = request["extraData"];
                    this.Command = request["payType"];
                    this.Version = request["version"];
                }
                catch { }
            }
            public void SetResponseNotify(HttpRequestBase request)
            {
                try
                {
                    //Dictionary<string, object> jObj = JsonHelper.JsonString2Dic(request);
                    this.PartnerCode = request["partnerCode"];
                    this.AccessKey = request["accessKey"];
                    this.RequestId = request["requestId"];
                    this.Amt = NumberHelper.ConvertToInt(request["amount"]);
                    this.TransactionId = request["orderId"];
                    this.OrderInfo = request["orderInfo"];
                    this.OrderType = request["orderType"];
                    this.TransId = request["transId"];
                    this.RequestTime = request["localMessage"];
                    this.ResponseCode = request["errorCode"];
                    this.ResponseMessage = request["message"];
                    this.ResponseTime = request["responseTime"];
                    this.ProviderTransactionId = request["transId"];
                    this.TransactionStatus = request["errorCode"];
                    this.Status = request["errorCode"];
                    this.PayURL = request["extraData"];
                    this.Command = request["payType"];
                    this.Version = request["version"];
                }
                catch { }
            }

            public bool IsValidIBN(string sign)
            {
                string secretkey = ConfigHelper.GetConfig("momo_serectkey");
                string raw = string.Format("partnerCode={0}&accessKey{1}&requestId={2}&amount={3}&orderId={4}&orderInfo={5}&orderType={6}&transId={7}&message={8}&localMessage={9}&responseTime={10}&errorCode={11}&payType={12}&extraData={13}",
                    PartnerCode, AccessKey, RequestId, Amt, TransactionId, OrderInfo, OrderType, TransId, ResponseMessage, RequestTime, ResponseTime, ResponseCode, Command, PayURL);

                var signature = signSHA256(raw, secretkey);
                return sign == signature;
            }
            public string GetIBNResponse()
            {
                /*partnerCode=$partnerCode&accessKey=$accessKey&
                 * requestId=$requestId&orderId=$orderId
                 * &errorCode=$errorCode&message=$message&responseTime=responseTime
                 * &extraData=extraData*/
                string secretkey = ConfigHelper.GetConfig("momo_serectkey");
                Dictionary<string, object> dic = new Dictionary<string, object>
                {
                     { "partnerCode", this.PartnerCode},
                     { "accessKey", this.PartnerCode},
                     { "requestId", this.PartnerCode},
                     { "orderId", this.PartnerCode},
                     { "errorCode", this.PartnerCode},
                     { "message", this.PartnerCode},
                     { "responseTime", this.PartnerCode},
                     { "extraData", this.PartnerCode}
                 };
                string sign = signSHA256(StringHelper.DicToQueryString(dic), secretkey);
                dic.Add("signature", sign);

                return JsonHelper.Dic2JsonString(dic);
            }
        }
    }
}
