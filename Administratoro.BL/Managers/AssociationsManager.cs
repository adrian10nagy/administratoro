
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

        public static Associations AddNew(Associations association)
        {
            GetContext(true).Associations.Add(association);
            GetContext().SaveChanges();

            return association;
        }


        public static List<Associations> GetAllAssociationsByPartner(int partnerId)
        {
            return GetContext(true).Associations.Where(e => e.Id_Partner == partnerId).ToList();
        }

        public static Associations GetById(int assocId)
        {
            return GetContext(true).Associations.FirstOrDefault(e => e.Id == assocId);
        }

        public static Associations GetByAssociationExpenseId(int associationExpenseId)
        {
            AssociationExpenses associationExpense = GetContext(true).AssociationExpenses.FirstOrDefault(e => e.Id == associationExpenseId);

            return GetContext(true).Associations.FirstOrDefault(e => e.Id == associationExpense.Id_Estate);
        }

        public static int GetNrOfApartments(int associationId)
        {
            var result = 0;
            var association = GetById(associationId);
            if (association != null)
            {
                result = association.Apartments.Count();
            }

            return result;
        }

        public static int GetNrOfApartments(int associationId, int? stairCaseId)
        {
            var result = 0;
            var association = GetById(associationId);
            if (association != null)
            {
                result = association.Apartments.Where(t => t.Id_StairCase == stairCaseId).Count();
            }

            return result;
        }

        public static void UpdateStairs(Associations es, bool hasStairs)
        {
            Associations associations = new Associations();
            associations = GetContext().Associations.First(b => b.Id == es.Id);

            if (associations != null)
            {
                associations.HasStaircase = hasStairs;
                GetContext().Entry(associations).CurrentValues.SetValues(associations);

                GetContext().SaveChanges();
            }
        }

        public static void Update(int id, Associations association)
        {
            if (association == null)
            {
                return;
            }

            var associations = new Associations();
            associations = GetContext().Associations.First(b => b.Id == association.Id);

            if (associations != null)
            {
                associations.Address = association.Address;
                associations.BanckAccont = association.BanckAccont;
                associations.CotaIndivizaAparments = association.CotaIndivizaAparments;
                associations.FiscalCode = association.FiscalCode;
                associations.HasStaircase = association.HasStaircase;
                associations.Name = association.Name;

                GetContext().Entry(associations).CurrentValues.SetValues(associations);

                GetContext().SaveChanges();
            }
        }

        public static void UpdateRoundUpColumn(Associations association, bool hasRoundCoulmn)
        {
            Associations associations = new Associations();
            associations = GetContext().Associations.First(b => b.Id == association.Id);

            if (associations != null)
            {
                associations.HasRoundUpColumn = hasRoundCoulmn;
                GetContext().Entry(associations).CurrentValues.SetValues(associations);

                GetContext().SaveChanges();
            }
        }
    }
}
