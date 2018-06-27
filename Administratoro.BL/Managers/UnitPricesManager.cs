
namespace Administratoro.BL.Managers
{
    using Administratoro.BL.Constants;
    using Administratoro.DAL;
    using System.Collections.Generic;
    using System.Linq;

    public static class UnitPricesManager
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

        #region Get
        private static decimal? GetPrice(int associationExpenseId, int assCounterId)
        {
            decimal? result = null;
            var assExpenseUnitPrice = GetContext(true).AssociationExpensesUnitPrices.FirstOrDefault(u => u.Id_assExpense == associationExpenseId && u.Id_assCounter == assCounterId);
            if (assExpenseUnitPrice != null)
            {
                result = assExpenseUnitPrice.PricePerExpenseUnit;

            }

            return result;
        }

        private static AssociationExpensesUnitPrices Get(int associationExpenseId, int assCounterId)
        {
            AssociationExpensesUnitPrices result = null;
            var assExpenseUnitPrice = GetContext(true).AssociationExpensesUnitPrices.FirstOrDefault(u => u.Id_assExpense == associationExpenseId && u.Id_assCounter == assCounterId);
            if (assExpenseUnitPrice != null)
            {
                result = assExpenseUnitPrice;
            }

            return result;
        }

        public static decimal? GetPrice(int associationExpenseId, int? stairCase)
        {
            decimal? result = null;

            var associationExpense = AssociationExpensesManager.GetById(associationExpenseId);

            if (associationExpense != null)
            {
                var assCounter = AssociationCountersManager.GetByExpenseAndStairCase(associationExpense.Id_Estate, associationExpense.Id_Expense, stairCase);
                if (assCounter != null)
                {
                    result = GetPrice(associationExpenseId, assCounter.Id);
                }
            }

            return result;
        }

        public static AssociationExpensesUnitPrices Get(int associationExpenseId, int? stairCase)
        {
            AssociationExpensesUnitPrices result = null;

            var associationExpense = AssociationExpensesManager.GetById(associationExpenseId);

            if (associationExpense != null)
            {
                var assCounter = AssociationCountersManager.GetByExpenseAndStairCase(associationExpense.Id_Estate, associationExpense.Id_Expense, stairCase);
                if (assCounter != null)
                {
                    result = Get(associationExpenseId, assCounter.Id);
                }
            }

            return result;
        }

        private static void Update(int assCounterId, int assExpenseId, int? newPrice)
        {
            AssociationExpensesUnitPrices result = new AssociationExpensesUnitPrices();
            result = GetContext().AssociationExpensesUnitPrices.FirstOrDefault(b => b.Id_assCounter == assCounterId && b.Id_assExpense == assExpenseId);

            if (result != null)
            {
                result.PricePerExpenseUnit = newPrice;
                GetContext().Entry(result).CurrentValues.SetValues(result);

                GetContext().SaveChanges();
            }
        }
        #endregion

        #region update

        public static void Update(int associationExpensesUnitPricesId, decimal? newPrice)
        {
            AssociationExpensesUnitPrices result = new AssociationExpensesUnitPrices();
            result = GetContext().AssociationExpensesUnitPrices.FirstOrDefault(b => b.Id == associationExpensesUnitPricesId);

            if (result != null)
            {
                result.PricePerExpenseUnit = newPrice;
                GetContext().Entry(result).CurrentValues.SetValues(result);

                GetContext().SaveChanges();
            }
        }

        public static void Update(int? stairCase, int assExpenseId, decimal? newPrice)
        {
            var associationExpense = AssociationExpensesManager.GetById(assExpenseId);

            if (associationExpense != null)
            {
                var assCounter = AssociationCountersManager.GetByExpenseAndStairCase(associationExpense.Id_Estate, associationExpense.Id_Expense, stairCase);
                if (assCounter != null)
                {
                    var x = Get(assExpenseId, assCounter.Id);
                    if(x!= null)
                    {
                        Update(x.Id, newPrice);
                    }
                    //Update(assCounter.Id, assExpenseId, newPrice);
                }
            }
        }

        #endregion
    }
}
