
namespace Administratoro.BL.Managers
{
    using Toolbox.Email;

    public static class EmailsManager
    {
        public static bool SendEmail(string addressFrom, string addressTo, string mailSubject, string mailBody, string filePath)
        {
            return EmailClient.Send(addressFrom, addressTo, mailSubject, mailBody, filePath);
        }
    }
}
