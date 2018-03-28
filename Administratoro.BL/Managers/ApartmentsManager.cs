
namespace Administratoro.BL.Managers
{
    using Administratoro.DAL;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using System.Linq;
    using System;

    public static class ApartmentsManager
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


        public static List<Tenants> GetAllByAssociationId(int associationId)
        {
            return GetContext().Tenants.Where(t => t.Estates.Id == associationId).OrderBy(a=>a.Number).ToList();
        }

        public static int GetDependentsNr(int associationId)
        {
            int result = 0;

            var association =  AssociationsManager.GetById(associationId);
            if(association != null)
            {
               result =  association.Tenants.Select(t => t.Dependents).Sum();
            }

            return result;
        }

        public static int GetDependentsNr(int associationId, int? stairCase)
        {
            int result = 0;

            var association = AssociationsManager.GetById(associationId);
            if (association != null)
            {
                result = association.Tenants.Where(t=>t.Id_StairCase == stairCase).Select(t => t.Dependents).Sum();
            }

            return result;
        }

        public static DbSet<Tenants> GetAllAsDbSet(int associationId)
        {
            return GetContext().Tenants;
        }

        public static Tenants GetById(int id)
        {
            return GetContext(true).Tenants.FirstOrDefault(x => x.Id == id);
        }

        public static void Update(Tenants tenant)
        {
            var result = GetContext().Tenants.SingleOrDefault(b => b.Id == tenant.Id);

            if (result != null)
            {
                result.Email = tenant.Email;
                result.ExtraInfo = tenant.ExtraInfo;
                result.Telephone = tenant.Telephone;
                result.Dependents = tenant.Dependents;
                result.Name = tenant.Name;
                result.Password = tenant.Password;
                result.TenantPersons = null;
                result.CotaIndiviza = tenant.CotaIndiviza;
                GetContext().Entry(result).CurrentValues.SetValues(tenant);

                GetContext().SaveChanges();
            }
        }

        public static Tenants Add(Tenants tenant)
        {
            Tenants result = null;

            result = GetContext().Tenants.Add(tenant);
            GetContext().SaveChanges();

            return result;
        }

        public static List<Tenants> GetAllThatAreRegisteredWithSpecificCounters(int associationId, int esexId)
        {
            var result = new List<Tenants>();

            EstateExpenses estateExpense = EstateExpensesManager.GetById(esexId);
            if (estateExpense != null)
            {
                List<Tenants> allTenants = GetAllByAssociationId(associationId);
                foreach (var tenant in allTenants)
                {
                    List<Counters> counters = CountersManager.GetByApartment(tenant.Id);

                    if(counters.Any(c=>c.Id_Expense == estateExpense.Expenses.Id))
                    {
                        result.Add(tenant);
                    }

                }
            }

            return result;
        }

        public static List<Tenants> GetAllByEstateIdAndStairCase(int associationId, int stairCaseId)
        {
            return GetContext().Tenants.Where(t => t.Estates.Id == associationId && t.Id_StairCase == stairCaseId).ToList();
        }

        public static decimal? GetSumOfIndivizaForAllTenants(int associationId)
        {
            decimal? result = null;

            var tenants = GetContext().Tenants.Where(t => t.id_Estate == associationId).ToList();

            if (tenants != null && tenants.Count > 0)
            {
                result = tenants.Sum(s => s.CotaIndiviza);
            }

            return result;
        }

        public static decimal? GetSumOfIndivizaForAllTenants(int associationId, int? stairCase)
        {
            decimal? result = null;

            var tenants = GetContext().Tenants.Where(t => t.id_Estate == associationId && t.Id_StairCase == stairCase).ToList();

            if (tenants != null && tenants.Count > 0)
            {
                result = tenants.Sum(s => s.CotaIndiviza);
            }

            return result;
        }

        public static IEnumerable<Tenants> GetAllEnabledForHeatHelp(int associationId)
        {
           return GetContext(true).Tenants.Where(t=>t.id_Estate == associationId && t.HasHeatHelp.HasValue && t.HasHeatHelp.Value);
        }
    }
}
