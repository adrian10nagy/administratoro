
namespace Toolbox.Email
{
    using System;
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
            MailMessage mail = new MailMessage();
            mail.To.Add(new MailAddress(addressTo));
            mail.From = new MailAddress(addressFrom);
            mail.Subject = mailSubject;
            mail.Body = mailBody;
            mail.IsBodyHtml = true;
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
            SmtpClient client = new SmtpClient();
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.UseDefaultCredentials = false;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("asociatie.online@gmail.com", "21decembrie");
            return client;
        }
    }
}
