using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public abstract class BankBaseResponse
    {
        public const string SUCC = "succ";
        public const string FAIL = "fail";
        public const string PENDING = "pending";
        public Dictionary<string, string> RStatus = new Dictionary<string, string>();
        public abstract string GetTransactionStatus(string code);
        public string GetMessage()
        {
            try
            {
                return this.RStatus[this.ResponseStatus];
            }
            catch
            {
                return "Lỗi không xác định";
            }
        }

        public string ResponseStatus
        {
            get; set;
        }
        public string ProviderTransactionId
        {
            get; set;
        }
        public string TransactionId
        {
            get; set;
        }
        public int Amt
        {
            get; set;
        }
        public string Description
        {
            get; set;
        }
        /// <summary>
        /// Đại diện cho sản phẩm của merchant khai báo trong hệ thống 1pay.vn
        /// </summary>
        public string AccessKey
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
        public string Version
        {
            get; set;
        }
        public abstract void SetResponse(string cardType, string response);
    }
}
