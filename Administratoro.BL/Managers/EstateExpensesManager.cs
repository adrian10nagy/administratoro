
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

        public static List<EstateExpenses> GetAllEstateExpensesByMonthAndYearNotDisabled(int associationId, int year, int month)
        {
            return GetContext(true).EstateExpenses.Where(
                ee => ee.Id_Estate == associationId &&
                ee.Year == year && ee.Month == month &&
                !ee.WasDisabled && !ee.Expenses.specialType.HasValue).ToList();
        }

        public static List<EstateExpenses> GetForAddPage(int associationId, int year, int month)
        {
            return GetContext(true).EstateExpenses.Where(
                ee => ee.Id_Estate == associationId && ee.Year == year && ee.Month == month &&
                !ee.WasDisabled && !ee.Expenses.specialType.HasValue && ee.Expenses.Id != 25).ToList();
        }

        public static List<EstateExpenses> GetAllEstateExpensesByMonthAndYearwithDiverse(int associationId, int year, int month)
        {
            return GetContext(true).EstateExpenses.Where(
                ee => ee.Id_Estate == associationId &&
                ee.Year == year && ee.Month == month &&
                !ee.WasDisabled).ToList();
        }

        public static List<EstateExpenses> GetFromLastesOpenedMonth(int associationId, bool shouldRefresh = false)
        {
            if (GetContext(shouldRefresh).EstateExpenses.Count() > 0)
            {
                int? maxYear = GetContext(shouldRefresh).EstateExpenses.Max(i => i.Year);
                if (maxYear.HasValue)
                {
                    List<EstateExpenses> eeFromMaxYear = GetContext(shouldRefresh).EstateExpenses.Where(c => c.Year == maxYear && c.Id_Estate == associationId).ToList();

                    if (eeFromMaxYear != null && eeFromMaxYear.Count > 0)
                    {
                        int maxMonth = eeFromMaxYear.Max(i => i.Month);

                        return GetContext(shouldRefresh).EstateExpenses.Where(ee => ee.Month == maxMonth && ee.Year == maxYear &&
                            ee.Id_Estate == associationId && !ee.WasDisabled && !ee.Expenses.specialType.HasValue).ToList();
                    }
                    else
                    {
                        return GetDefault(associationId);
                    }
                }
            }

            return GetDefault(associationId);

        }

        public static List<EstateExpenses> GetDefault(int associationId)
        {
            return GetContext().EstateExpenses.Where(ee => ee.Id_Estate == associationId && ee.isDefault && !ee.WasDisabled && !ee.Expenses.specialType.HasValue).ToList();
        }

        public static List<EstateExpenses> GetAllEstateExpensesByMonthAndYearIncludingDisabled(int associationId, int year, int month)
        {
            return GetContext().EstateExpenses.Where(
                ee => ee.Id_Estate == associationId &&
                ee.Year == year && ee.Month == month && !ee.Expenses.specialType.HasValue).ToList();
        }

        public static EstateExpenses GetEstateExpensesByMonthAndYearAndDisabled(int associationId, int expenseId, int year, int month, bool wasDisabled = true)
        {
            return GetContext().EstateExpenses.FirstOrDefault(
                ee => ee.Id_Estate == associationId &&
                ee.Year == year && ee.Month == month &&
                ee.Id_Expense == expenseId &&
                ee.WasDisabled == wasDisabled);
        }

        public static EstateExpenses GetEstateExpense(int associationId, int expenseId, int year, int month)
        {
            return GetContext(true).EstateExpenses.FirstOrDefault(
                ee => ee.Id_Estate == associationId &&
                ee.Year == year && ee.Month == month &&
                ee.Id_Expense == expenseId && !ee.Expenses.specialType.HasValue);
        }

        public static EstateExpenses GetById(int idExpenseEstate)
        {
            return GetContext(true).EstateExpenses.Where(
                ee => ee.Id == idExpenseEstate).First();
        }

        public static EstateExpenses Add(int associationId, int expenseId, int month, int year, string expenseType, bool? isStairCaseSplit)
        {
            EstateExpenses ee = null;

            ExpenseType expenseTypeEnum;
            if (Enum.TryParse<ExpenseType>(expenseType, out  expenseTypeEnum))
            {
                ee = new EstateExpenses
                {
                    Id_Expense = expenseId,
                    Id_ExpenseType = (int)expenseTypeEnum,
                    Id_Estate = associationId,
                    Month = month,
                    Year = year,
                    isDefault = false,
                    WasDisabled = false,
                    SplitPerStairCase = isStairCaseSplit
                };

                GetContext().EstateExpenses.Add(ee);
                GetContext().SaveChanges();
            }

            return ee;
        }

        public static void AddEstateExpensesByTenantAndMonth(int associationId, Dictionary<int, int> expenses)
        {

            foreach (var expense in expenses)
            {
                EstateExpenses ee = null;
                ee = new EstateExpenses
                {
                    Id_Expense = expense.Key,
                    Id_ExpenseType = expense.Value,
                    Id_Estate = associationId,
                    Year = -1,
                    Month = -1,
                    isDefault = true,
                    WasDisabled = false
                };

                GetContext().EstateExpenses.Add(ee);
            }

            GetContext().SaveChanges();
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
            var estateExpense = GetContext().EstateExpenses.FirstOrDefault(ee => ee.Id == idExpenseEstate && !ee.Expenses.specialType.HasValue);
            if (estateExpense != null)
            {
                estateExpense.PricePerExpenseUnit = newPricePerUnit;
                GetContext().SaveChanges();

                TenantExpensesManager.UpdateValueForPriceUpdate(estateExpense.Id, newPricePerUnit);
            }
        }

        public static List<YearMonth> GetAllMonthsAndYearsAvailableByAssociationId(int assotiationId)
        {
            return GetContext().EstateExpenses.Where(ee => ee.Id_Estate == assotiationId &&
                ee.Year != -1 && ee.Month != -1 && !ee.Expenses.specialType.HasValue)
                .Select(s => new YearMonth { Year = s.Year, Month = s.Month }).Distinct().OrderBy(ee => ee.Year).ToList();
        }

        public static List<YearMonth> GetAllMonthsAndYearsNotClosedByAssociationId(int assotiationId)
        {
            return GetContext().EstateExpenses.Where(ee => ee.Id_Estate == assotiationId &&
                ee.Year != -1 && ee.Month != -1 && !ee.Expenses.specialType.HasValue && !ee.IsClosed.HasValue)
                .Select(s => new YearMonth { Year = s.Year, Month = s.Month }).Distinct().OrderBy(ee => ee.Year).ToList();
        }

        public static void UpdatePricePerUnitDefaultPrevieousMonth(EstateExpenses newEE, List<EstateExpenses> oldEEs)
        {
            if (newEE != null)
            {
                EstateExpenses oldEE = oldEEs.FirstOrDefault(ee => ee.Id_Expense == newEE.Id_Expense && !ee.Expenses.specialType.HasValue);
                if (oldEE != null && oldEE.Id_ExpenseType == (int)ExpenseType.PerIndex)
                {
                    UpdatePricePerUnit(newEE.Id, oldEE.PricePerExpenseUnit);
                    UpdateRedistributeMethod(newEE.Id, oldEE.RedistributeType);
                }
            }
        }

        public static void UpdateRedistributeMethod(int estateExpenseId, int? type)
        {
            EstateExpenses result = new EstateExpenses();
            result = GetContext(true).EstateExpenses.FirstOrDefault(ee => ee.Id == estateExpenseId && !ee.Expenses.specialType.HasValue);
            if (result != null)
            {
                result.RedistributeType = type;
                GetContext().Entry(result).CurrentValues.SetValues(result);
                GetContext().SaveChanges();
            }
        }

        public static EstateExpenses GetMonthYearAssoiationExpense(int associationId, int expenseId, int year, int month)
        {
            return GetContext(true).EstateExpenses.FirstOrDefault(ee => ee.Id_Estate == associationId && ee.Id_Expense == expenseId
                && ee.Year == year && ee.Month == month);
        }

        #region statusOfinvoiceFor Split -NoSplit

        private static string StatusOfInvoicesForSplit(EstateExpenses estateExpense, string result, string redistributeValue, string percentage)
        {
            if (estateExpense.ExpenseTypes.Id == (int)ExpenseType.PerIndex)
            {
                if (StatusAllInvoicesHaveValue(estateExpense, 1) && StatusHasAddedAllExpenses(percentage) &&
                    StatusHasRedistributedTheValue(estateExpense, redistributeValue))
                {
                    result = "<i class='fa fa-check'></i> 100%";
                }
                else if (!StatusHasAddedAllExpenses(percentage) && !StatusAllInvoicesHaveValue(estateExpense, 1))
                {
                    result = "Adaugă facturile, cheltuielile individuale! 0%";
                }
                else if (!StatusHasAddedAllExpenses(percentage))
                {
                    result = "Cheltuieli neadăugate! 20%";
                }
                else if (!StatusAllInvoicesHaveValue(estateExpense, 1))
                {
                    result = "Facturi neadăugate! 50%";
                }
                else if (!StatusHasRedistributedTheValue(estateExpense, redistributeValue))
                {
                    result = "Redistribuie cheltuiala! 80%";
                }
            }
            else
            {
                if (StatusAllInvoicesHaveValue(estateExpense, 1))
                {
                    result = "<i class='fa fa-check'></i> 100%";
                }
                else
                {
                    result = "Facturi neadăugate! 0%";
                }
            }

            return result;
        }

        private static bool StatusAllInvoicesHaveValue(EstateExpenses estateExpense, int numberOfInvoices)
        {
            return estateExpense.Invoices.All(i => i.Value.HasValue) && estateExpense.Invoices.Count == numberOfInvoices;
        }

        private static bool StatusHasRedistributedTheValue(EstateExpenses estateExpense, string redistributeValue)
        {
            bool result = false;

            if (estateExpense.RedistributeType.HasValue)
            {
                result = true;
            }
            else if (string.IsNullOrEmpty(redistributeValue) || redistributeValue == "0,0000000")
            {
                result = true;
            }

            return result;
        }

        private static bool StatusHasAddedAllExpenses(string percentage)
        {
            return (string.IsNullOrEmpty(percentage) || percentage == "100");
        }

        private static string StatusOfInvoicesForNoSplit(string result, string redistributeValue, string percentage, EstateExpenses estateExpense)
        {
            if (estateExpense.ExpenseTypes.Id == (int)ExpenseType.PerIndex)
            {
                if (estateExpense != null && estateExpense.Invoices.All(i => i.Value.HasValue) && estateExpense.Invoices.Count > 0 && (string.IsNullOrEmpty(percentage) || percentage == "100") &&
                    (estateExpense.RedistributeType.HasValue || string.IsNullOrEmpty(redistributeValue) || redistributeValue == "0,00"))
                {
                    result = "<i class='fa fa-check'></i> 100%";
                }
                else if (!estateExpense.RedistributeType.HasValue && estateExpense.Invoices.Any(i => !i.Value.HasValue) && percentage == "0")
                {
                    result = "Adaugă factura, cheltuielile! 0%";
                }
                else if ((string.IsNullOrEmpty(percentage) || percentage != "100"))
                {
                    result = "Cheltuieli neadăugate! 20%";
                }
                else if (estateExpense.Invoices.Any(i => !i.Value.HasValue) || estateExpense.Invoices.Count == 0)
                {
                    result = "Facturi neadăugate! 50%";
                }
                else if (!estateExpense.RedistributeType.HasValue)
                {
                    result = "Redistribuie cheltuiala! 80%";
                }
            }
            else if (estateExpense.ExpenseTypes.Id == (int)ExpenseType.Individual)
            {
                if (estateExpense.TenantExpenses.Any(te => !te.Value.HasValue) || estateExpense.Estates.Tenants.Where(t => t.HasHeatHelp.HasValue).Count() != estateExpense.TenantExpenses.Count())
                {
                    result = "Cheltuieli neadăugate! 0%";
                }
                else
                {
                    result = "<i class='fa fa-check'></i> 100%";
                }
            }
            else
            {
                if (estateExpense != null && estateExpense.Invoices.All(i => i.Value.HasValue) && estateExpense.Invoices.Count > 0)
                {
                    result = "<i class='fa fa-check'></i> 100%";
                }
                else if (estateExpense.Invoices.Any(i => !i.Value.HasValue) || estateExpense.Invoices.Count == 0)
                {
                    result = "Facturi neadăugate! 50%";
                }
                else if (!estateExpense.RedistributeType.HasValue)
                {
                    result = "Redistribuie cheltuiala! 80%";
                }
            }

            return result;
        }

        public static ExpensesCompletedStatus StatusOfInvoicesForNoSplit2(string result, string redistributeValue, string percentage, EstateExpenses estateExpense)
        {
            return ExpensesCompletedStatus.All;
        }

        public static string StatusOfInvoices(EstateExpenses estateExpense, bool isExpensePerIndex)
        {
            string result = string.Empty;
            var redistributeValue = RedistributionManager.CalculateRedistributeValueAsString(estateExpense.Id);
            var percentage = string.Empty;

            if (isExpensePerIndex)
            {
                percentage = GetPercentageAsString(estateExpense);
            }

            //if (estateExpense.SplitPerStairCase.HasValue && estateExpense.SplitPerStairCase.Value)
            //{
                result = StatusOfInvoicesForSplit(estateExpense, result, redistributeValue, percentage);
            //}
            //else
            //{
            //    result = StatusOfInvoicesForNoSplit(result, redistributeValue, percentage, estateExpense);
            //}

            return result;
        }

        public static string GetPercentageAsString(EstateExpenses estateExpense)
        {
            string percentage = string.Empty;

            int tenantsWithCountersOfThatExpense = GetTenantsWithCountersOfThatExpense(estateExpense);

            if (tenantsWithCountersOfThatExpense > 0)
            {
                percentage = ExpensePercentageFilledInAsString(estateExpense, tenantsWithCountersOfThatExpense);
            }
            else
            {
                percentage = "100";
            }

            return percentage;
        }

        public static decimal GetPercentage(EstateExpenses estateExpense)
        {
            decimal percentage = 0.0m;

            int tenantsWithCountersOfThatExpense = GetTenantsWithCountersOfThatExpense(estateExpense);

            if (tenantsWithCountersOfThatExpense > 0)
            {
                percentage = ExpensePercentageFilledIn(estateExpense, tenantsWithCountersOfThatExpense);
            }
            else
            {
                percentage = 100m;
            }

            return percentage;
        }

        private static string ExpensePercentageFilledInAsString(EstateExpenses estateExpense, int tenantsWithCounters)
        {
            var addedExpenses = estateExpense.TenantExpenses.Count(te => te.IndexNew.HasValue);
            var percentage = (((decimal)addedExpenses / (decimal)tenantsWithCounters) * 100).ToString("0.##");

            return percentage;
        }

        private static decimal ExpensePercentageFilledIn(EstateExpenses estateExpense, int tenantsWithCounters)
        {
            var addedExpenses = estateExpense.TenantExpenses.Count(te => te.IndexNew.HasValue);
            var percentage = (((decimal)addedExpenses / (decimal)tenantsWithCounters) * 100);

            return percentage;
        }

        public static string ExpensePercentageFilledInMessage(EstateExpenses estateExpense)
        {
            var addedExpenses = estateExpense.TenantExpenses.Count(te => te.IndexNew.HasValue);
            int tenantsWithCountersOfThatExpense = GetTenantsWithCountersOfThatExpense(estateExpense);

            return "<b>" + addedExpenses + "</b> cheltuieli adăugate din <b>" + tenantsWithCountersOfThatExpense + "</b> ";
        }

        private static int GetTenantsWithCountersOfThatExpense(EstateExpenses estateExpense)
        {
            int tenantsWithCountersOfThatExpense = 0;
            List<Counters> allcountersOfExpense = CountersManager.GetAllByExpenseType(estateExpense.Estates.Id, estateExpense.Expenses.Id);
            foreach (var tenant in estateExpense.Estates.Tenants)
            {
                if (allcountersOfExpense.Select(c => c.Id).Intersect(tenant.ApartmentCounters.Select(ac => ac.Id_Counters)).Any())
                {
                    tenantsWithCountersOfThatExpense++;
                }
            }
            return tenantsWithCountersOfThatExpense;
        }

        #endregion


        public static void CloseMonth(int association, int year, int month, bool shouldClose = true)
        {
            List<EstateExpenses> allEE = GetContext(true).EstateExpenses.Where(ee => ee.Id_Estate == association &&
                ee.Year == year && ee.Month == month).ToList();
            foreach (var estateExpense in allEE)
            {
                estateExpense.IsClosed = shouldClose;
                GetContext().Entry(estateExpense).CurrentValues.SetValues(estateExpense);
                GetContext().SaveChanges();
            }
        }


        public static List<Expense> CheckCloseMonth(int association, int year, int month)
        {
            List<Expense> result = new List<Expense>();

            List<EstateExpenses> allEE = GetContext(true).EstateExpenses.Where(ee => ee.Id_Estate == association &&
                ee.Year == year && ee.Month == month).ToList();
            foreach (var estateExpense in allEE)
            {
                if (estateExpense.SplitPerStairCase.HasValue && estateExpense.SplitPerStairCase.Value)
                {
                    if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerTenants || estateExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
                    {
                        if (estateExpense.Invoices.Count != estateExpense.Estates.StairCases.Count || estateExpense.Invoices.Any(i => !i.Value.HasValue))
                        {
                            result.Add((Expense)estateExpense.Id_Expense);
                        }
                    }
                    else if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerIndex)
                    {
                        var percentage = GetPercentage(estateExpense);
                        if (percentage != 100m)
                        {
                            result.Add((Expense)estateExpense.Id_Expense);
                        }
                    }
                }
                else
                {
                    if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerTenants || estateExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
                    {
                        if (estateExpense.Invoices.Any(i => !i.Value.HasValue) && estateExpense.Invoices.Count != 0)
                        {
                            result.Add((Expense)estateExpense.Id_Expense);
                        }
                    }
                    else if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerIndex)
                    {
                        var percentage = GetPercentage(estateExpense);
                        if (percentage != 100m)
                        {
                            result.Add((Expense)estateExpense.Id_Expense);
                        }
                    }
                    else if (estateExpense.Id_ExpenseType == (int)ExpenseType.Individual)
                    {
                        if (estateExpense.TenantExpenses.Any(te => !te.Value.HasValue))
                        {
                            result.Add((Expense)estateExpense.Id_Expense);
                        }
                    }
                }
            }

            return result;
        }

        public static bool IsMonthClosed(int association, int year, int month)
        {
            List<EstateExpenses> allEE = GetContext(true).EstateExpenses.Where(ee => ee.Id_Estate == association &&
                ee.Year == year && ee.Month == month).ToList();

            return allEE.All(e => e.IsClosed.HasValue && e.IsClosed.Value);
        }

        public static List<Tenants> GetApartmentsNrThatShouldRedistributeTo(int estateExpenseId)
        {
            List<Tenants> result = new List<Tenants>();

            var estateExpense = EstateExpensesManager.GetById(estateExpenseId);
            if (estateExpense != null)
            {
                result = estateExpense.TenantExpenses.Where(te => te.Tenants.ApartmentCounters.Any(ac => ac.Counters.Id_Expense == estateExpense.Id_Expense)).Select(te => te.Tenants).ToList();
            }

            return result;
        }

        public static List<Tenants> GetApartmentsNrThatShouldRedistributeTo(int estateExpenseId, int? stairCaseId)
        {
            List<Tenants> result = new List<Tenants>();

            var estateExpense = EstateExpensesManager.GetById(estateExpenseId);
            if (estateExpense != null)
            {
                result = estateExpense.TenantExpenses.Where(te => te.Tenants.ApartmentCounters.Any(ac => ac.Counters.Id_Expense == estateExpense.Id_Expense)
                    && te.Tenants.Id_StairCase == stairCaseId).Select(te => te.Tenants).ToList();
            }

            return result;
        }

        public static bool HasCounterOfExpense(int tenantId, int expenseId)
        {
            var counters = CountersManager.GetByApartment(tenantId);
            return counters.Any(c => c.Id_Expense == expenseId);
        }
    }
}
