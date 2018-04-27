using Administratoro.BL.Constants;
using Administratoro.BL.Extensions;
using Administratoro.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Administratoro.BL.Managers
{
    public static class RedistributionManager
    {
        public static string RedistributeValueCotaIndivizaAsString(Associations association, AssociationExpenses associationExpense)
        {
            decimal? allInvoicesSum = RedistributeValueCotaIndiviza(association, associationExpense);

            return allInvoicesSum.HasValue ? allInvoicesSum.Value.ToString() : string.Empty;
        }

        public static decimal? RedistributeValueCotaIndiviza(Associations association, AssociationExpenses associationExpense)
        {
            decimal? allInvoicesSum = associationExpense.Invoices.Where(i => i.Value.HasValue).Sum(i => i.Value);
            decimal? sumOfIndiviza = ApartmentsManager.GetSumOfIndivizaForAllApartments(associationExpense.Id_Estate);
            decimal? result = (sumOfIndiviza != null && association != null && allInvoicesSum.HasValue)
                ? ((sumOfIndiviza.Value / association.Indiviza) * allInvoicesSum.Value)
                : null;

            return result;
        }

        public static decimal? RedistributeValueCotaIndivizaForSpecificApartments(Apartments apartment, Invoices invoice, List<Apartments> apartments)
        {
            decimal? sumOfIndiviza = apartments.Sum(t => t.CotaIndiviza);
            decimal? result = null;
            if (invoice != null)
            {
                decimal? allInvoicesSum = invoice.Value;
                result = (sumOfIndiviza != null && allInvoicesSum.HasValue && sumOfIndiviza.Value != 0)
                ? ((allInvoicesSum.Value * apartment.CotaIndiviza) / sumOfIndiviza.Value)
                : null;
            }

            return result;
        }

        public static decimal? RedistributeValueCotaIndiviza(Associations association, AssociationExpenses associationExpense, int? stairCaseId)
        {
            decimal? allInvoicesSum = associationExpense.Invoices.Where(i => i.Value.HasValue && i.Id_StairCase == stairCaseId).Sum(i => i.Value);
            decimal? sumOfIndiviza = ApartmentsManager.GetSumOfIndivizaForAllApartments(associationExpense.Id_Estate, stairCaseId);
            decimal? result = (sumOfIndiviza != null && association != null && allInvoicesSum.HasValue)
                ? ((sumOfIndiviza.Value / association.Indiviza) * allInvoicesSum.Value)
                : null;

            return result;
        }

        public static decimal? RedistributeValuePerIndex(AssociationExpenses associationExpense)
        {
            decimal? result = null;

            decimal? sumOfIndexes = ApartmentExpensesManager.GetSumOfIndexesForexpense(associationExpense.Id);
            //if (estateExpense.SplitPerStairCase.HasValue && estateExpense.SplitPerStairCase.Value)
            //{
            //    decimal? sumOfInvoices = null;
            //    foreach (StairCases stairCase in estateExpense.Association.StairCases)
            //    {
            //        var invoice = stairCase.Invoices.FirstOrDefault(i => i.Id_StairCase == stairCase.Id && i.Id_EstateExpense == estateExpense.Id);
            //        if (invoice != null && invoice.Value.HasValue)
            //        {
            //            if (!sumOfInvoices.HasValue)
            //            {
            //                sumOfInvoices = 0m;
            //            }
            //            sumOfInvoices = sumOfInvoices + invoice.Value.Value;
            //        }
            //    }

            //    if (sumOfInvoices.HasValue && estateExpense.PricePerExpenseUnit.HasValue && sumOfIndexes.HasValue)
            //    {
            //        result = (sumOfInvoices.Value - (estateExpense.PricePerExpenseUnit.Value * sumOfIndexes.Value));
            //    }
            //}
            //else
            //{
            var invoice = associationExpense.Invoices.FirstOrDefault(i => i.Id_StairCase == null && i.Id_EstateExpense == associationExpense.Id);

            if (sumOfIndexes.HasValue & invoice != null && invoice.Value.HasValue && associationExpense.PricePerExpenseUnit.HasValue)
            {
                result = (invoice.Value - (associationExpense.PricePerExpenseUnit * sumOfIndexes.Value));
            }
            //}

            return result;
        }

        public static string RedistributeValuePerIndexAsString(AssociationExpenses associationExpense)
        {
            var result = RedistributeValuePerIndex(associationExpense);

            return result.HasValue ? result.Value.ToString() : string.Empty;
        }


        public static decimal? CalculateRedistributeValue(int associationExpenseId)
        {
            decimal? result = null;

            AssociationExpenses associationExpense = AssociationExpensesManager.GetById(associationExpenseId);

            if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerIndex)
            {
                result = RedistributionManager.RedistributeValuePerIndex(associationExpense);
            }
            else if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
            {
                var estate = AssociationsManager.GetById(associationExpenseId);
                result = RedistributionManager.RedistributeValueCotaIndiviza(estate, associationExpense);
            }

            return result;
        }

        public static decimal? CalculateRedistributeValueForStairCase(int associationExpenseId, Apartments apartment, IEnumerable<ApartmentExpenses> apartmentExpenses)
        {
            decimal? result = null;

            AssociationExpenses associationExpense = AssociationExpensesManager.GetById(associationExpenseId);

            if (!associationExpense.RedistributeType.HasValue)
            {
                return result;
            }

            if (associationExpense.RedistributeType.Value == (int)RedistributionType.PerApartament)
            {
                result = RedistributionManager.RedistributeValuePerApartment(associationExpense, apartment);
            }
            else if (associationExpense.RedistributeType.Value == (int)RedistributionType.PerApartments)
            {
                result = RedistributionManager.RedistributeValuePerApartments(associationExpense, apartment);
            }
            else if (associationExpense.RedistributeType.Value == (int)RedistributionType.PerConsumption)
            {
                result = RedistributionManager.RedistributeValuePerConsumption(associationExpense, apartment, apartmentExpenses);
            }

            return result;
        }

        private static decimal? RedistributeValuePerConsumption(AssociationExpenses associationExpense, Apartments apartment, IEnumerable<ApartmentExpenses> apartmentExpenses)
        {
            decimal? result = null;
            decimal apartmentConsumption = GetApartmentConsumprionAsDecimal(apartmentExpenses);

            if (associationExpense.SplitPerStairCase.HasValue && associationExpense.SplitPerStairCase.Value)
            {
                var invoice = associationExpense.Invoices.FirstOrDefault(i => i.Id_StairCase == apartment.Id_StairCase && i.Id_EstateExpense == associationExpense.Id);
                var sumOfIndicesForStairCase = ApartmentExpensesManager.GetSumOfIndexesForexpense(associationExpense.Id, apartment.Id_StairCase);

                if (invoice != null && invoice.Value.HasValue && sumOfIndicesForStairCase.HasValue && associationExpense.PricePerExpenseUnit.HasValue)
                {
                    var redistributeValue = invoice.Value - (sumOfIndicesForStairCase * associationExpense.PricePerExpenseUnit);
                    result = (apartmentConsumption / sumOfIndicesForStairCase) * redistributeValue;
                }

            }
            else
            {
                var redistributeValue = RedistributeValuePerIndex(associationExpense);
                var sumOfIndices = ApartmentExpensesManager.GetSumOfIndexesForexpense(associationExpense.Id);
                if (redistributeValue.HasValue && sumOfIndices != 0)
                {
                    result = ((apartmentConsumption) / sumOfIndices) * redistributeValue;
                }
            }

            return result;
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

        private static decimal? RedistributeValuePerApartments(AssociationExpenses associationExpense, Apartments apartment)
        {
            decimal? result = null;

            if (associationExpense.SplitPerStairCase.HasValue && associationExpense.SplitPerStairCase.Value)
            {
                var redistributeSum = GetRedistributeValueForIndexExpenseForStairCase(associationExpense, apartment);
                var allApartmentsDependents = AssociationExpensesManager.GetApartmentsNrThatShouldRedistributeTo(associationExpense.Id, apartment.Id_StairCase).Sum(t => t.Dependents);

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
            }
            else
            {
                var allApartmentsDependents = AssociationExpensesManager.GetApartmentsNrThatShouldRedistributeTo(associationExpense.Id).Sum(e => e.Dependents);

                if (allApartmentsDependents != 0)
                {
                    var redistributeVal = CalculateRedistributeValue(associationExpense.Id);
                    if (redistributeVal != null && redistributeVal.HasValue && allApartmentsDependents != 0)
                    {
                        if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerIndex && !AssociationExpensesManager.HasCounterOfExpense(apartment.Id, associationExpense.Id_Expense))
                        {
                        }
                        else
                        {
                            result = redistributeVal.Value * apartment.Dependents / allApartmentsDependents;
                        }
                    }
                }
            }

            return result;
        }

        private static decimal GetSumToRedistribute(AssociationExpenses associationExpense, decimal? sumOfInvoices, decimal? sumOfIndices)
        {
            decimal sumToRedistribute;
            if (associationExpense.PricePerExpenseUnit.HasValue && sumOfIndices.HasValue)
            {
                sumToRedistribute = sumOfInvoices.Value - (sumOfIndices.Value * associationExpense.PricePerExpenseUnit.Value);
            }
            else
            {
                sumToRedistribute = sumOfInvoices.Value;
            }

            return sumToRedistribute;
        }

        private static decimal? RedistributeValuePerApartment(AssociationExpenses associationExpense, Apartments apartment)
        {
            decimal? result = null;
            if (associationExpense.SplitPerStairCase.HasValue && associationExpense.SplitPerStairCase.Value)
            {
                var redistributeSum = GetRedistributeValueForIndexExpenseForStairCase(associationExpense, apartment);

                var allApartmentsNr = AssociationExpensesManager.GetApartmentsNrThatShouldRedistributeTo(associationExpense.Id, apartment.Id_StairCase).Count();

                if (redistributeSum.HasValue && allApartmentsNr != 0)
                {
                    if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerIndex && !AssociationExpensesManager.HasCounterOfExpense(apartment.Id, associationExpense.Id_Expense))
                    {
                    }
                    else
                    {
                        result = redistributeSum / allApartmentsNr;
                    }
                }
            }
            else
            {
                var redistributeVal = CalculateRedistributeValue(associationExpense.Id);
                int allApartmentsNr;
                if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerIndex)
                {
                    IEnumerable<Apartments> apartments = AssociationExpensesManager.GetApartmentsNrThatShouldRedistributeTo(associationExpense.Id);
                    allApartmentsNr = apartments.Count();
                }
                else
                {
                    allApartmentsNr = associationExpense.Associations.Apartments.Count();
                }

                if (redistributeVal != null && redistributeVal.HasValue && allApartmentsNr != 0)
                {
                    if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerIndex && !AssociationExpensesManager.HasCounterOfExpense(apartment.Id, associationExpense.Id_Expense))
                    {
                    }
                    else
                    {
                        result = redistributeVal.Value / allApartmentsNr;
                    }
                }
            }

            return result;
        }

        private static decimal? GetRedistributeValueForIndexExpenseForStairCase(AssociationExpenses associationExpense, Apartments apartment)
        {
            decimal? sumOfInvoicesForStairCase = GetSumOfInvoices(associationExpense, apartment);
            var sumOfExpensesForStairCase = associationExpense.ApartmentExpenses.Where(tex => tex.Apartments.Id_StairCase == apartment.Id_StairCase).Sum(t => t.IndexNew - t.IndexOld) * associationExpense.PricePerExpenseUnit;
            var redistributeSum = sumOfInvoicesForStairCase - sumOfExpensesForStairCase;
            return redistributeSum;
        }

        private static decimal? GetSumOfInvoices(AssociationExpenses associationExpense, Apartments apartment)
        {
            decimal? sumOfInvoices = null;
            foreach (var invoice in associationExpense.Invoices.Where(i => i.Id_StairCase == apartment.Id_StairCase))
            {
                if (invoice != null && invoice.Value.HasValue)
                {
                    if (!sumOfInvoices.HasValue)
                    {
                        sumOfInvoices = 0m;
                    }
                    sumOfInvoices = sumOfInvoices + invoice.Value.Value;
                }
            }

            return sumOfInvoices;
        }

        public static string CalculateRedistributeValueAsString(int associationExpenseId)
        {
            var result = CalculateRedistributeValue(associationExpenseId);

            return result.HasValue ? result.Value.ToString() : string.Empty;
        }
    }
}

