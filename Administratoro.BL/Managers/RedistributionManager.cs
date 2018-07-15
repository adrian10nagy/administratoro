
using System.Globalization;

namespace Administratoro.BL.Managers
{
    using Constants;
    using DAL;
    using System.Collections.Generic;
    using System.Linq;

    public static class RedistributionManager
    {
        public static decimal? RedistributeValueCotaIndivizaForSpecificApartments(Apartments apartment, Invoices invoice, List<Apartments> apartments)
        {
            decimal? sumOfIndiviza = apartments.Sum(t => t.CotaIndiviza);
            decimal? result = null;
            if (invoice != null)
            {
                decimal? allInvoicesSum = invoice.Value;
                result = (sumOfIndiviza != null && allInvoicesSum.HasValue && sumOfIndiviza.Value != 0)
                ? (allInvoicesSum.Value * apartment.CotaIndiviza) / sumOfIndiviza.Value
                : null;
            }

            return result;
        }

        public static decimal? GetRedistributeValuePerIndex(AssociationExpenses associationExpense)
        {
            decimal? result = null;

            var invoice = associationExpense.Invoices.FirstOrDefault(i => i.Id_StairCase == null && i.Id_EstateExpense == associationExpense.Id);
            var assCountersForThisExpense = AssociationCountersManager.GetAllByExpenseType(associationExpense.Id_Estate, associationExpense.Id_Expense).ToList();
            foreach (var counter in assCountersForThisExpense)
            {
                var pricePerUnit = UnitPricesManager.GetPrice(associationExpense.Id, counter.AssociationCountersStairCase.FirstOrDefault().Id_StairCase);
                decimal? sumOfIndexes = ApartmentExpensesManager.GetSumOfIndexesOnSameCounter(associationExpense.Id, counter.AssociationCountersStairCase.FirstOrDefault().Id_StairCase);
                if (sumOfIndexes.HasValue & invoice != null && pricePerUnit.HasValue)
                {
                    var consummedIndices = InvoiceIndexesManager.GetByInvoiceAndCounterFirst(invoice, counter);

                    if (consummedIndices != null && consummedIndices.IndexOld.HasValue && consummedIndices.IndexNew.HasValue)
                    {
                        result = ((consummedIndices.IndexNew.Value - consummedIndices.IndexOld.Value) - sumOfIndexes.Value) * pricePerUnit.Value;
                    }
                }
            }

            return result;
        }

        public static decimal? GetRedistributeValuePerIndex(AssociationExpenses associationExpense, int? stairCase)
        {
            decimal? result = null;

            var invoice = associationExpense.Invoices.FirstOrDefault(i => i.Id_StairCase == null && i.Id_EstateExpense == associationExpense.Id);
            var assCountersForThisExpense = AssociationCountersManager.GetByExpenseAndStairCase(associationExpense, stairCase);
            if (assCountersForThisExpense != null)
            {
                var pricePerUnit = UnitPricesManager.GetPrice(associationExpense.Id, assCountersForThisExpense.AssociationCountersStairCase.FirstOrDefault().Id_StairCase);
                decimal? sumOfIndexes = ApartmentExpensesManager.GetSumOfIndexesOnSameCounter(associationExpense.Id, assCountersForThisExpense.AssociationCountersStairCase.FirstOrDefault().Id_StairCase);
                if (sumOfIndexes.HasValue & invoice != null && pricePerUnit.HasValue)
                {
                    var consummedIndices = InvoiceIndexesManager.GetByInvoiceAndCounterFirst(invoice, assCountersForThisExpense);

                    if (consummedIndices != null && consummedIndices.IndexOld.HasValue && consummedIndices.IndexNew.HasValue)
                    {
                        result = ((consummedIndices.IndexNew.Value - consummedIndices.IndexOld.Value) - sumOfIndexes.Value) * pricePerUnit.Value;
                    }
                }
            }

            return result;
        }

        public static decimal? CalculateRedistributeValue(int associationExpenseId)
        {
            decimal? result = null;

            AssociationExpenses associationExpense = AssociationExpensesManager.GetById(associationExpenseId);

            if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerIndex)
            {
                result = GetRedistributeValuePerIndex(associationExpense);
            }

