using _1Pay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class Card1PayHelper
    {
        public class CardType
        {
            public const string Viettel = "viettel";
            public const string Mobifone = "mobifone";
            public const string Vinaphone = "vinaphone";
            public const string Gate = "gate";
            public const string Vcoin = "vcoin";
            public const string Zing = "zing";
            public const string Vnmobile = "vnmobile";
        }
        public class CardResponse: CardBaseResponse
        {
            public CardResponse()
            {
                RStatus = new Dictionary<string, string>()
            {
                {"00","Giao dịch thành công"},
                {"01","Lỗi, địa chỉ IP truy cập API bị từ chối"},
                {"02","Lỗi, tham số gửi từ merchant tới chưa chính xác (thường sai tên tham số hoặc thiếu tham số)"},
                {"03","Lỗi, merchant không tồn tại hoặc merchant đang bị khóa kết nối."},
                {"04","Mật khẩu hoặc chữ ký xác thực không chính xác."},
                {"05","Trùng mã giao dịch (transRef)."},
                {"06","Mã giao dịch không tồn tại hoặc sai định dạng."},
                {"07","Thẻ đã được sử dụng, hoặc thẻ sai."},
                {"08","Thẻ bị khóa"},
                {"09","Thẻ hết hạn sử dụng."},
                {"10","Thẻ chưa được kích hoạt hoặc không tồn tại."},
                {"11","Mã thẻ sai định dạng."},
                {"12","Sai số serial của thẻ."},
                {"13","Mã thẻ và số serial không khớp."},
                {"14","Thẻ không tồn tại"},
                {"15","Thẻ không sử dụng được."},
                {"16","Số lần thử (nhập sai liên tiếp) của thẻ vượt quá giới hạn cho phép"},
                {"17","Hệ thống đơn vị phát hành (Telco) bị lỗi hoặc quá tải, thẻ chưa bị trừ."},
                {"18","Hệ thống đơn vị phát hành (Telco) bị lỗi hoặc quá tải, thẻ có thể bị trừ, cần phối hợp với 1pay để tra soát"},
                {"19","Đơn vị phát hành không tồn tại"},
                {"20","Đơn vị phát hành không hỗ trợ nghiệp vụ này"},
                {"21","Không hỗ trợ loại card này"},
                {"22","Kết nối tới hệ thống đơn vị phát hành (Telco) bị lỗi, thẻ chưa bị trừ (thường do lỗi kết nối với Telco, ví dụ sai tham số kết nối, mà không liên quan đến merchant)."},
                {"23","Kết nối 1Pay tới hệ thống đơn vị cung cấp bị lỗi, thẻ chưa bị trừ."},
                {"99","Lỗi, tuy nhiên lỗi chưa được định nghĩa hoặc chưa xác định được nguyên nhân"}
            };
            }
            public override string GetTransactionStatus(string code)
            {
                if (this.ResponseStatus == "18" || this.ResponseStatus == "99")
                {
                    return PENDING;
                }
                if (this.ResponseStatus == "00")
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
                    this.ProviderTransactionId = JsonHelper.GetValue(jObj, "transId");
                    this.TransactionId = JsonHelper.GetValue(jObj, "transRef");
                    this.SerialNo = JsonHelper.GetValue(jObj, "serial");
                    this.ResponseStatus = JsonHelper.GetValue(jObj, "status");
                    this.Amt = NumberHelper.ConvertToInt(JsonHelper.GetValue(jObj, "amount"));
                    this.Description = JsonHelper.GetValue(jObj, "description");
                }
                catch { }
            }
        }
        public static string CardTopup(string type, string pin, string serial, string transRef)
        {
            String result = "";
            String url = ConfigHelper.GetConfig("Card1PayUrl") + "topup"; //"https://api.1pay.vn/card-charging/v5/topup";
            String accessKey = ConfigHelper.GetConfig("Card1PayAccessKey");
            String secretKey = ConfigHelper.GetConfig("Card1PaySecretKey");
            My1Pay my1Pay = new My1Pay();
            String signature = my1Pay.generateSignature_Card_V5_TopupApi(accessKey, pin, serial, transRef, type, secretKey); //create signature
            String urlParameter = String.Format("access_key={0}&type={1}&pin={2}&serial={3}&signature={4}&transRef={5}", accessKey, type, pin, serial, signature, transRef);
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
        public static string CardQuery(string type, string pin, string serial, string transId, string transRef)
        {
            String result = "";
            String url = ConfigHelper.GetConfig("Card1PayUrl") + "query"; //"https://api.1pay.vn/card-charging/v5/topup";
            String accessKey = ConfigHelper.GetConfig("Card1PayAccessKey");
            String secretKey = ConfigHelper.GetConfig("Card1PaySecretKey");
            My1Pay my1Pay = new My1Pay();
            String signature = my1Pay.generateSignature_Card_V5_QueryApi(accessKey, pin, serial, transId, transRef, type, secretKey); //create signature
            String urlParameter = String.Format("access_key={0}&type={1}&pin={2}&serial={3}&signature={4}&transRef={5}&transId={6}", accessKey, type, pin, serial, signature, transRef, transId);
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
                //result = e.GetBaseException().ToString();
                throw e;
            }
            return result;
        }
    }
}
