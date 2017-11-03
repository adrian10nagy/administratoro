
namespace Administratoro.BL.Managers
{
    using Administratoro.BL.Constants;
    using Administratoro.DAL;
    using System.Collections.Generic;
    using System.Data.Entity;
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

        public static DbSet<Expenses> GetAllExpenses()
        {
            return GetContext().Expenses;
        }

        public static List<Expenses> GetAllExpensesAsList()
        {
            return GetContext().Expenses.ToList();
        }

        public static List<TenantExpenses> GetAllExpensesByTenantAndMonth(int tenantId, int year, int month)
        {
            var te = GetContext().TenantExpenses.Where(e => e.Id_Tenant == tenantId 
                && e.EstateExpenses.Month == month && e.EstateExpenses.Year == year)
                .ToListAsync().Result;

            return te;
        }

        public static TenantExpenses GetExpenseByTenantMonth(int tenantId, int year, int month, object expense)
        {
            var allTenantExpenses = GetAllExpensesByTenantAndMonth(tenantId, year, month);
            var specificExpense = allTenantExpenses.Where(e => e.EstateExpenses.Id_Expense == ((Expenses)expense).Id).ToList();
            TenantExpenses result = null;

            if (specificExpense != null && specificExpense.Count == 1)
            {
                result = specificExpense[0];
            }
            else
            {
                // log error / cleanup
            }

            return result;
        }

        public static Expenses GetExpenseByName(string expenseName)
        {
            var allTenantExpenses = GetAllExpenses();
            var expense = allTenantExpenses.Where(e => e.Name == expenseName).ToList();
            if (expense.Count() > 0)
            {
                return expense[0];
            }
            else
            {
                //// log error
                return null;
            }
        }

        public static void UpdateTenantExpense(int tenantId, int year, int month, object te)
        {
            TenantExpenses result = new TenantExpenses();
            result = GetContext().TenantExpenses.First(b => b.Id == ((TenantExpenses)te).Id);

            if (result != null)
            {
                result.Value = ((TenantExpenses)te).Value;
                GetContext().Entry(result).CurrentValues.SetValues(result);

                GetContext().SaveChanges();
            }
        }

        public static void AddDefaultTenantExpense(int tenantId, int year, int month, int estateExpenseId)
        {
            var estateExpense = GetContext().EstateExpenses.FirstOrDefault(ee => ee.Id == estateExpenseId);

            if (estateExpense != null)
            {
                decimal? value = null; 
                decimal? oldIndex = null;
                if(estateExpense.ExpenseTypes.Id == (int)ExpenseType.PerIndex)
                {
                    var te = TenantExpensesManager.GetForIndexExpensPreviousMonthTenantExpense(estateExpenseId, tenantId);
                    oldIndex = (te!=null)?te.IndexNew:0;
                    value = 100;
                }

                TenantExpenses tenantExpense = new TenantExpenses
                {
                    Value = value,
                    Id_Tenant = tenantId,
                    Id_EstateExpense = estateExpenseId,
                    IndexOld = oldIndex
                };

                GetContext().TenantExpenses.Add(tenantExpense);
                GetContext().SaveChanges();
            }
        }

        public static void AddDefaultTenantExpense(Tenants tenant, int year, int month)
        {
            List<EstateExpenses> estateExpenses = GetContext().EstateExpenses.Where(ee => ee.Id_Estate == tenant.id_Estate
                && ee.Month == month && ee.Year == year).ToList();
                EstateExpenses estateExpense;
                if (estateExpenses != null && estateExpenses.Count > 0)
                {
                    estateExpense = estateExpenses[0];

                    TenantExpenses tenantExpense = new TenantExpenses
                    {
                        Value = null,
                        Id_Tenant = tenant.Id,
                        Id_EstateExpense = estateExpense.Id
                    };

                    GetContext().TenantExpenses.Add(tenantExpense);
                    GetContext().SaveChanges();
                }
        }

        public static void AddTenantExpense(int tenantId, int year, int month, object theExpense, decimal expenseValue)
        {
            List<Tenants> tenants = GetContext().Tenants.Where(t => t.Id == tenantId).ToList();
            Expenses expense = (Expenses)theExpense;
            Tenants tenant;
            if (tenants != null && tenants.Count == 1)
            {
                tenant = tenants[0];
                List<EstateExpenses> estateExpenses = GetContext().EstateExpenses.Where(ee => ee.Id_Estate == tenant.id_Estate 
                    && ee.Id_Expense == expense.Id && ee.Month == month && ee.Year == year).ToList();
                EstateExpenses estateExpense;
                if (estateExpenses != null && estateExpenses.Count == 1)
                {
                    estateExpense = estateExpenses[0];
                    TenantExpenses tenantExpense = new TenantExpenses
                    {
                        Value = expenseValue,
                        Id_Tenant = tenantId,
                        Id_EstateExpense = estateExpense.Id
                    };

                    GetContext().TenantExpenses.Add(tenantExpense);
                    GetContext().SaveChanges();
                }
            }
        }

        public static Expenses GetById(int expenseId)
        {
            return GetContext().Expenses.FirstOrDefault(e => e.Id == expenseId);
        }
    }
}
