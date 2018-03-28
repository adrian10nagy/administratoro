

namespace Administratoro.BL.Managers
{
    using Administratoro.BL.Constants;
    using Administratoro.DAL;
    using System.Linq;
    using System;

    public static class RecalculationManager
    {
        public static void RecalculateMonthlyExpenses(int associationId, int year, int month)
        {
            var estateExpenses = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearNotDisabled(associationId, year, month);

            foreach (var estateExpense in estateExpenses)
            {
                RelalculateExpense(estateExpense);
            }
        }

        private static void RelalculateExpense(EstateExpenses item)
        {
            if (item.Id_ExpenseType == (int)ExpenseType.PerTenants || item.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
            {
                if(item.Invoices.Count == 1)
                {
                    var invoice = item.Invoices.FirstOrDefault();
                    if(invoice!=null && invoice.Value.HasValue)
                    {
                        TenantExpensesManager.UpdateTenantExpenses(invoice.EstateExpenses, invoice.Value.Value, null);
                    }
                }
            }
            else if (item.Id_ExpenseType == (int)ExpenseType.PerIndex)
            {
                //redistribute
            }
        }
    }
}