            return result;
        }

        public static decimal? CalculateRedistributeValueForStairCase(int associationExpenseId, Apartments apartment, IEnumerable<ApartmentExpenses> apartmentExpenses)
        {
            decimal? result = null;

            AssociationExpenses associationExpense = AssociationExpensesManager.GetById(associationExpenseId);

            if (!associationExpense.RedistributeType.HasValue)
            {
                return null;
            }

            switch (associationExpense.RedistributeType.Value)
            {
                case (int)RedistributionType.PerApartament:
                    result = RedistributeValuePerApartment(associationExpense, apartment);
                    break;
                case (int)RedistributionType.PerDependents:
                    result = RedistributeValuePerApartmentDependents(associationExpense, apartment);
                    break;
                case (int)RedistributionType.PerConsumption:
                    result = RedistributeValuePerConsumption(associationExpense, apartment, apartmentExpenses);
                    break;

            }

            return result;
        }

        private static decimal? RedistributeValuePerConsumption(AssociationExpenses associationExpense, Apartments apartment, IEnumerable<ApartmentExpenses> apartmentExpenses)
        {
            decimal? result = null;
            decimal apartmentConsumption = GetApartmentConsumprionAsDecimal(apartmentExpenses);

            var redistributeValue = GetRedistributeValuePerIndex(associationExpense, apartment.Id_StairCase);
            decimal? sumOfIndexes = ApartmentExpensesManager.GetSumOfIndexesOnSameCounter(associationExpense.Id, apartment.Id_StairCase);

            if (redistributeValue.HasValue && sumOfIndexes.HasValue && sumOfIndexes.Value != 0)
            {
                result = (apartmentConsumption / sumOfIndexes.Value) * redistributeValue;
            }

            return result;
        }

        private static decimal? RedistributeValuePerApartmentDependents(AssociationExpenses associationExpense, Apartments apartment)
        {
            decimal? result = null;

            //if (associationExpense.SplitPerStairCase.HasValue && associationExpense.SplitPerStairCase.Value)
            //{
            var redistributeSum = GetRedistributeValueForIndexExpenseForStairCase(associationExpense, apartment);
            var allApartmentsDependents = ApartmentExpensesManager.GetApartmentsOnSameCounter(associationExpense.Id, apartment.Id_StairCase).Sum(t => t.Dependents);

            if (redistributeSum.HasValue && allApartmentsDependents != 0)
            {
                if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerIndex && !AssociationExpensesManager.HasCounterOfExpense(apartment.Id, associationExpense.Id_Expense))
                {
                }
                else
                {
                    result = redistributeSum * apartment.Dependents / allApartmentsDependents;
                }
            }
            //}
            //else
            //{
            // var allApartmentsDependentsu = AssociationExpensesManager.GetApartmentsNrThatShouldRedistributeTo(associationExpense.Id).Sum(e => e.Dependents);

            //if (allApartmentsDependents != 0)
            //{
            //    var redistributeVal = CalculateRedistributeValue(associationExpense.Id);
            //    if (redistributeVal != null && redistributeVal.HasValue && allApartmentsDependents != 0)
            //    {
            //        if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerIndex && !AssociationExpensesManager.HasCounterOfExpense(apartment.Id, associationExpense.Id_Expense))
            //        {
            //        }
            //        else
            //        {
            //            result = redistributeVal.Value * apartment.Dependents / allApartmentsDependents;
            //        }
            //    }
            //}
            //}

