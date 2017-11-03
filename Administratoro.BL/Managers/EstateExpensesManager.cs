
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
            var fromMaxYear = GetContext().EstateExpenses.Where(c => c.Year == maxYear && c.Id_Estate == estateId);
            int maxMonth = fromMaxYear.Max(i => i.Month);

            return GetContext().EstateExpenses.Where(c => c.Month == maxMonth && c.Id_Estate == estateId && !c.WasDisabled).ToList();
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
            return GetContext().EstateExpenses.Where(
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

        public static void RemoveEstateExpensesByTenantAndMonth(int expenseId, int estateId, int month, int year)
        {
            var exEstate = GetContext().EstateExpenses.Where(ee => ee.Id_Expense == expenseId && ee.Id_Estate == estateId).First();
            if (exEstate != null)
            {
                GetContext().EstateExpenses.Remove(exEstate);
                GetContext().SaveChanges();
            }
        }

        public static void MarkEstateExpensesDisableProperty(EstateExpenses ee, bool isDisabled)
        {
            //EstateExpenses result = new EstateExpenses();
            //result = GetContext().EstateExpenses.Where(e1 => e1.Id_Expense == e1.Id_Expense && e1.Id_Expense == ee.Id_Estate
            //    && e1.isDefault == ee.isDefault && e1.Year == ee.Year && e1.Month == ee.Month).First();

            if (ee != null)
            {
                ee.WasDisabled = isDisabled;
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
    }
}
