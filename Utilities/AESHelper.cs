using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;


namespace Utilities
{
    public class AESHelper
    {
        public static String Encrypt(String rawText, string key = "jZjneNba78tqCuB844eQrXoAigmbjIwwngYfjEdnnLg=", string iv = "Uty66eAigmbjIwwng3532JFbeRxGJzhl4Ymw9ry6Slc=")
        {
            var aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 256;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Convert.FromBase64String(key);
            aes.IV = Convert.FromBase64String(iv);

            var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                {
                    byte[] xXml = Encoding.UTF8.GetBytes(rawText);
                    cs.Write(xXml, 0, xXml.Length);
                }

                xBuff = ms.ToArray();
            }

            String Output = Convert.ToBase64String(xBuff);
            return Output;
        }

        public static String Decrypt(String encryptText, string key = "jZjneNba78tqCuB844eQrXoAigmbjIwwngYfjEdnnLg=", string iv = "Uty66eAigmbjIwwng3532JFbeRxGJzhl4Ymw9ry6Slc=")
        {
            RijndaelManaged aes = new RijndaelManaged();
            aes.KeySize = 256;
            aes.BlockSize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = Convert.FromBase64String(key);
            aes.IV = Convert.FromBase64String(iv);

            var decrypt = aes.CreateDecryptor();
            byte[] xBuff = null;
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                {
                    byte[] xXml = Convert.FromBase64String(encryptText);
                    cs.Write(xXml, 0, xXml.Length);
                }
                xBuff = ms.ToArray();
            }

            String Output = Encoding.UTF8.GetString(xBuff);
            return Output;
        }
    }
}
