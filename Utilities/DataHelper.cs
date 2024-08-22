using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utilities
{
    public class DataHelper
    {
        public static object GetValue(PropertyInfo pi, string property, object value)
        {
            try
            {
                //PropertyInfo pi = input.GetType().GetProperty(property);
                Type a = pi.PropertyType;
                switch (a.Name.ToLower())
                {
                    case "datetime":
                    case "date":
                        return (DateTime)value;
                        break;
                    case "byte":
                        byte byteValue = 0;
                        try
                        {
                            byteValue = byte.Parse(value.ToString());
                        }
                        catch (Exception e)
                        {
                        }
                        return byteValue;
                        break;
                    case "bit":
                        bool bValue = false;
                        try
                        {
                            bValue = bool.Parse(value.ToString());
                        }
                        catch (Exception e)
                        {
                        }
                        return bValue;
                        break;
                    default:
                        return value;
                        break;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
