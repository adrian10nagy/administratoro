
namespace Administratoro.BL.Managers
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Models;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

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

        public static List<Expenses> GetAllExpenses()
        {
            return GetContext().Expenses.Where(e=>!e.specialType.HasValue).ToList();
        }

        public static List<Expenses> GetAllExpensesIncludingSpecialTypes()
        {
            return GetContext().Expenses.ToList();
        }

        public static Expenses GetById(int expenseId)
        {
            return GetContext().Expenses.FirstOrDefault(e => e.Id == expenseId);
        }

    }
}
