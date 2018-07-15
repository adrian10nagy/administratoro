
namespace Administratoro.BL.Managers
{
    using DAL;
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

        #endregion

        #region update

        public static void Update(int associationExpensesUnitPricesId, decimal? newPrice)
        {
            AssociationExpensesUnitPrices result;
            result = GetContext().AssociationExpensesUnitPrices.FirstOrDefault(b => b.Id == associationExpensesUnitPricesId);

            if (result != null)
            {
                result.PricePerExpenseUnit = newPrice;
                GetContext().Entry(result).CurrentValues.SetValues(result);

                GetContext().SaveChanges();
            }
        }

        public static void AddOrUpdate(int? stairCase, int assExpenseId, decimal? newPrice)
        {
            var associationExpense = AssociationExpensesManager.GetById(assExpenseId);

            if (associationExpense != null)
            {
                var assCounter = AssociationCountersManager.GetByExpenseAndStairCase(associationExpense.Id_Estate, associationExpense.Id_Expense, stairCase);
                if (assCounter != null)
                {
                    var assExUnitPrice = Get(assExpenseId, assCounter.Id);
                    if (assExUnitPrice == null)
                    {
                        Add(assExpenseId, assCounter.Id, newPrice);
                    }
                    else
                    {
                        Update(assExUnitPrice.Id, newPrice);
                    }
                }
            }
        }

        private static void Add(int assExpenseId, int assCounterId, decimal? newPrice)
        {
            AssociationExpensesUnitPrices associationExpensesUnitPrices = new AssociationExpensesUnitPrices
            {
                Id_assCounter = assCounterId,
                Id_assExpense = assExpenseId,
                PricePerExpenseUnit = newPrice
            };

            GetContext().AssociationExpensesUnitPrices.Add(associationExpensesUnitPrices);
            GetContext().SaveChanges();
        }

        #endregion
    }
}
