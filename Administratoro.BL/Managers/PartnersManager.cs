
namespace Administratoro.BL.Managers
{
    using Administratoro.DAL.SDK;

    public static class PartnersManager
    {
        public static DAL.Partners Login(string email, string password)
        {
            return Kit.Instance.Partners.Get(email, password);
        }
    }
}
