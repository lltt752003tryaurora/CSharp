using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class GenerateCodeHelper
    {
        public static string GetClassName(string tableName)
        {
            if (tableName.EndsWith("ies"))
            {
                return tableName.Substring(0, tableName.Length - 3) + "y";
            }
            else if (tableName.EndsWith("s"))
            {
                return tableName.Substring(0, tableName.Length - 1);
            }
            else
            {
                return tableName;
            }
        }
        public static string GetCamelCaseName(string name)
        {
            if (name.Equals(name.ToUpper()))
                return name.ToLower();
            else
                return name.Substring(0, 1).ToLower() + name.Substring(1);
        }
    }
}
