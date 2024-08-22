using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Net.Mail;
using System.Collections.Specialized;
using System.Web.Script.Serialization;

namespace Utilities
{
    public class Mailer
    {
        private string serverIp = "127.0.0.1";
        private int port = 25;
        private string userName = string.Empty;
        private string password = string.Empty;
        public bool enableSsl = false;
        string qName = ConfigHelper.GetConfig("QueueEmail");

        /// <summary>
        /// Initializes a new instance of the <see cref="Mailer"/> class.
        /// </summary>
        /// <param name="serverIp">The server ip.</param>
        /// <param name="port">The port.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        public Mailer(string serverIp, int port, string userName, string password, bool enableSsl = false)
        {
            this.serverIp = serverIp;
            this.port = port;
            this.userName = userName;
            this.password = password;
            this.enableSsl = enableSsl;
        }
        /// <summary>
        /// Send mail to one person.
        /// </summary>
        /// <param name="sSubject">The s subject.</param>
        /// <param name="sBody">The s body.</param>
        /// <param name="sFromAddress">The s from address.</param>
        /// <param name="sFromName">Name of the s from.</param>
        /// <param name="sToAddress">The s to address.</param>
        /// <param name="bIsHTML">if set to <c>true</c> [b is HTML].</param>
        /// <param name="sCharSet">The s char set.</param>
        public void SendEmail(string sSubject, string sBody, string sFromAddress, string sFromName, string sToAddress, Boolean bIsHTML, string sCharSet)
        {

            try
            {
                SmtpClient mailClient = new SmtpClient(this.serverIp, this.port);

                mailClient.EnableSsl = this.enableSsl;
                if (!string.IsNullOrEmpty(this.userName) || !string.IsNullOrEmpty(this.password))
                {
                    mailClient.Credentials = new System.Net.NetworkCredential(this.userName, this.password);
                    mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                }
                MailMessage message = new MailMessage();
                message.From = new MailAddress(sFromAddress, sFromName);
                foreach (string to in sToAddress.Split(','))
                {
                    message.To.Add(to);
                }
                message.IsBodyHtml = true;
                if (sCharSet.Equals("utf-8")) message.BodyEncoding = System.Text.Encoding.UTF8;
                message.Subject = sSubject;
                message.Body = sBody;
                mailClient.Timeout = ConfigHelper.GetConfig("SendMailTimeoutSecond", 30) * 1000;
                try
                {
                    mailClient.Send(message);
                }
                catch (Exception e)
                {
                    QueueHelper _queue = new QueueHelper(qName);
                    MailObj m = new MailObj();
                    m.Body = sBody;
                    m.CharSet =sCharSet;
                    m.FromAddress = sFromAddress;
                    m.FromName = sFromName;
                    m.IsHTML = bIsHTML;
                    m.Subject = sSubject;
                    m.ToAddress = sToAddress;
                    _queue.Send(m);
                    //throw e;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Send mail to one person.
        /// </summary>
        /// <param name="mailHost">The mail host.</param>
        /// <param name="sSubject">The s subject.</param>
        /// <param name="sBody">The s body.</param>
        /// <param name="sFromAddress">The s from address.</param>
        /// <param name="sFromName">Name of the s from.</param>
        /// <param name="sToAddress">The s to address.</param>
        /// <param name="bIsHTML">if set to <c>true</c> [b is HTML].</param>
        /// <param name="sCharSet">The s char set.</param>
        public void SendEmail(string mailHost, string sSubject, string sBody, string sFromAddress, string sFromName, string sToAddress, Boolean bIsHTML, string sCharSet)
        {
            try
            {
                SmtpClient mailClient = new SmtpClient(this.serverIp, this.port);
                mailClient.EnableSsl = this.enableSsl;
                if (!string.IsNullOrEmpty(this.userName) || !string.IsNullOrEmpty(this.password)) mailClient.Credentials = new System.Net.NetworkCredential(this.userName, this.password);
                MailMessage message = new MailMessage();
                message.From = new MailAddress(sFromAddress, sFromName);
                message.To.Add(sToAddress);
                message.IsBodyHtml = true;
                if (sCharSet.Equals("utf-8")) message.BodyEncoding = System.Text.Encoding.UTF8;
                message.Subject = sSubject;
                message.Body = sBody;
                mailClient.Send(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Send mail to many people.
        /// </summary>
        /// <param name="sSubject">The s subject.</param>
        /// <param name="sBody">The s body.</param>
        /// <param name="sFromAddress">The s from address.</param>
        /// <param name="sFromName">Name of the s from.</param>
        /// <param name="arrToAddress">The arr to address.</param>
        /// <param name="arrCCAddress">The arr CC address.</param>
        /// <param name="arrBCCAddress">The arr BCC address.</param>
        /// <param name="bIsHTML">if set to <c>true</c> [b is HTML].</param>
        /// <param name="sCharSet">The s char set.</param>
        public void SendMultiEmails(string sSubject, string sBody, string sFromAddress, string sFromName, string[] arrToAddress, string[] arrCCAddress, string[] arrBCCAddress, Boolean bIsHTML, string sCharSet)
        {
            try
            {
                SmtpClient mailClient = new SmtpClient(this.serverIp, this.port);
                mailClient.EnableSsl = this.enableSsl;
                if (!string.IsNullOrEmpty(this.userName) || !string.IsNullOrEmpty(this.password)) mailClient.Credentials = new System.Net.NetworkCredential(this.userName, this.password);
                MailMessage message = new MailMessage();
                message.From = new MailAddress(sFromAddress, sFromName);
                StringCollection strarrToAddress = new StringCollection();
                StringCollection strarrCCAddress = new StringCollection();
                StringCollection strarrBCCAddress = new StringCollection();
                strarrToAddress.AddRange(arrToAddress);

                for (int i = 0; i < strarrToAddress.Count; i++)
                    message.To.Add(strarrToAddress[i].ToString());
                if (arrCCAddress != null)
                {
                    strarrCCAddress.AddRange(arrCCAddress);
                    for (int j = 0; j < strarrCCAddress.Count; j++)
                        message.CC.Add(strarrCCAddress[j].ToString());
                }
                if (arrBCCAddress != null)
                {
                    strarrBCCAddress.AddRange(arrBCCAddress);
                    for (int k = 0; k < strarrBCCAddress.Count; k++)
                        message.Bcc.Add(strarrBCCAddress[k].ToString());
                }
                message.IsBodyHtml = true;
                if (sCharSet.Equals("utf-8"))
                {
                    message.BodyEncoding = System.Text.Encoding.UTF8;
                }
                message.Subject = sSubject;
                message.Body = sBody;
                mailClient.Send(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Sends to many.
        /// </summary>
        /// <param name="sSubject">The s subject.</param>
        /// <param name="sBody">The s body.</param>
        /// <param name="sFromAddress">The s from address.</param>
        /// <param name="sFromName">Name of the s from.</param>
        /// <param name="toAddress">To address.</param>
        /// <param name="ccAddress">The cc address.</param>
        /// <param name="bIsHTML">if set to <c>true</c> [b is HTML].</param>
        /// <param name="sCharSet">The s char set.</param>
        public void SendToMany(string sSubject, string sBody, string sFromAddress, string sFromName, string toAddress, string ccAddress, Boolean bIsHTML, string sCharSet)
        {
            try
            {
                SmtpClient mailClient = new SmtpClient(this.serverIp, this.port);
                mailClient.EnableSsl = this.enableSsl;
                if (!string.IsNullOrEmpty(this.userName) || !string.IsNullOrEmpty(this.password)) mailClient.Credentials = new System.Net.NetworkCredential(this.userName, this.password);
                MailMessage message = new MailMessage();
                message.From = new MailAddress(sFromAddress, sFromName);
                string[] arrToAddress = toAddress.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                string[] arrCCAddress = ccAddress.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                //to address
                foreach (string toadd in arrToAddress)
                    message.To.Add(toadd);

                //cc
                foreach (string ccadd in arrCCAddress)
                    message.CC.Add(ccadd);

                message.IsBodyHtml = true;
                if (sCharSet.Equals("utf-8"))
                {
                    message.BodyEncoding = System.Text.Encoding.UTF8;
                }
                message.Subject = sSubject;
                message.Body = sBody;
                mailClient.Send(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SendEmailAttachFile(string sSubject, string sBody, string sFromAddress, string sFromName, string sToAddress, Boolean bIsHTML, string sCharSet, string rootfileName)
        {
            try
            {
                SmtpClient mailClient = new SmtpClient(this.serverIp, this.port);
                mailClient.EnableSsl = this.enableSsl;
                if (!string.IsNullOrEmpty(this.userName) || !string.IsNullOrEmpty(this.password))
                    mailClient.Credentials = new System.Net.NetworkCredential(this.userName, this.password);
                MailMessage message = new MailMessage();
                message.From = new MailAddress(sFromAddress, sFromName);
                message.To.Add(sToAddress);
                message.IsBodyHtml = true;

                if (sCharSet.Equals("utf-8"))
                    message.BodyEncoding = System.Text.Encoding.UTF8;
                message.Subject = sSubject;
                message.Body = sBody;
                Attachment attachFile = new Attachment(rootfileName);
                message.Attachments.Add(attachFile);
                mailClient.Send(message);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SendEmail(MailMessage message, ref string log)
        {
            log = CLogger.Append(log, "SendEmail");
            long t1 = DateHelper.GetTimeStamp(DateTime.Now);
            try
            {
                SmtpClient mailClient = new SmtpClient(this.serverIp, this.port);

                mailClient.EnableSsl = this.enableSsl;
                if (!string.IsNullOrEmpty(this.userName) || !string.IsNullOrEmpty(this.password))
                {
                    mailClient.Credentials = new System.Net.NetworkCredential(this.userName, this.password);
                    mailClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                }
                mailClient.Timeout = ConfigHelper.GetConfig("SendMailTimeoutSecond", 30) * 1000;
                try
                {
                    log = CLogger.Append(log, "Send");
                    mailClient.Send(message);
                    long t2 = DateHelper.GetTimeStamp(DateTime.Now);
                    log = CLogger.Append(log, "Succ", t1, t2, t2 - t1);
                }
                catch (Exception e)
                {
                    log = CLogger.Append(log, "ex>>", e.Message, qName);
                    QueueHelper _queue = new QueueHelper(qName);
                    string s = new JavaScriptSerializer().Serialize(message);
                    log = CLogger.Append(log, "Send");
                    _queue.Send(s);
                    log = CLogger.Append(log, "Succ");
                    throw e;
                }
            }
            catch (Exception ex)
            {
                log = CLogger.Append(log, "ex>>", ex.Message);
            }
        }
    }
    [Serializable]
    public class MailObj
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string ToAddress { get; set; }
        public Boolean IsHTML { get; set; }
        public string CharSet { get; set; }
        public MailObj() { }
    }
}