            return result;
        }

        private static decimal? RedistributeValuePerApartment(AssociationExpenses associationExpense, Apartments apartment)
        {
            decimal? result = null;
            // todo change this to be for index expense or add new if branch
            //if (associationExpense.SplitPerStairCase.HasValue && associationExpense.SplitPerStairCase.Value)
            //{
            var redistributeSum = GetRedistributeValueForIndexExpenseForStairCase(associationExpense, apartment);

            var allApartmentsOnSamecounter = ApartmentExpensesManager.GetApartmentsOnSameCounter(associationExpense.Id, apartment.Id_StairCase).Count();

            if (redistributeSum.HasValue && allApartmentsOnSamecounter != 0)
            {
                if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerIndex && !AssociationExpensesManager.HasCounterOfExpense(apartment.Id, associationExpense.Id_Expense))
                {
                }
                else
                {
                    result = redistributeSum / allApartmentsOnSamecounter;
                }
            }
            //}
            //else
            //{
            //    var redistributeVal = CalculateRedistributeValue(associationExpense.Id);
            //    int allApartmentsNr;
            //    if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerIndex)
            //    {
            //        IEnumerable<Apartments> apartments = AssociationExpensesManager.GetApartmentsNrThatShouldRedistributeTo(associationExpense.Id);
            //        allApartmentsNr = apartments.Count();
            //    }
            //    else
            //    {
            //        allApartmentsNr = associationExpense.Associations.Apartments.Count();
            //    }

            //    if (redistributeVal != null && redistributeVal.HasValue && allApartmentsNr != 0)
            //    {
            //        if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerIndex && !AssociationExpensesManager.HasCounterOfExpense(apartment.Id, associationExpense.Id_Expense))
            //        {
            //        }
            //        else
            //        {
            //            result = redistributeVal.Value / allApartmentsNr;
            //        }
            //    }
            //}

            return result;
        }

        private static decimal? GetRedistributeValueForIndexExpenseForStairCase(AssociationExpenses associationExpense, Apartments apartment)
        {
            decimal? result = null;
            decimal? sumOfInvoiceForCounter = GetSumOfInvoiceForStairCaseCounter(associationExpense, apartment.Id_StairCase);

            if (sumOfInvoiceForCounter.HasValue)
            {
                var sumOfExpensesForCounter = ApartmentExpensesManager.GetConsumption(associationExpense, apartment.Id_StairCase);
                if (sumOfExpensesForCounter.HasValue)
                {
                    result = sumOfInvoiceForCounter - sumOfExpensesForCounter;
                }
            }

            return result;
        }

        private static decimal? GetSumOfInvoiceForStairCaseCounter(AssociationExpenses associationExpense, int? stairCase)
        {
            decimal? sumOfInvoices = null;
            var invoice = associationExpense.Invoices.FirstOrDefault();

            if (invoice == null)
            {
                return null;
            }

            var invoiceIndexes = InvoiceIndexesManager.Get(invoice, stairCase);

            foreach (var invoiceIndex in invoiceIndexes)
            {
                var pricePerUnit = UnitPricesManager.GetPrice(invoice.AssociationExpenses.Id, stairCase);

                if (invoiceIndex != null && invoiceIndex.IndexNew.HasValue && invoiceIndex.IndexOld.HasValue && invoiceIndex.Invoices.Value.HasValue
                    && pricePerUnit.HasValue)
                {

                    if (!sumOfInvoices.HasValue)
                    {
                        sumOfInvoices = 0m;
                    }
                    decimal? valueToAdd = (invoiceIndex.IndexNew - invoiceIndex.IndexOld) * pricePerUnit.Value;

                    sumOfInvoices = sumOfInvoices + valueToAdd;
                }
            }

            return sumOfInvoices;
        }

        public static string CalculateRedistributeValueAsString(int associationExpenseId)
        {
            var result = CalculateRedistributeValue(associationExpenseId);

            return result.HasValue ? result.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }

        private static decimal GetApartmentConsumprionAsDecimal(IEnumerable<ApartmentExpenses> apartmentExpenses)
        {
            decimal apartmentConsumption = 0;
            foreach (var apartmentExpense in apartmentExpenses)
            {
                // redistrib = invoice - allConsum 
                if (apartmentExpense.IndexNew.HasValue && apartmentExpense.IndexOld.HasValue)
                {
                    apartmentConsumption = apartmentConsumption + (apartmentExpense.IndexNew.Value - apartmentExpense.IndexOld.Value);
                }
            }
            return apartmentConsumption;
        }
    }

}
