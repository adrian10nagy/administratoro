
namespace Administratoro.DAL.SDK
{
    using DAL;
    using DAL.Repositories;
    using System.Collections;
    using System.Collections.Generic;

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
