

namespace Administratoro.BL.Managers
{
    using Constants;
    using DAL;
    using System.Linq;

    public static class RecalculationManager
    {
        public static void RecalculateMonthlyExpenses(int associationId, int year, int month)
        {
            var associationExpenses = AssociationExpensesManager.GetByMonthAndYearWithoutDiverse(associationId, year, month);

            foreach (var associationExpense in associationExpenses)
            {
                RelalculateExpense(associationExpense);
            }
        }

        private static void RelalculateExpense(AssociationExpenses item)
        {
            switch (item.Id_ExpenseType)
            {
                case (int)ExpenseType.PerNrTenants:
                case (int)ExpenseType.PerCotaIndiviza:
                    if (item.Invoices.Count != 1) return;
                    var invoice = item.Invoices.FirstOrDefault();
                    if(invoice!=null && invoice.Value.HasValue)
                    {
                        ApartmentExpensesManager.UpdateApartmentExpenses(invoice.AssociationExpenses, invoice.Value.Value, null);
                    }

                    break;
                case (int)ExpenseType.PerIndex:
                    //redistribute
                    break;
            }
        }
    }
}
