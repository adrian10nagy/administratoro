
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

        public static IEnumerable<StairCases> GetAllByEstate(int estateId)
        {
            return GetContext().StairCases.Where(c => c.Id_Estate == estateId);
        }

        public static void AddNew(string name, Estates estate)
        {

            StairCases stairCase = new StairCases
            {
               Id_Estate = estate.Id,
               Value = name
            };

            GetContext().StairCases.Add(stairCase);
            GetContext().SaveChanges();
        }

        public static void Remove(int stairCaseId, int estate)
        {
            var staircase = GetContext().StairCases.FirstOrDefault(s => s.Id == stairCaseId && s.Id_Estate == estate);

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
    }
}
