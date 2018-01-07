
namespace Administratoro.BL.Managers
{
    using Administratoro.BL.Constants;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class CashBookManager
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

        public static CashBooks GetByEstateExpenseId(int estateExpenseId)
        {
            return GetContext().CashBooks.FirstOrDefault(c => c.Id_EstateExpense == estateExpenseId);
        }

        public static CashBooks GetAllLatestByEstateId(int estateId)
        {
            int maxYear = GetContext().CashBooks.Max(i => i.EstateExpenses.Year);

            var fromMaxYear = GetContext().CashBooks.Where(c => c.EstateExpenses.Year == maxYear && c.EstateExpenses.Id_Estate == estateId);

            int maxMonth = fromMaxYear.Max(i => i.EstateExpenses.Month);

            return GetContext().CashBooks.FirstOrDefault(c => c.EstateExpenses.Month == maxMonth);
        }

        public static void AddDefault(int estateExpenseId)
        {
            CashBooks cashBooks = new CashBooks
            {
                Id_EstateExpense = estateExpenseId
            };

            GetContext().CashBooks.Add(cashBooks);
            GetContext().SaveChanges();
        }

        public static void Update(EstateExpenses estateExpense, decimal? value)
        {
            CashBooks result = new CashBooks();
            result = GetContext(true).CashBooks.FirstOrDefault(c => c.Id_EstateExpense == estateExpense.Id);

            if (result != null)
            {
                result.Value = value;
                GetContext().Entry(result).CurrentValues.SetValues(result);
            }
            else
            {
                result = new CashBooks
                {
                    Id_EstateExpense = estateExpense.Id,
                    Value = value
                };
                GetContext().CashBooks.Add(result);
            }

            UpdateTenantExpenses(estateExpense, value);

            GetContext().SaveChanges();
        }

        private static void UpdateTenantExpenses(EstateExpenses estateExpense, decimal? value)
        {
            if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerIndex)
            {
                // no update needed
            }
            else if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
            {
                TenantExpensesManager.AddCotaIndivizaTenantExpenses(estateExpense, value);
            }
            else if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerTenants)
            {
                var tenants = ApartmentsManager.GetAllByEstateId(estateExpense.Id_Estate);
                var allTenantDependents = tenants.Sum(t => t.Dependents);
                decimal? valuePerTenant = null;
                if (value.HasValue && allTenantDependents != 0)
                {
                    valuePerTenant = Math.Round(value.Value / allTenantDependents, 2);
                }
                TenantExpensesManager.AddPerTenantExpenses(estateExpense.Id, valuePerTenant);
            }
        }

        public static void UpdateRedistributeMethodAndValue(int estateExpenseId, int type)
        {
            CashBooks result = new CashBooks();
            result = GetContext(true).CashBooks.FirstOrDefault(c => c.Id_EstateExpense == estateExpenseId);
            if (result != null)
            {
                result.RedistributeType = type;
                GetContext().Entry(result).CurrentValues.SetValues(result);
                GetContext().SaveChanges();
            }
        }

        public static void Update(EstateExpenses estateExpense, decimal? value, int stairCaseId)
        {
            CashBooks cashbook = new CashBooks();
            Invoices invoice = new Invoices();
            cashbook = GetContext(true).CashBooks.FirstOrDefault(c => c.Id_EstateExpense == estateExpense.Id);

            if (cashbook == null)
            {
                cashbook = new CashBooks
                {
                    Id_EstateExpense = estateExpense.Id
                };
                GetContext().CashBooks.Add(cashbook);
            }

            invoice = GetContext(true).Invoices.FirstOrDefault(c => c.Id_StairCase.ToString() == stairCaseId.ToString() && c.Id_CashBook == cashbook.Id);

            if (invoice != null)
            {
                invoice.Value = value;
                GetContext().Entry(invoice).CurrentValues.SetValues(invoice);
            }
            else
            {
                invoice = new Invoices
                {
                    Id_StairCase = stairCaseId,
                    Value = value,
                    Id_CashBook = cashbook.Id
                };
                GetContext().Invoices.Add(invoice);
            }

            UpdateTenantExpenses(estateExpense, value);

            GetContext().SaveChanges();
        }
    }
}
