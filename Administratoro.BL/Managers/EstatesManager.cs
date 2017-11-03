
namespace Administratoro.BL.Managers
{
    using Administratoro.DAL;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    public static class EstatesManager
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

        public static List<Estates> GetAllEstates()
        {
            return GetContext(true).Estates.ToList();
        }

        public static List<Estates> GetAllEstatesByPartner(int partnerId)
        {
            return GetContext(true).Estates.Where(e=>e.Id_Partner==partnerId).ToList();
        }

        public static Estates GetById(int estateId)
        {
            return GetContext(true).Estates.FirstOrDefault(e => e.Id == estateId);
        }

        public static Estates GetByEstateExpenseId(int estateExpenseId)
        {
            EstateExpenses estateExpense = GetContext(true).EstateExpenses.FirstOrDefault(e => e.Id == estateExpenseId);

            return GetContext(true).Estates.FirstOrDefault(e => e.Id == estateExpense.Id_Estate);
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
    }
}
