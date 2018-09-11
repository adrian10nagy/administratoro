
namespace Toolbox.Email
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Net;
    using System.Net.Mail;

    public static class EmailClient
    {
        public static bool Send(string addressFrom, string addressTo, string mailSubject, string mailBody, string filePath)
        {
            bool wasSend = false;

            try
            {
                MailMessage mail = InitializeMail(addressFrom, addressTo, mailSubject, mailBody, filePath);
                SmtpClient client = InitializeClient();

                client.Send(mail);

                wasSend = true;
            }
            catch (Exception e)
            {
                //log error
            }

            return wasSend;
        }

        private static MailMessage InitializeMail(string addressFrom, string addressTo, string mailSubject, string mailBody, string filePath)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(addressFrom),
                Subject = mailSubject,
                Body = mailBody,
                IsBodyHtml = true
            };

            mail.To.Add(new MailAddress(addressTo));
            if (File.Exists(filePath))
            {
                mail.Attachments.Add(
                    new Attachment(filePath)
                    {
                        
                    });
            }
            else
            {

            }

            return mail;
        }

        private static SmtpClient InitializeClient()
        {
            SmtpClient client = new SmtpClient
            {
                Host = ConfigurationManager.AppSettings["EmailHost"],
                Port = 587,
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                EnableSsl = true,
                Credentials = new NetworkCredential(ConfigurationManager.AppSettings["EmailAddress"], ConfigurationManager.AppSettings["EmailPassword"])
            };

            return client;
        }
    }
}
