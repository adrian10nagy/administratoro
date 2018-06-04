

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
            var associationExpenses = AssociationExpensesManager.GetByMonthAndYearNotDisabled(associationId, year, month);

            foreach (var associationExpense in associationExpenses)
            {
                RelalculateExpense(associationExpense);
            }
        }

        private static void RelalculateExpense(AssociationExpenses item)
        {
            if (item.Id_ExpenseType == (int)ExpenseType.PerNrTenants || item.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
            {
                if(item.Invoices.Count == 1)
                {
                    var invoice = item.Invoices.FirstOrDefault();
                    if(invoice!=null && invoice.Value.HasValue)
                    {
                        ApartmentExpensesManager.UpdateApartmentExpenses(invoice.AssociationExpenses, invoice.Value.Value, null);
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
