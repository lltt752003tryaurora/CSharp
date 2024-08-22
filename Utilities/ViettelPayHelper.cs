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
    public class ViettelPayHelper
    {
        public class ViettelPayOrderResponse : BankBaseResponse
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
            public ViettelPayOrderResponse()
            {
                RStatus = new Dictionary<string, string>()
                {
                    {"00","Giao dịch thành công."},
                    {"V01","Thông tin đối tác truyền lên không chính xác hoặc thiếu thông tin"},
                    {"V02","Mã xác nhân  người dung nhập không đúng"},
                    {"V04","Có lỗi khi truy vấn hệ thống"},
                    {"V05","Sai check_sum khi gọi sang API của đối tác"},
                    {"V09","Sai secure_hash"},
                    {"11","Thuê bao %PHONE_NUMBER% đã được sử dụng để đăng ký dịch vụ, Quý khách vui lòng kiểm tra lại thông tin."},
                    {"12","Số tài khoản không hợp lệ, Quý khách vui lòng kiểm tra và thử thực hiện lại giao dịch."},
                    {"16","Số dư tài khoản của Quý khách không đủ để thực hiện giao dịch, vui lòng kiểm tra và thực hiện lại."},
                    {"99","Lỗi không xác định"},
                    {"117","Tài khoản của quý khách hiện đang bị khóa, vui lòng thử thực hiện lại giao dịch, hoặc liên hệ %BANK_NAME% "},
                    {"120","Lỗi hạch toán tại ngân hàng %BANK_NAME%, vui lòng thử thực hiện lại giao dịch, hoặc liên hệ tổng đài %BANK_HOTLINE% "},
                    {"121","Thông tin tài khoản nhận tiền không hợp lệ, Quý khách vui lòng kiểm tra và thực hiện lại."},
                    {"123","Tài khoản người nhận không tồn tại, Quý khách vui lòng kiểm tra và thử thực hiện lại giao dịch."},
                    {"124","Số dư tài khoản không đủ để thanh toán phí giao dịch, Quý khách vui lòng kiểm tra và thực hiện lại."},
                    {"125","Tài khoản của quý khách hiện chưa được kích hoạt, vui lòng thực hiện lại giao dịch sau, hoặc liên hệ ngân hàng "},
                    {"127","Xác nhận gửi về thuê bao %PHONE_NUMBER% không có phản hồi, vui lòng thực hiện lại giao dịch."},
                    {"130","Tài khoản của quý khách hiện đang không hoạt động, vui lòng thử thực hiện lại giao dịch, hoặc liên hệ %BANK_NAME% "},
                    {"132","Có lỗi xảy ra tại ngân hàng %BANK_CODE%, vui lòng thử thực hiện lại giao dịch hoặc liên hệ tổng đài %BANK_HOTLINE% "},
                    {"139","Thẻ chưa được kích hoạt, Quý khách vui lòng kích hoạt thẻ để sử dụng dịch vụ."},
                    {"140","Có lỗi tại ngân hàng %BANK_NAME%, vui lòng thử thực hiện lại giao dịch hoặc liên hệ dịch vụ chăm sóc khách hàng "},
                    {"143","Tài khoản của quý khách không còn tồn tại, vui lòng liên hệ  tổng đài ngân hàng %BANK_CODE% (%BANK_HOTLINE%) để "},
                    {"145","Có một hạn chế trong tài khoản của quý khách tại ngân hàng %BANK_CODE%, vui lòng liên hệ dịch vụ chăm sóc khách hàng "},
                    {"147","Ngân hàng không xác thực được mã PIN, vui lòng thử thực hiện lại giao dịch hoặc liên hệ tổng đài %BANK_CODE% "},
                    {"148","Tài khoản của quý khách hiện đang bị khóa, vui lòng thử thực hiện lại giao dịch hoặc liên hệ %BANK_NAME% "},
                    {"150","Số tài khoản không đúng định dạng."},
                    {"158","Mã hóa đơn không tồn tại, Quý khách vui lòng kiểm tra và thực hiện lại."},
                    {"197","Sai mật khẩu."}
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
        public static string CreateOrder(string trans_amount, string order_id, string desc)
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
            string raw = string.Format("{0}{1}{2}{3}{4}{5}", accessCode, command, merchant_code, order_id, trans_amount, verion);

            String signature = CalculateRFC2104HMAC(raw, secureKey); //create signature
            string getParam = string.Format("version={0}&command={1}&merchant_code={2}&order_id={3}&trans_amount={4}&secure_hash={5}&return_url={6}&desc={7}",
                verion, command, merchant_code, order_id, trans_amount, signature, return_url, desc);
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
        //
        public static bool VerifyURLReturn(string billcode, string cust_msisdn, string error_code,
        string merchant_code, string order_id, string payment_status, string trans_amount,
            string vt_transaction_id, string secure_hash, out string code)
        {
            bool resultCode = true;
            code = "00";
            String accessCode = ConfigHelper.GetConfig("BankPlusAccessCode");
            String secureKey = ConfigHelper.GetConfig("BankPlusSecureKey");
            String keyOf789 = ConfigHelper.GetConfig("BankPlus789Key");
            String merchantCode = ConfigHelper.GetConfig("BankPlusMerchantCode");

            if (string.IsNullOrEmpty(cust_msisdn) || string.IsNullOrEmpty(error_code) || 
                string.IsNullOrEmpty(merchant_code) || string.IsNullOrEmpty(order_id) ||
                string.IsNullOrEmpty(payment_status) || string.IsNullOrEmpty(trans_amount) ||
                string.IsNullOrEmpty(vt_transaction_id) || string.IsNullOrEmpty(secure_hash))
            {
                resultCode = false;
                code = "01";
            }

            string raw = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", accessCode, billcode, cust_msisdn,  error_code,
                merchant_code,  order_id,  payment_status,  trans_amount, vt_transaction_id);

            String myChecksum = CalculateRFC2104HMAC(raw, secureKey);
            if (myChecksum != secure_hash)
            {
                resultCode = false;
                code = "02";
            }
            return resultCode;
        }
        public static string VerifyTransaction(string billcode, string check_sum, string merchant_code, string order_id, string trans_amount, out string code)
        {
            String resultCode = "00";
            String accessCode = ConfigHelper.GetConfig("BankPlusAccessCode");
            String secureKey = ConfigHelper.GetConfig("BankPlusSecureKey");
            String keyOf789 = ConfigHelper.GetConfig("BankPlus789Key");
            String merchantCode = ConfigHelper.GetConfig("BankPlusMerchantCode");

            if(string.IsNullOrEmpty(billcode) || string.IsNullOrEmpty(check_sum) || string.IsNullOrEmpty(merchant_code) || string.IsNullOrEmpty(order_id) || string.IsNullOrEmpty(trans_amount))
            {
                resultCode = "01";
            }
            /*
             00: thành công (dữ liệu chính xác, số tiền khớp với mã giao dịch)
            01: không thành công (dữ liệu không chính xác)
            02: check_sum gửi sang không đúng
            03: có lỗi tại hệ thống đối tác (các loại exception)
             */

            string raw = string.Format("{0}{1}{2}{3}{4}", accessCode, billcode, merchant_code, order_id, trans_amount);

            String myChecksum = CalculateRFC2104HMAC(raw, secureKey);
            if (myChecksum != check_sum)
            {
                resultCode = "02";
            }
            code = resultCode;
            return CreateResponse(billcode, resultCode, merchant_code, order_id, trans_amount);
        }
        /*billcode Mã hóa đơn của KH, không bắt buộc nhưng nếu
            đã quy định với Viettel thì sẽ truyền BILL123TEST Có
            cust_msisdn Số điện thoại KH thanh toán 01680000000 Có
            error_code Mã lỗi trả về từ Viettel 00 (Xem bảng dưới) Có
            merchant_code Mã đối tác SHOPEE Có
            order_id Mã giao dịch lưu bên đối tác SHOPEE01234 Có
            payment_status Trạng thái thanh toán của KH 1 (Xem bảng dưới) Có
            check_sum Chuỗi mã hóa tạo ra dựa trên dữ liệu truyền sang. Không
            trans_amount Số tiền thanh toán 3750000 Có
            vt_transaction_id Mã giao dịch bên viettel 201805310849ABC Có*/
        public static string ReceivedNotify(
            string billcode, 
            string cust_msisdn,
            string error_code, 
            string merchant_code, 
            string order_id,
            string payment_status,
            string check_sum,
            string trans_amount,
            string vt_transaction_id,
            out string code)
        {
            //strData = AccessCode + merchant_trans_id + merchant_code + trans_amount + status
            String resultCode = "00";
            String accessCode = ConfigHelper.GetConfig("BankPlusAccessCode");
            String verion = ConfigHelper.GetConfig("BankPlusVerssion");
            String keyOf789 = ConfigHelper.GetConfig("BankPlus789Key");
            String merchantCode = ConfigHelper.GetConfig("BankPlusMerchantCode");
            //strData = “ACVNTxxxM12330000001”

            if (string.IsNullOrEmpty(billcode) || string.IsNullOrEmpty(cust_msisdn) || string.IsNullOrEmpty(error_code) || 
                string.IsNullOrEmpty(merchant_code) || string.IsNullOrEmpty(order_id) ||
                string.IsNullOrEmpty(payment_status) || string.IsNullOrEmpty(check_sum) ||
                string.IsNullOrEmpty(trans_amount) || string.IsNullOrEmpty(vt_transaction_id))
            {
                resultCode = "01";
            }

            string raw = string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}", accessCode, billcode, cust_msisdn, error_code, merchant_code, order_id, payment_status, trans_amount, vt_transaction_id);

            String myChecksum = CalculateRFC2104HMAC(raw, keyOf789);
            if (myChecksum != check_sum)
            {
                resultCode = "02";
            }

            code = resultCode;
            //do update data
            return CreateNotifyResponse(resultCode, merchant_code, order_id);


        }
        /*{
 "billcode" : "BILL123TEST",
 "error_code" : "00",
 "merchant_code" : "SHOPEE",
 "order_id" : "SHOPEE01234",
 "trans_amount" : "3750000",
 "check_sum" : "AcirUAX2+pvEnIZ8Q85aAL6NGJs%3D"
}*/
        public static string CreateResponse(string billcode, string error_code, string merchant_code, string order_id, string trans_amount)
        {
            String accessCode = ConfigHelper.GetConfig("BankPlusAccessCode");
            String keyOf789 = ConfigHelper.GetConfig("BankPlus789Key");
            String merchantCode = ConfigHelper.GetConfig("BankPlusMerchantCode");

            string strData = accessCode + billcode + error_code + merchant_code + order_id + trans_amount;
            string rsChecksum = CalculateRFC2104HMAC(strData, keyOf789);
            return "{" + string.Format("\"billcode\":\"{0}\",\"error_code\":\"{1}\",\"merchant_code\":\"{2}\" ,\"order_id\":\"{3}\",\"check_sum\":\"{4}\"",
                billcode, error_code, merchant_code, order_id, rsChecksum) + "}";
        }
        public static string CreateNotifyResponse(string error_code, string merchant_code, string order_id)
        {
            String accessCode = ConfigHelper.GetConfig("BankPlusAccessCode");
            String keyOf789 = ConfigHelper.GetConfig("BankPlus789Key");
            String merchantCode = ConfigHelper.GetConfig("BankPlusMerchantCode");

            string strData = accessCode + error_code + merchant_code + order_id;
            string rsChecksum = CalculateRFC2104HMAC(strData, keyOf789);
            return "{" + string.Format("\"error_code\":\"{1}\",\"merchant_code\":\"{2}\" ,\"order_id\":\"{3}\",\"check_sum\":\"{4}\"",
                error_code, merchant_code, order_id, rsChecksum) + "}";
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
