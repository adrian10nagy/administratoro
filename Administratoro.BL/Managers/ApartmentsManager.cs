
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


        public static List<Apartments> Get(int associationId)
        {
            return GetContext().Apartments.Where(t => t.Associations.Id == associationId).OrderBy(a => a.Number).ToList();
        }

        public static int GetDependentsNr(int associationId)
        {
            int result = 0;

            var association =  AssociationsManager.GetById(associationId);
            if(association != null)
            {
                result = association.Apartments.Select(t => t.Dependents).Sum();
            }

            return result;
        }

        public static int GetDependentsNr(int associationId, int? stairCase)
        {
            int result = 0;

            var association = AssociationsManager.GetById(associationId);
            if (association != null)
            {
                result = association.Apartments.Where(t => t.Id_StairCase == stairCase).Select(t => t.Dependents).Sum();
            }

            return result;
        }

        public static DbSet<Apartments> GetAsDbSet(int associationId)
        {
            return GetContext().Apartments;
        }

        public static Apartments GetById(int id)
        {
            return GetContext(true).Apartments.FirstOrDefault(x => x.Id == id);
        }

        public static void Update(Apartments apartment)
        {
            var result = GetContext().Apartments.SingleOrDefault(b => b.Id == apartment.Id);

            if (result != null)
            {
                result.Email = apartment.Email;
                result.ExtraInfo = apartment.ExtraInfo;
                result.Telephone = apartment.Telephone;
                result.Dependents = apartment.Dependents;
                result.Name = apartment.Name;
                result.Password = apartment.Password;
                result.CotaIndiviza = apartment.CotaIndiviza;
                GetContext().Entry(result).CurrentValues.SetValues(apartment);

                GetContext().SaveChanges();
            }
        }

        public static Apartments Add(Apartments apartment)
        {
            Apartments result = null;

            result = GetContext().Apartments.Add(apartment);
            GetContext().SaveChanges();

            return result;
        }

        public static List<Apartments> GetAllThatAreRegisteredWithSpecificCounters(int associationId, int esexId)
        {
            var result = new List<Apartments>();

            AssociationExpenses associationExpense = AssociationExpensesManager.GetById(esexId);
            if (associationExpense != null)
            {
                List<Apartments> allApartments = Get(associationId);
                foreach (var apartment in allApartments)
                {
                    IEnumerable<AssociationCounters> counters = CountersManager.GetByApartment(apartment.Id);

                    if(counters.Any(c=>c.Id_Expense == associationExpense.Expenses.Id))
                    {
                        result.Add(apartment);
                    }

                }
            }

            return result;
        }

        public static List<Apartments> Get(int associationId, int stairCaseId)
        {
            return GetContext().Apartments.Where(t => t.Associations.Id == associationId && t.Id_StairCase == stairCaseId).ToList();
        }

        public static decimal? GetSumOfIndivizaForAllApartments(int associationId)
        {
            decimal? result = null;

            var apartments = GetContext().Apartments.Where(t => t.id_Estate == associationId).ToList();

            if (apartments != null && apartments.Count > 0)
            {
                result = apartments.Sum(s => s.CotaIndiviza);
            }

            return result;
        }

        public static decimal? GetSumOfIndivizaForAllApartments(int associationId, int? stairCase)
        {
            decimal? result = null;

            var apartments = GetContext().Apartments.Where(t => t.id_Estate == associationId && t.Id_StairCase == stairCase).ToList();

            if (apartments != null && apartments.Count > 0)
            {
                result = apartments.Sum(s => s.CotaIndiviza);
            }

            return result;
        }

        public static IEnumerable<Apartments> GetAllEnabledForHeatHelp(int associationId)
        {
            return GetContext(true).Apartments.Where(t => t.id_Estate == associationId && t.HasHeatHelp.HasValue && t.HasHeatHelp.Value);
        }
    }
}
