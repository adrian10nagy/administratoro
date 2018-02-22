
namespace Administratoro.BL.Managers
{
    using Administratoro.DAL;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    public static class AssociationsManager
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

        public static Estates AddNew(Estates association)
        {
            GetContext(true).Estates.Add(association);
            GetContext().SaveChanges();

            return association;
        }

        public static List<Estates> GetAll()
        {
            return GetContext(true).Estates.ToList();
        }

        public static List<Estates> GetAllAssociationsByPartner(int partnerId)
        {
            return GetContext(true).Estates.Where(e => e.Id_Partner == partnerId).ToList();
        }

        public static Estates GetById(int assocId)
        {
            return GetContext(true).Estates.FirstOrDefault(e => e.Id == assocId);
        }

        public static Estates GetByEstateExpenseId(int estateExpenseId)
        {
            EstateExpenses estateExpense = GetContext(true).EstateExpenses.FirstOrDefault(e => e.Id == estateExpenseId);

            return GetContext(true).Estates.FirstOrDefault(e => e.Id == estateExpense.Id_Estate);
        }

        public static int GetNrOfApartments(int associationId)
        {
            var result = 0;
            var association = GetById(associationId);
            if (associationId != null)
            {
                result = association.Tenants.Count();
            }

            return result;
        }

        public static int GetNrOfApartments(int associationId, int? stairCaseId)
        {
            var result = 0;
            var association = GetById(associationId);
            if (association != null)
            {
                result = association.Tenants.Where(t => t.Id_StairCase == stairCaseId).Count();
            }

            return result;
        }

        public static void UpdateStairs(Estates es, bool hasStairs)
        {
            Estates estate = new Estates();
            estate = GetContext().Estates.First(b => b.Id == es.Id);

            if (estate != null)
            {
                estate.HasStaircase = hasStairs;
                GetContext().Entry(estate).CurrentValues.SetValues(estate);

                GetContext().SaveChanges();
            }
        }

        public static void Update(int id, Estates association)
        {
            if (association == null)
            {
                return;
            }

            var estate = new Estates();
            estate = GetContext().Estates.First(b => b.Id == association.Id);

            if (estate != null)
            {
                estate.Address = association.Address;
                estate.BanckAccont = association.BanckAccont;
                estate.CotaIndivizaAparments = association.CotaIndivizaAparments;
                estate.FiscalCode = association.FiscalCode;
                estate.HasStaircase = association.HasStaircase;
                estate.Name = association.Name;

                GetContext().Entry(estate).CurrentValues.SetValues(estate);

                GetContext().SaveChanges();
            }
        }

        public static void UpdateRoundUpColumn(Estates association, bool hasRoundCoulmn)
        {
            Estates estate = new Estates();
            estate = GetContext().Estates.First(b => b.Id == association.Id);

            if (estate != null)
            {
                estate.HasRoundUpColumn = hasRoundCoulmn;
                GetContext().Entry(estate).CurrentValues.SetValues(estate);

                GetContext().SaveChanges();
            }
        }
    }
}
