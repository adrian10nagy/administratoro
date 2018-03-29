
namespace Administratoro.BL.Managers
{
    using Administrataro.BL.Models;
    using Administratoro.BL.Constants;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class AssociationExpensesManager
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

        public static List<AssociationExpenses> GetAllAssociationsByMonthAndYearNotDisabled(int associationId, int year, int month)
        {
            return GetContext(true).AssociationExpenses.Where(
                ee => ee.Id_Estate == associationId &&
                ee.Year == year && ee.Month == month &&
                !ee.WasDisabled && !ee.Expenses.specialType.HasValue).ToList();
        }

        public static List<AssociationExpenses> GetForAddPage(int associationId, int year, int month)
        {
            return GetContext(true).AssociationExpenses.Where(
                ee => ee.Id_Estate == associationId && ee.Year == year && ee.Month == month &&
                !ee.WasDisabled && !ee.Expenses.specialType.HasValue && ee.Expenses.Id != 25).ToList();
        }

        public static List<AssociationExpenses> GetAllAssociationExpensesByMonthAndYearwithDiverse(int associationId, int year, int month)
        {
            return GetContext(true).AssociationExpenses.Where(
                ee => ee.Id_Estate == associationId &&
                ee.Year == year && ee.Month == month &&
                !ee.WasDisabled).ToList();
        }

