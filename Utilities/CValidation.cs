using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using System.Net.Mail;

namespace Utilities
{
    public class CValidation
    {
        public static bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        public static bool IsValidPhone(string phone)
        {
            try
            {
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

    }
}
