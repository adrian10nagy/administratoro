
using System;
using System.Collections.Generic;
using Administratoro.DAL.Repositories;

namespace Administratoro.DAL.SDK
{
    public class RegistriesHome
    {
        private static IRegistryHomeRepository _repository;

        static RegistriesHome()
        {
            _repository = new Repository();
        }

        public List<DAL.RegistriesHome> GetByAssandDate(int associationId, DateTime date)
        {
            return _repository.GetByAssAndDate(associationId, date);
        }

    }
}
