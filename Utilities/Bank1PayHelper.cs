using _1Pay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Utilities
{
    public class Bank1PayHelper
    {
        public class CreateOrderResponse
        {
            public const string SUCC = "succ";
            public const string FAIL = "fail";
            public const string PENDING = "pending";
            public Dictionary<string, string> RStatus = new Dictionary<string, string>()
            {
                {"00","Giao dịch thành công."},
                {"01","Ngân hàng từ chối thanh toán: thẻ/tài khoản bị khóa."},
                {"02","Thông tin thẻ không hợp lệ."},
                {"03","Thẻ hết hạn."},
                {"04","Lỗi người mua hàng: Quá số lần cho phép. (Sai OTP, quá hạn mức trong ngày)."},
                {"05","Không có trả lời của Ngân hàng."},
                {"06","Lỗi giao tiếp với Ngân hàng."},
                {"07","Tài khoản không đủ tiền."},
                {"08","Lỗi dữ liệu."},
                {"09","Kiểu giao dịch không được hỗ trợ."},
                {"10","Giao dịch không thành công."},
                {"11","Giao dịch chưa xác thực OTP."},
                {"12","Giao dịch không thành công, số tiền giao dịch vượt hạn mức ngày."},
                {"13","Thẻ chưa đăng ký Internet Banking"},
                {"14","Khách hàng nhập sai OTP."},
                {"15","Khách hàng nhập sai thông tin xác thực."},
                {"16","Khách hàng nhập sai tên chủ thẻ."},
                {"17","Khách hàng nhập sai số thẻ."},
                {"18","Khách hàng nhập sai ngày phát hành thẻ."},
                {"19","Khách hàng nhập sai ngày hết hạn thẻ."},
                {"20","OTP hết thời gian hiệu lực."},
                {"21","Quá thời gian thực hiện request (7 phút) hoặc OTP timeout."},
                {"22","Khách hàng chưa xác thực thông tin thẻ."},
                {"23","Thẻ không đủ điều kiện thanh toán (Thẻ/Tài khoản không hợp lệ hoặc TK không đủ số dư)."},
                {"24","Giao dịch vượt quá hạn mức một lần thanh toán của ngân hàng."},
                {"25","Giao dịch vượt quá hạn mức của ngân hàng."},
                {"26","Giao dịch chờ xác nhận từ Ngân hàng."},
                {"27","Khách hàng nhập sai thông tin bảo mật thẻ."},
                {"28","Giao dịch không thành công do quá thời gian quy định."},
                {"29","Lỗi xử lý giao dịch tại hệ thống Ngân hàng."},
                {"99","Không xác định."}
            };
            public string GetTransactionStatus(string code)
            {
                if (this.ResponseCode == "05" || this.ResponseCode == "06" || this.ResponseCode == "26" || this.ResponseCode == "29" || this.ResponseCode == "99")
                {
                    return PENDING;
                }
                if (this.ResponseCode == "00")
                {
                    return SUCC;
                }
                return FAIL;
            }
            public string GetMessage()
            {
                try
                {
                    return this.RStatus[this.ResponseCode];
                }
                catch
                {
                    return "Lỗi không xác định";
                }
            }

            /// <summary>
            /// Đại diện cho sản phẩm của merchant khai báo trong hệ thống 1pay.vn
            /// </summary>
            public string AccessKey
            {
                get; set;
            }
            /// <summary>
            /// Mã giao dịch do 1PAY sinh ra
            /// </summary>
            public string ProviderTransactionId
            {
                get; set;
            }
            /// <summary>
            /// Mã hóa đơn là duy nhất, đại diện cho giao dịch <50 ký tự
            /// </summary>
            public string TransactionId
            {
                get; set;
            }
            /// <summary>
            /// Tên chi nhánh ngân hàng, do khách hàng lựa chọn
            /// </summary>
            public string CardName
            {
                get; set;
            }
            /// <summary>
            /// Loại thẻ ngân hàng sử dụng
            /// </summary>
            public string CardType
            {
                get; set;
            }
            /// <summary>
            /// Số tiền cần giao dịch (>10000vnd)
            /// </summary>
            public int Amt
            {
                get; set;
            }
            /// <summary>
            /// Mô tả hóa đơn
            /// </summary>
            public string OrderInfo
            {
                get; set;
            }
            /// <summary>
            /// Nhận giá trị : ND
            /// </summary>
            public string OrderType
            {
                get; set;
            }
            /// <summary>
            /// Thời gian bắt đầu giao dịch ở dạng iso, ví dụ: 2013-07-06T22:54:50Z
            /// </summary>
            public string RequestTime
            {
                get; set;
            }
            /// <summary>
            /// Kết quả giao dịch .Nhận giá trị : 0
            /// </summary>
            public string ResponseCode
            {
                get; set;
            }
            /// <summary>
            /// Mô tả hóa đơn
            /// </summary>
            public string ResponseMessage
            {
                get; set;
            }
            /// <summary>
            /// Thời gian hoàn thành giao dịch ở dạng iso, ví dụ: 2013-07-06T22:54:50Z
            /// </summary>
            public string ResponseTime
            {
                get; set;
            }
            /// <summary>
            /// Mô tả trạng thái giao dịch
            /// </summary>
            public string TransactionStatus
            {
                get; set;
            }
            /// <summary>
            /// Địa chỉ url để thực hiện submit request (redirect)
            /// </summary>
            public string PayURL
            {
                get; set;
            }
            /// <summary>
            /// 	Địa chỉ url sau khi thực hiện thanh toán sẽ được redirect về, merchant cần xây dựng để nhận kết quả từ 1Pay gửi sang. Request do hệ thống 1Pay System gửi sang sẽ ở dạng HTTP GET
            /// </summary>
            public string Status
            {
                get; set;
            }
            public string Command
            {
                get; set;
            }
            public void SetResponse(string response)
            {
                try
                {
                    Dictionary<string, object> jObj = JsonHelper.JsonString2Dic(response);
                    this.AccessKey = JsonHelper.GetValue(jObj, "access_key");
                    this.Amt = NumberHelper.ConvertToInt(JsonHelper.GetValue(jObj, "amount"));
                    this.CardName = JsonHelper.GetValue(jObj, "card_name");
                    this.CardType = JsonHelper.GetValue(jObj, "card_type");
                    this.TransactionId = JsonHelper.GetValue(jObj, "order_id");
                    this.OrderInfo = JsonHelper.GetValue(jObj, "order_info");
                    this.OrderType = JsonHelper.GetValue(jObj, "order_type");
                    this.RequestTime = JsonHelper.GetValue(jObj, "request_time");
                    this.ResponseCode = JsonHelper.GetValue(jObj, "response_code");
                    this.ResponseMessage = JsonHelper.GetValue(jObj, "response_message");
                    this.ResponseTime = JsonHelper.GetValue(jObj, "response_time");
                    this.ProviderTransactionId = JsonHelper.GetValue(jObj, "trans_ref");
                    this.TransactionStatus = JsonHelper.GetValue(jObj, "trans_status");
                    this.Status = JsonHelper.GetValue(jObj, "status");
                    this.PayURL = JsonHelper.GetValue(jObj, "pay_url");
                    this.Command = JsonHelper.GetValue(jObj, "command");
                }
                catch { }
            }
            public void SetResponse(HttpRequestBase request)
            {
                try
                {
                    this.AccessKey = request["access_key"];
                    this.Amt = NumberHelper.ConvertToInt(request["amount"]);
                    this.CardName = request["card_name"];
                    this.CardType = request["card_type"];
                    this.TransactionId = request["order_id"];
                    this.OrderInfo = request["order_info"];
                    this.OrderType = request["order_type"];
                    this.RequestTime = request["request_time"];
                    this.ResponseCode = request["response_code"];
                    this.ResponseMessage = request["response_message"];
                    this.ResponseTime = request["response_time"];
                    this.ProviderTransactionId = request["trans_ref"];
                    this.TransactionStatus = request["trans_status"];
                    this.Status = request["status"];
                    this.PayURL = request["pay_url"];
                    this.Command = request["command"];
                }
                catch { }
            }
        }
        public String CreateOrder(string amount, string order_id, string order_info, string return_url)
        {
            String result = "";
            String url = ConfigHelper.GetConfig("Bank1PayUrl");
            String accessKey = ConfigHelper.GetConfig("Bank1PayAccessKey");
            String secretKey = ConfigHelper.GetConfig("Bank1PaySecretKey");
            string command = "request_transaction";
            My1Pay my1Pay = new My1Pay();
            String signature = my1Pay.generateSignature_Bank_2TransactionRequest(accessKey, amount, command, order_id, order_info, return_url, secretKey); //create signature
            String urlParameter = String.Format("access_key={0}&amount={1}&command={2}&order_id={3}&order_info={4}&return_url={5}&signature={6}", accessKey, amount, command, order_id, order_info, return_url, signature);
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
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
        public String QueryOrder(string providerTransId)
        {
            String result = "";
            String url = ConfigHelper.GetConfig("Bank1PayUrl");
            String accessKey = ConfigHelper.GetConfig("Bank1PayAccessKey");
            String secretKey = ConfigHelper.GetConfig("Bank1PaySecretKey");
            string command = "get_transaction_detail";
            My1Pay my1Pay = new My1Pay();
            String signature = my1Pay.generateSignature_Bank_5CommitRequest(accessKey, command, providerTransId, secretKey); //create signature
            String urlParameter = String.Format("access_key={0}&trans_ref={1}&command={2}&signature={3}", accessKey, providerTransId, command, signature);
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
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
    }
}
