using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class BaseResponse
    {
        public object ReturnObj { get; set; }
        public EAction Action { get; set; }
        public bool IsCancel { get; set; }
        public ErrorCode ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public Exception Ex { get; set; }
    }
    public enum EAction
    {
        Add,
        Edit,
        Delete
    }
}
