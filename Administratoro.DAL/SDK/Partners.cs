
namespace Administratoro.DAL.SDK
{
    using Repositories;

    public class Partners
    {
        private static IPartnerRepository _repository;

        static Partners()
        {
            _repository = new Repository();
        }

        #region Get

        public DAL.Partners Get(string email, string password)
        {
            return _repository.GetSumOfIndiviza(email, password);
        }

        #endregion
    }
}
