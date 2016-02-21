using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net.Mime;

namespace LogParserService.Helpers
{
    public static class Email
    {
     
        public static void Send(
            string from,
            string to,
            string cc,
            string subject,
            string body,
            bool isHtml = false,
            MailPriority prio = MailPriority.Normal,
            Attachment attachment = null)
        {
            try
            {
                // TODO: Host should be in the config
                SmtpClient mailhost = new SmtpClient("mailhost.swissbank.com");
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(from);
                mail.To.Add(to);
                if (!string.IsNullOrEmpty(cc))
                    mail.CC.Add(cc);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = isHtml;
                mail.Priority = prio;
                if (attachment != null)
                {
                    mail.Attachments.Add(attachment);
                }
                mailhost.Send(mail);
                mail.Dispose();
                mail = null;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    string.Format(
                        "Error sending Email [Subject: {0}, To: {1}, CC:{2}, From: {3}]. Error: {4}", 
                        subject, to, cc, from, ex.Message),
                    ex);
            }
        }

        public static void Send(
            string from,
            string to,
            string subject,
            string body,
            bool isHtml = false,
            MailPriority prio = MailPriority.Normal,
            Attachment attachment = null)
        {
            Send(from, to, null, subject, body, isHtml, prio, attachment);
        }

        public static void AttachAndSend(
            string from,
            string to,
            string subject,
            string body,
            bool isHtml = false,
            MailPriority prio = MailPriority.Normal,
            string attachment = null)
        {

            Attachment data = new Attachment(attachment, MediaTypeNames.Application.Octet);
            Send(from, to, null, subject, body, isHtml, prio, data);
        }
    }
}
