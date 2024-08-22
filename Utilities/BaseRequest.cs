using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class BaseRequest
    {
        public object Id { get; set; }
        public string Title { get; set; }
        public string TableName { get; set; }
        public object FormRequest  { get; set; }
        public bool viewOnly { get; set; }
    }
}
