
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


        public static List<Tenants> GetAllByEstateId(int estateId)
        {
            return GetContext().Tenants.Where(t => t.Estates.Id == estateId).ToList();
        }

        public static DbSet<Tenants> GetAllAsDbSet(int estateId)
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

        public static List<Tenants> GetAllThatAreRegisteredWithSpecificCounters(int estateId, int esexId)
        {
            var result = new List<Tenants>();

            EstateExpenses estateExpense = EstateExpensesManager.GetById(esexId);
            if (estateExpense != null)
            {
                List<Tenants> allTenants = GetAllByEstateId(estateId);
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

        public static List<Tenants> GetAllByEstateIdAndStairCase(int estateId, int stairCaseId)
        {
            return GetContext().Tenants.Where(t => t.Estates.Id == estateId && t.Id_StairCase == stairCaseId).ToList();
        }
    }
}
