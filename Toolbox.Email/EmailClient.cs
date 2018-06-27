using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Toolbox.Email
{
    public static class EmailClient
    {
        public static bool Send(string addressFrom, string addressTo, string mailSubject, string mailBody)
        {
            bool wasSend = false;

            try
            {
                var filePath = "C:\\Users\\Adrian\\Documents\\fluturasi\\1lucaciu.pdf";

                if(File.Exists(filePath))
                {

                }
                MailMessage mail = new MailMessage();
                mail.To.Add(new MailAddress(addressTo));
                mail.From = new MailAddress(addressFrom);
                mail.Subject = mailSubject;
                mail.Body = mailBody;
                mail.IsBodyHtml = true;
                mail.Attachments.Add(
                    new Attachment(filePath)
                    {
                        
                    });

                SmtpClient client = InitializeClient();
                client.Send(mail);

                wasSend = true;
            }
            catch(Exception e)
            {
                //log error
            }

            return wasSend;
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
