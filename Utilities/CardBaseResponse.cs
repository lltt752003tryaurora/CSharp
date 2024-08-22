using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public abstract class CardBaseResponse
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
        public string TelcoCode
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
        public string SerialNo
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
        public abstract void SetResponse(string cardType, string response);
    }
}
