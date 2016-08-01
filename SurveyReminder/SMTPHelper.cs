using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;
using SendGrid;
using System.Text;
using System.Configuration;

namespace SurveyReminder
{
    public class SMTPHelper
    {
        public static bool SendEmail(string body, string subject, IEnumerable<string> TOs, bool isHtml, IEnumerable<string> CCs = null, IEnumerable<string> BCCs = null)
        {
            string smtpServer = Convert.ToString(ConfigurationManager.AppSettings["SMTPSERVER"]); //"smtp.gmail.com";
            int smtpPort = Convert.ToInt32(ConfigurationManager.AppSettings["SMTPPORT"]);
            string sender = Convert.ToString(ConfigurationManager.AppSettings["SMTPSENDER"]);
            string password = Encryption.Decrypt(Convert.ToString(ConfigurationManager.AppSettings["key2"]), ConfigurationManager.AppSettings["key5"].ToString(), false);

            var message = new MailMessage { Body = body, BodyEncoding = Encoding.UTF8, IsBodyHtml = isHtml, Subject = subject, From = new MailAddress(sender) };

            foreach (var r in TOs)
            {
                message.To.Add(r);
            }

            if (CCs != null)
            {
                foreach (var c in CCs)
                {
                    message.CC.Add(c);
                }
            }

            if (BCCs != null)
            {
                foreach (var c in BCCs)
                {
                    message.Bcc.Add(c);
                }
            }

            using (SmtpClient objSMTPClient = new SmtpClient())
            {
                objSMTPClient.EnableSsl = true;
                objSMTPClient.UseDefaultCredentials = false;
                objSMTPClient.Credentials = new NetworkCredential(sender, password);
                objSMTPClient.Host = smtpServer;
                objSMTPClient.Port = smtpPort;
                objSMTPClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                try
                {
                    objSMTPClient.Send(message);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool SendGridEmail(string subject, string body, IEnumerable<string> TOs, bool isHtml, IEnumerable<string> CCs = null, IEnumerable<string> BCCs = null)
        {
            string sender = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SMTPSENDER"]);
            string password = Encryption.Decrypt(Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["key2"]), System.Configuration.ConfigurationManager.AppSettings["key5"].ToString(), false);

            try
            {
                
                var msg = new SendGridMessage();                
                msg.Subject = subject;
                msg.Html = body;

                msg.From = new MailAddress(sender);
                                
                bool isEmailEnabled = Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["SendEmails"]);
                if (isEmailEnabled)
                {
                    msg.AddTo(TOs);
                    List<MailAddress> ccEmails = new List<MailAddress>();
                    List<MailAddress> bccEmails = new List<MailAddress>();

                    if (CCs != null)
                    {
                        foreach (var c in CCs)
                        {
                            ccEmails.Add(new MailAddress(c));                            
                        }
                        msg.Cc = ccEmails.ToArray();
                    }

                    if (BCCs != null)
                    {
                        foreach (var c in BCCs)
                        {
                            bccEmails.Add(new MailAddress(c));                            
                        }
                        msg.Bcc = bccEmails.ToArray();
                    }
                }
                else
                {
                    msg.AddTo(System.Configuration.ConfigurationManager.AppSettings["TestEmailAddress"].ToString());
                }

                var credentials = new NetworkCredential(sender, password);
                var transportWeb = new Web(credentials);
                transportWeb.DeliverAsync(msg).Wait();
                

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}