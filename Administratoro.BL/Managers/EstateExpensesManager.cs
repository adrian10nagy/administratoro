
namespace Administratoro.BL.Managers
{
    using Administrataro.BL.Models;
    using Administratoro.BL.Constants;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class EstateExpensesManager
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

        public static List<EstateExpenses> GetAllEstateExpenses(int estateId)
        {
            return GetContext().EstateExpenses.Where(ee => ee.Id_Estate == estateId && ee.WasDisabled == false
                && ee.isDefault == false).ToList();
        }

        public static List<EstateExpenses> GetAllEstateExpensesByMonthAndYearNotDisabled(int estateId, int year, int month)
        {
            return GetContext(true).EstateExpenses.Where(
                ee => ee.Id_Estate == estateId &&
                ee.Year == year && ee.Month == month &&
                !ee.WasDisabled).ToList();
        }

        public static List<EstateExpenses> GetFromLastesOpenedMonth(int estateId)
        {
            int maxYear = GetContext().EstateExpenses.Max(i => i.Year);
            var fromMaxYear = GetContext().EstateExpenses.Where(c => c.Year == maxYear && c.Id_Estate == estateId).ToList();

            if (fromMaxYear != null && fromMaxYear.Count > 0)
            {
                int maxMonth = fromMaxYear.Max(i => i.Month);

                return GetContext().EstateExpenses.Where(c => c.Month == maxMonth && c.Id_Estate == estateId && !c.WasDisabled).ToList();
            }
            else
            {
                return GetDefault(estateId);
            }
        }

        public static List<EstateExpenses> GetDefault(int estateId)
        {
            return GetContext().EstateExpenses.Where(c => c.Id_Estate == estateId && c.isDefault && !c.WasDisabled).ToList();
        }

        public static List<EstateExpenses> GetAllEstateExpensesByMonthAndYearIncludingDisabled(int estateId, int year, int month)
        {
            return GetContext().EstateExpenses.Where(
                ee => ee.Id_Estate == estateId &&
                ee.Year == year && ee.Month == month).ToList();
        }

        public static EstateExpenses GetEstateExpensesByMonthAndYearAndDisabled(int estateId, int expenseId, int year, int month, bool wasDisabled = true)
        {
            return GetContext().EstateExpenses.FirstOrDefault(
                ee => ee.Id_Estate == estateId &&
                ee.Year == year && ee.Month == month &&
                ee.Id_Expense == expenseId &&
                ee.WasDisabled == wasDisabled);
        }

        public static EstateExpenses GetEstateExpenses(int estateId, int expenseId, int year, int month)
        {
            return GetContext(true).EstateExpenses.FirstOrDefault(
                ee => ee.Id_Estate == estateId &&
                ee.Year == year && ee.Month == month &&
                ee.Id_Expense == expenseId);
        }

        public static EstateExpenses GetById(int idExpenseEstate)
        {
            return GetContext(true).EstateExpenses.Where(
                ee => ee.Id == idExpenseEstate).First();
        }

        public static EstateExpenses AddEstateExpensesByTenantAndMonth(int estateId, int expenseId, int month, int year, string expenseType)
        {
            EstateExpenses ee = null;

            ExpenseType expenseTypeEnum;
            if (Enum.TryParse<ExpenseType>(expenseType, out  expenseTypeEnum))
            {
                ee = new EstateExpenses
                {
                    Id_Expense = expenseId,
                    Id_ExpenseType = (int)expenseTypeEnum,
                    Id_Estate = estateId,
                    Month = month,
                    Year = year,
                    isDefault = false,
                    WasDisabled = false
                };

                GetContext().EstateExpenses.Add(ee);
                GetContext().SaveChanges();
            }

            return ee;
        }

        public static void AddEstateExpensesByTenantAndMonth(int estateId, Dictionary<int, int> expenses)
        {

            foreach (var expense in expenses)
            {
                EstateExpenses ee = null;
                ee = new EstateExpenses
                {
                    Id_Expense = expense.Key,
                    Id_ExpenseType = expense.Value,
                    Id_Estate = estateId,
                    Year = -1,
                    Month = -1,
                    isDefault = true,
                    WasDisabled = false
                };

                GetContext().EstateExpenses.Add(ee);
            }

            GetContext().SaveChanges();
        }

        public static void RemoveEstateExpensesByTenantAndMonth(int expenseId, int estateId, int month, int year)
        {
            var exEstate = GetContext().EstateExpenses.Where(ee => ee.Id_Expense == expenseId && ee.Id_Estate == estateId).First();
            if (exEstate != null)
            {
                GetContext().EstateExpenses.Remove(exEstate);
                GetContext().SaveChanges();
            }
        }

        public static void MarkEstateExpensesDisableProperty(EstateExpenses ee, bool isDisabled, bool? isStairCaseSplit)
        {
            if (ee != null)
            {
                ee.WasDisabled = isDisabled;
                ee.SplitPerStairCase = isStairCaseSplit;
                GetContext().SaveChanges();
            }
        }

        public static void UpdateEstateExpenseType(EstateExpenses ee, ExpenseType selectedExpenseType)
        {
            var et = GetContext().ExpenseTypes.FirstOrDefault(ext => ext.Id == (int)selectedExpenseType);
            if (ee != null && et != null)
            {
                ee.ExpenseTypes = et;
                GetContext().SaveChanges();
            }
        }

        public static void UpdatePricePerUnit(int idExpenseEstate, decimal? newPricePerUnit)
        {
            var e = GetContext().EstateExpenses.FirstOrDefault(ee => ee.Id == idExpenseEstate);
            if (e != null)
            {
                e.PricePerExpenseUnit = newPricePerUnit;
                GetContext().SaveChanges();
            }
        }

        public static List<YearMonth> GetAllMonthsAndYeardAvailableByEstateId(int p)
        {
            return GetContext().EstateExpenses.Where(ee => ee.Id_Estate == p && ee.Year != -1 && ee.Month != -1)
                .Select(s => new YearMonth { Year = s.Year, Month = s.Month }).Distinct().ToList();
        }

        public static void UpdatePricePerUnitDefaultPrevieousMonth(EstateExpenses newEE, List<EstateExpenses> oldEEs)
        {
            if (newEE != null)
            {
                EstateExpenses oldEE = oldEEs.FirstOrDefault(ee => ee.Id_Expense == newEE.Id_Expense);
                if (oldEE != null)
                {
                    UpdatePricePerUnit(newEE.Id, oldEE.PricePerExpenseUnit);
                }
            }
        }

        public static string CalculatePertenantPrice(EstateExpenses estateExpense)
        {
            var result = "";
            int dependents = estateExpense.Estates.Tenants.Sum(s => s.Dependents);
            CashBooks cashbook = estateExpense.CashBooks.FirstOrDefault();

            if (dependents != 0 && cashbook!= null && cashbook.Value.HasValue)
            {
                result = Math.Round(cashbook.Value.Value / dependents, 2).ToString();
            }
            return result;
        }
    }
}
