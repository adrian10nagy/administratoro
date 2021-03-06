﻿
namespace Administratoro.DAL.SDK
{
    using Repositories;
    using System.Collections.Generic;

    public class Localities
    {
        private static ILocalityRepository _repository;

        static Localities()
        {
            _repository = new Repository();
        }

        #region Get

        public IEnumerable<DAL.Localities> GetLocalitiesAll()
        {
            return _repository.GetLocalitiesAll();
        }

        #endregion
    }
}
