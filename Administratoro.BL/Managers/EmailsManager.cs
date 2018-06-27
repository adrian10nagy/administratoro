﻿
namespace Administratoro.BL.Managers
{
    using Toolbox.Email;

    public static class EmailsManager
    {
        public static bool SendEmail(string addressFrom, string addressTo, string mailSubject, string mailBody)
        {
            return EmailClient.Send(addressFrom, addressTo, mailSubject, mailBody);
        }
    }
}