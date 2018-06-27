
namespace Administratoro.DAL.SDK
{
    using Administratoro.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

 public class Apartments
    {
        private static IApartmentRepository _repository;

        static Apartments()
        {
            _repository = new Repository();
        }

        #region Get

        public decimal GetSumOfIndiviza(int associationId)
        {
            return _repository.GetSumOfIndiviza(associationId);
        }

        public List<DAL.Apartments> GetByAss(int associationId, int stairCaseId)
        {
            return _repository.GetByAss(associationId, stairCaseId);
        }

        public List<DAL.Apartments> GetByAss(int associationId)
        {
            return _repository.GetByAss(associationId);
        }

        public DAL.Apartments Get(int id)
        {
            return _repository.Get(id);
        }

        public List<DAL.Apartments> GetAllEnabledForHeatHelp(int associationId)
        {
            return _repository.GetAllEnabledForHeatHelp(associationId);
        }

        public int GetDependentsNr(int associationId)
        {
            return _repository.GetDependentsNr(associationId);
        }

        public int GetDependentsNr(int associationId, int? stairCase)
        {
            return _repository.GetDependentsNr(associationId, stairCase);
        }

        #endregion
    }
}
