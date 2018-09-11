
namespace Administratoro.DAL.SDK
{
    using Administratoro.DAL.Repositories;
    using System;

    public class RegistriesHomeDaily
    {
        private static IRegistryHomeDailyRepository _repository;

        static RegistriesHomeDaily()
        {
            _repository = new Repository();
        }

        public int Add(DAL.RegistriesHomeDaily registriesHomeDaily)
        {
            return _repository.Add(registriesHomeDaily);
        }

        public DAL.RegistriesHomeDaily Get(int assId, DateTime date)
        {
            return _repository.Get(assId, date);
        }


        public DAL.RegistriesHomeDaily GetById(int id)
        {
            return _repository.GetById(id);
        }
    }
}
