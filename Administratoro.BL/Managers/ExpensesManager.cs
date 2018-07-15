
namespace Administratoro.BL.Managers
{
    using DAL;
    using System.Collections.Generic;
    using System.Linq;

    public static class ExpensesManager
    {
        private static AdministratoroEntities _administratoroEntities;

        private static AdministratoroEntities GetContext()
        {
            if (_administratoroEntities == null)
            {
                _administratoroEntities = new AdministratoroEntities();
            }

            return _administratoroEntities;
        }

        public static IEnumerable<Expenses> GetAllExpenses()
        {
            return GetContext().Expenses.Where(e => !e.specialType.HasValue);
        }


        public static Expenses GetById(int expenseId)
        {
            return GetContext().Expenses.FirstOrDefault(e => e.Id == expenseId);
        }


        public static IEnumerable<AssociationExpensesRedistributionTypes> GetRedistributiontypesForDiverse()
        {
            return GetContext().AssociationExpensesRedistributionTypes.Where(e => e.Id != 3).ToList();
        }
    }
}