        public static List<AssociationExpenses> GetFromLastesOpenedMonth(int associationId, bool shouldRefresh = false)
        {
            if (GetContext(shouldRefresh).AssociationExpenses.Count() > 0)
            {
                int? maxYear = GetContext(shouldRefresh).AssociationExpenses.Max(i => i.Year);
                if (maxYear.HasValue)
                {
                    List<AssociationExpenses> eeFromMaxYear = GetContext(shouldRefresh).AssociationExpenses.Where(c => c.Year == maxYear && c.Id_Estate == associationId).ToList();

                    if (eeFromMaxYear != null && eeFromMaxYear.Count > 0)
                    {
                        int maxMonth = eeFromMaxYear.Max(i => i.Month);

                        return GetContext(shouldRefresh).AssociationExpenses.Where(ee => ee.Month == maxMonth && ee.Year == maxYear &&
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

        public static List<AssociationExpenses> GetDefault(int associationId)
        {
            return GetContext().AssociationExpenses.Where(ee => ee.Id_Estate == associationId && ee.isDefault && !ee.WasDisabled && !ee.Expenses.specialType.HasValue).ToList();
        }

        public static List<AssociationExpenses> GetAllAssociationExpensesByMonthAndYearIncludingDisabled(int associationId, int year, int month)
        {
            return GetContext().AssociationExpenses.Where(
                ee => ee.Id_Estate == associationId &&
                ee.Year == year && ee.Month == month && !ee.Expenses.specialType.HasValue).ToList();
        }

        public static AssociationExpenses GetAssociationExpensesByMonthAndYearAndDisabled(int associationId, int expenseId, int year, int month, bool wasDisabled = true)
        {
            return GetContext().AssociationExpenses.FirstOrDefault(
                ee => ee.Id_Estate == associationId &&
                ee.Year == year && ee.Month == month &&
                ee.Id_Expense == expenseId &&
                ee.WasDisabled == wasDisabled);
        }

        public static AssociationExpenses GetAssociationExpense(int associationId, int expenseId, int year, int month)
        {
            return GetContext(true).AssociationExpenses.FirstOrDefault(
                ee => ee.Id_Estate == associationId &&
                ee.Year == year && ee.Month == month &&
                ee.Id_Expense == expenseId && !ee.Expenses.specialType.HasValue);
        }

        public static AssociationExpenses GetById(int idExpenseEstate)
        {
            return GetContext(true).AssociationExpenses.Where(
                ee => ee.Id == idExpenseEstate).First();
        }

        public static AssociationExpenses Add(int associationId, int expenseId, int month, int year, string expenseType, bool? isStairCaseSplit)
        {
            AssociationExpenses ee = null;

            ExpenseType expenseTypeEnum;
            if (Enum.TryParse<ExpenseType>(expenseType, out  expenseTypeEnum))
            {
                ee = new AssociationExpenses
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

                GetContext().AssociationExpenses.Add(ee);
                GetContext().SaveChanges();
            }

            return ee;
        }

        public static void AddAssociationExpensesByApartmentAndMonth(int associationId, Dictionary<int, int> expenses)
        {

            foreach (var expense in expenses)
            {
                AssociationExpenses ee = null;
                ee = new AssociationExpenses
                {
                    Id_Expense = expense.Key,
                    Id_ExpenseType = expense.Value,
                    Id_Estate = associationId,
                    Year = -1,
                    Month = -1,
                    isDefault = true,
                    WasDisabled = false
                };

                GetContext().AssociationExpenses.Add(ee);
            }

            GetContext().SaveChanges();
        }

        public static void MarkAssociationExpensesDisableProperty(AssociationExpenses ee, bool isDisabled, bool? isStairCaseSplit)
        {
            if (ee != null)
            {
                ee.WasDisabled = isDisabled;
                ee.SplitPerStairCase = isStairCaseSplit;
                GetContext().SaveChanges();
            }
        }

        public static void UpdateAssociationExpenseType(AssociationExpenses ee, ExpenseType selectedExpenseType)
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
            var associationExpense = GetContext().AssociationExpenses.FirstOrDefault(ee => ee.Id == idExpenseEstate && !ee.Expenses.specialType.HasValue);
            if (associationExpense != null)
            {
                associationExpense.PricePerExpenseUnit = newPricePerUnit;
                GetContext().SaveChanges();

                ApartmentExpensesManager.UpdateValueForPriceUpdate(associationExpense.Id, newPricePerUnit);
            }
        }

        public static List<YearMonth> GetAllMonthsAndYearsAvailableByAssociationId(int assotiationId)
        {
            return GetContext().AssociationExpenses.Where(ee => ee.Id_Estate == assotiationId &&
                ee.Year != -1 && ee.Month != -1 && !ee.Expenses.specialType.HasValue)
                .Select(s => new YearMonth { Year = s.Year, Month = s.Month }).Distinct().OrderBy(ee => ee.Year).ToList();
        }

        public static List<YearMonth> GetAllMonthsAndYearsNotClosedByAssociationId(int assotiationId)
        {
            return GetContext().AssociationExpenses.Where(ee => ee.Id_Estate == assotiationId &&
                ee.Year != -1 && ee.Month != -1 && !ee.Expenses.specialType.HasValue && !ee.IsClosed.HasValue)
                .Select(s => new YearMonth { Year = s.Year, Month = s.Month }).Distinct().OrderBy(ee => ee.Year).ToList();
        }

        public static void UpdatePricePerUnitDefaultPrevieousMonth(AssociationExpenses newEE, List<AssociationExpenses> oldEEs)
        {
            if (newEE != null)
            {
                AssociationExpenses oldEE = oldEEs.FirstOrDefault(ee => ee.Id_Expense == newEE.Id_Expense && !ee.Expenses.specialType.HasValue);
                if (oldEE != null && oldEE.Id_ExpenseType == (int)ExpenseType.PerIndex)
                {
                    UpdatePricePerUnit(newEE.Id, oldEE.PricePerExpenseUnit);
                    UpdateRedistributeMethod(newEE.Id, oldEE.RedistributeType);
                }
            }
        }

        public static void UpdateRedistributeMethod(int associationExpenseId, int? type)
        {
            AssociationExpenses result = new AssociationExpenses();
            result = GetContext(true).AssociationExpenses.FirstOrDefault(ee => ee.Id == associationExpenseId && !ee.Expenses.specialType.HasValue);
            if (result != null)
            {
                result.RedistributeType = type;
                GetContext().Entry(result).CurrentValues.SetValues(result);
                GetContext().SaveChanges();
            }
        }

        public static AssociationExpenses GetMonthYearAssoiationExpense(int associationId, int expenseId, int year, int month)
        {
            return GetContext(true).AssociationExpenses.FirstOrDefault(ee => ee.Id_Estate == associationId && ee.Id_Expense == expenseId
                && ee.Year == year && ee.Month == month);
        }

        #region statusOfinvoiceFor Split -NoSplit

        private static string StatusOfInvoicesForSplit(AssociationExpenses associationExpense, string result, string redistributeValue, string percentage)
        {
            if (associationExpense.ExpenseTypes.Id == (int)ExpenseType.PerIndex)
            {
                if (StatusAllInvoicesHaveValue(associationExpense, 1) && StatusHasAddedAllExpenses(percentage) &&
                    StatusHasRedistributedTheValue(associationExpense, redistributeValue))
                {
                    result = "<i class='fa fa-check'></i> 100%";
                }
                else if (!StatusHasAddedAllExpenses(percentage) && !StatusAllInvoicesHaveValue(associationExpense, 1))
                {
                    result = "Adaugă facturile, cheltuielile individuale! 0%";
                }
                else if (!StatusHasAddedAllExpenses(percentage))
                {
                    result = "Cheltuieli neadăugate! 20%";
                }
                else if (!StatusAllInvoicesHaveValue(associationExpense, 1))
                {
                    result = "Facturi neadăugate! 50%";
                }
                else if (!StatusHasRedistributedTheValue(associationExpense, redistributeValue))
                {
                    result = "Redistribuie cheltuiala! 80%";
                }
            }
            else
            {
                if (StatusAllInvoicesHaveValue(associationExpense, 1))
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

        private static bool StatusAllInvoicesHaveValue(AssociationExpenses associationExpense, int numberOfInvoices)
        {
            return associationExpense.Invoices.All(i => i.Value.HasValue) && associationExpense.Invoices.Count == numberOfInvoices;
        }

        private static bool StatusHasRedistributedTheValue(AssociationExpenses associationExpense, string redistributeValue)
        {
            bool result = false;

            if (associationExpense.RedistributeType.HasValue)
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

        private static string StatusOfInvoicesForNoSplit(string result, string redistributeValue, string percentage, AssociationExpenses associationExpense)
        {
            if (associationExpense.ExpenseTypes.Id == (int)ExpenseType.PerIndex)
            {
                if (associationExpense != null && associationExpense.Invoices.All(i => i.Value.HasValue) && associationExpense.Invoices.Count > 0 && (string.IsNullOrEmpty(percentage) || percentage == "100") &&
                    (associationExpense.RedistributeType.HasValue || string.IsNullOrEmpty(redistributeValue) || redistributeValue == "0,00"))
                {
                    result = "<i class='fa fa-check'></i> 100%";
                }
                else if (!associationExpense.RedistributeType.HasValue && associationExpense.Invoices.Any(i => !i.Value.HasValue) && percentage == "0")
                {
                    result = "Adaugă factura, cheltuielile! 0%";
                }
                else if ((string.IsNullOrEmpty(percentage) || percentage != "100"))
                {
                    result = "Cheltuieli neadăugate! 20%";
                }
                else if (associationExpense.Invoices.Any(i => !i.Value.HasValue) || associationExpense.Invoices.Count == 0)
                {
                    result = "Facturi neadăugate! 50%";
                }
                else if (!associationExpense.RedistributeType.HasValue)
                {
                    result = "Redistribuie cheltuiala! 80%";
                }
            }
            else if (associationExpense.ExpenseTypes.Id == (int)ExpenseType.Individual)
            {
                if (associationExpense.ApartmentExpenses.Any(te => !te.Value.HasValue) || associationExpense.Associations.Apartments.Where(t => t.HasHeatHelp.HasValue).Count() != associationExpense.ApartmentExpenses.Count())
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
                if (associationExpense != null && associationExpense.Invoices.All(i => i.Value.HasValue) && associationExpense.Invoices.Count > 0)
                {
                    result = "<i class='fa fa-check'></i> 100%";
                }
                else if (associationExpense.Invoices.Any(i => !i.Value.HasValue) || associationExpense.Invoices.Count == 0)
                {
                    result = "Facturi neadăugate! 50%";
                }
                else if (!associationExpense.RedistributeType.HasValue)
                {
                    result = "Redistribuie cheltuiala! 80%";
                }
            }

            return result;
        }

        public static ExpensesCompletedStatus StatusOfInvoicesForNoSplit2(string result, string redistributeValue, string percentage, AssociationExpenses associationExpense)
        {
            return ExpensesCompletedStatus.All;
        }

        public static string StatusOfInvoices(AssociationExpenses associationExpense, bool isExpensePerIndex)
        {
            string result = string.Empty;
            var redistributeValue = RedistributionManager.CalculateRedistributeValueAsString(associationExpense.Id);
            var percentage = string.Empty;

            if (isExpensePerIndex)
            {
                percentage = GetPercentageAsString(associationExpense);
            }

            //if (estateExpense.SplitPerStairCase.HasValue && estateExpense.SplitPerStairCase.Value)
            //{
                result = StatusOfInvoicesForSplit(associationExpense, result, redistributeValue, percentage);
            //}
            //else
            //{
            //    result = StatusOfInvoicesForNoSplit(result, redistributeValue, percentage, estateExpense);
            //}

            return result;
        }

        public static string GetPercentageAsString(AssociationExpenses associationExpense)
        {
            string percentage = string.Empty;

            int apartmentsWithCountersOfThatExpense = GetApartmentsWithCountersOfThatExpense(associationExpense);

            if (apartmentsWithCountersOfThatExpense > 0)
            {
                percentage = ExpensePercentageFilledInAsString(associationExpense, apartmentsWithCountersOfThatExpense);
            }
            else
            {
                percentage = "100";
            }

            return percentage;
        }

        public static decimal GetPercentage(AssociationExpenses associationExpense)
        {
            decimal percentage = 0.0m;

            int apartmentsWithCountersOfThatExpense = GetApartmentsWithCountersOfThatExpense(associationExpense);

            if (apartmentsWithCountersOfThatExpense > 0)
            {
                percentage = ExpensePercentageFilledIn(associationExpense, apartmentsWithCountersOfThatExpense);
            }
            else
            {
                percentage = 100m;
            }

            return percentage;
        }

        private static string ExpensePercentageFilledInAsString(AssociationExpenses associationExpense, int apartmentsWithCounters)
        {
            var addedExpenses = associationExpense.ApartmentExpenses.Count(te => te.IndexNew.HasValue);
            var percentage = (((decimal)addedExpenses / (decimal)apartmentsWithCounters) * 100).ToString("0.##");

            return percentage;
        }

        private static decimal ExpensePercentageFilledIn(AssociationExpenses associationExpense, int apartmentsWithCounters)
        {
            var addedExpenses = associationExpense.ApartmentExpenses.Count(te => te.IndexNew.HasValue);
            var percentage = (((decimal)addedExpenses / (decimal)apartmentsWithCounters) * 100);

            return percentage;
        }

        public static string ExpensePercentageFilledInMessage(AssociationExpenses associationExpense)
        {
            var addedExpenses = associationExpense.ApartmentExpenses.Count(te => te.IndexNew.HasValue);
            int apartmentsWithCountersOfThatExpense = GetApartmentsWithCountersOfThatExpense(associationExpense);

            return "<b>" + addedExpenses + "</b> cheltuieli adăugate din <b>" + apartmentsWithCountersOfThatExpense + "</b> ";
        }

        private static int GetApartmentsWithCountersOfThatExpense(AssociationExpenses associationExpense)
        {
            int apartmentsWithCountersOfThatExpense = 0;
            List<AssociationCounters> allcountersOfExpense = CountersManager.GetAllByExpenseType(associationExpense.Associations.Id, associationExpense.Expenses.Id);
            foreach (var apartment in associationExpense.Associations.Apartments)
            {
                if (allcountersOfExpense.Select(c => c.Id).Intersect(apartment.AssociationCountersApartment.Select(ac => ac.Id_Counters)).Any())
                {
                    apartmentsWithCountersOfThatExpense++;
                }
            }
            return apartmentsWithCountersOfThatExpense;
        }

        #endregion


        public static void CloseMonth(int association, int year, int month, bool shouldClose = true)
        {
            List<AssociationExpenses> allEE = GetContext(true).AssociationExpenses.Where(ee => ee.Id_Estate == association &&
                ee.Year == year && ee.Month == month).ToList();
            foreach (var associationExpense in allEE)
            {
                associationExpense.IsClosed = shouldClose;
                GetContext().Entry(associationExpense).CurrentValues.SetValues(associationExpense);
                GetContext().SaveChanges();
            }
        }


        public static List<Expense> CheckCloseMonth(int association, int year, int month)
        {
            List<Expense> result = new List<Expense>();

            List<AssociationExpenses> allEE = GetContext(true).AssociationExpenses.Where(ee => ee.Id_Estate == association &&
                ee.Year == year && ee.Month == month).ToList();
            foreach (var associationExpense in allEE)
            {
                if (associationExpense.SplitPerStairCase.HasValue && associationExpense.SplitPerStairCase.Value)
                {
                    if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerApartments || associationExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
                    {
                        if (associationExpense.Invoices.Count != associationExpense.Associations.StairCases.Count || associationExpense.Invoices.Any(i => !i.Value.HasValue))
                        {
                            result.Add((Expense)associationExpense.Id_Expense);
                        }
                    }
                    else if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerIndex)
                    {
                        var percentage = GetPercentage(associationExpense);
                        if (percentage != 100m)
                        {
                            result.Add((Expense)associationExpense.Id_Expense);
                        }
                    }
                }
                else
                {
                    if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerApartments || associationExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
                    {
                        if (associationExpense.Invoices.Any(i => !i.Value.HasValue) && associationExpense.Invoices.Count != 0)
                        {
                            result.Add((Expense)associationExpense.Id_Expense);
                        }
                    }
                    else if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerIndex)
                    {
                        var percentage = GetPercentage(associationExpense);
                        if (percentage != 100m)
                        {
                            result.Add((Expense)associationExpense.Id_Expense);
                        }
                    }
                    else if (associationExpense.Id_ExpenseType == (int)ExpenseType.Individual)
                    {
                        if (associationExpense.ApartmentExpenses.Any(te => !te.Value.HasValue))
                        {
                            result.Add((Expense)associationExpense.Id_Expense);
                        }
                    }
                }
            }

            return result;
        }

        public static bool IsMonthClosed(int association, int year, int month)
        {
            List<AssociationExpenses> allEE = GetContext(true).AssociationExpenses.Where(ee => ee.Id_Estate == association &&
                ee.Year == year && ee.Month == month).ToList();

            return allEE.All(e => e.IsClosed.HasValue && e.IsClosed.Value);
        }

        public static List<Apartments> GetApartmentsNrThatShouldRedistributeTo(int associationExpenseId)
        {
            List<Apartments> result = new List<Apartments>();

            var associationExpense = AssociationExpensesManager.GetById(associationExpenseId);
            if (associationExpense != null)
            {
                result = associationExpense.ApartmentExpenses.Where(te => te.Apartments.AssociationCountersApartment.Any(ac => ac.AssociationCounters.Id_Expense == associationExpense.Id_Expense)).Select(te => te.Apartments).ToList();
            }

            return result;
        }

        public static List<Apartments> GetApartmentsNrThatShouldRedistributeTo(int associationExpenseId, int? stairCaseId)
        {
            List<Apartments> result = new List<Apartments>();

            var associationExpense = AssociationExpensesManager.GetById(associationExpenseId);
            if (associationExpense != null)
            {
                result = associationExpense.ApartmentExpenses.Where(te => te.Apartments.AssociationCountersApartment.Any(ac => ac.AssociationCounters.Id_Expense == associationExpense.Id_Expense)
                    && te.Apartments.Id_StairCase == stairCaseId).Select(te => te.Apartments).ToList();
            }

            return result;
        }

        public static bool HasCounterOfExpense(int apartmentId, int expenseId)
        {
            var counters = CountersManager.GetByApartment(apartmentId);
            return counters.Any(c => c.Id_Expense == expenseId);
        }
    }
}
