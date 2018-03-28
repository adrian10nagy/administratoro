
namespace Administratoro.BL.Managers
{
    using Administratoro.BL.Constants;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class StairCasesManager
    {
        private static AdministratoroEntities _administratoroEntities;

        private static AdministratoroEntities GetContext(bool shouldRefresh = false)
        {
            if (_administratoroEntities == null || shouldRefresh)
            {
                _administratoroEntities = new AdministratoroEntities();
            }

            return _administratoroEntities;
        }

        public static IEnumerable<StairCases> GetAllByEstate(int associationId)
        {
            return GetContext().StairCases.Where(c => c.Id_Estate == associationId);
        }

        public static void AddNew(Estates estate, string name, decimal? indiviza)
        {

            StairCases stairCase = new StairCases
            {
                Id_Estate = estate.Id,
                Nume = name,
                Indiviza = indiviza
            };

            GetContext().StairCases.Add(stairCase);
            GetContext().SaveChanges();
        }

        public static void Remove(int stairCaseId, int estate)
        {
            var staircase = GetContext(true).StairCases.FirstOrDefault(s => s.Id == stairCaseId && s.Id_Estate == estate);

            if (staircase != null)
            {
                foreach (var tenants in staircase.Tenants)
                {
                    tenants.Id_StairCase = null;
                }
                GetContext().StairCases.Remove(staircase);
                GetContext().SaveChanges();
            }
        }

        public static void Update(StairCases newStairCase, int stairId)
        {
            var staircase = GetContext(true).StairCases.FirstOrDefault(s => s.Id == stairId);

            if (staircase != null)
            {
                staircase.Indiviza = newStairCase.Indiviza;
                staircase.Nume = newStairCase.Nume;
                GetContext().Entry(staircase).CurrentValues.SetValues(staircase);

                GetContext().SaveChanges();
            }
        }

        public static StairCases GetById(int stairId)
        {
            return GetContext().StairCases.FirstOrDefault(s => s.Id == stairId);
        }
    }
}
