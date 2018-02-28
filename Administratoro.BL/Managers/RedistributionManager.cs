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
        public static string RedistributeValueCotaIndivizaAsString(Estates association, EstateExpenses estateExpense)
        {
            decimal? allInvoicesSum = RedistributeValueCotaIndiviza(association, estateExpense);

            return allInvoicesSum.HasValue ? allInvoicesSum.Value.ToString() : string.Empty;
        }

        public static decimal? RedistributeValueCotaIndiviza(Estates association, EstateExpenses estateExpense)
        {
            decimal? allInvoicesSum = estateExpense.Invoices.Where(i => i.Value.HasValue).Sum(i => i.Value);
            decimal? sumOfIndiviza = ApartmentsManager.GetSumOfIndivizaForAllTenants(estateExpense.Id_Estate);
            decimal? result = (sumOfIndiviza != null && association != null && allInvoicesSum.HasValue)
                ? ((sumOfIndiviza.Value / association.Indiviza) * allInvoicesSum.Value)
                : null;

            return result;
        }

        public static decimal? RedistributeValueCotaIndivizaForSpecificTenants(Tenants tenant, Invoices invoice, List<Tenants> tenants)
        {
            decimal? sumOfIndiviza = tenants.Sum(t => t.CotaIndiviza);
            decimal? result = null;
            if (invoice != null)
            {
                decimal? allInvoicesSum = invoice.Value;
                result = (sumOfIndiviza != null && allInvoicesSum.HasValue && sumOfIndiviza.Value != 0)
                ? ((allInvoicesSum.Value * tenant.CotaIndiviza) / sumOfIndiviza.Value)
                : null;
            }
            
            return result;
        }

        public static decimal? RedistributeValueCotaIndiviza(Estates association, EstateExpenses estateExpense, int? stairCaseId)
        {
            decimal? allInvoicesSum = estateExpense.Invoices.Where(i => i.Value.HasValue && i.Id_StairCase == stairCaseId).Sum(i => i.Value);
            decimal? sumOfIndiviza = ApartmentsManager.GetSumOfIndivizaForAllTenants(estateExpense.Id_Estate, stairCaseId);
            decimal? result = (sumOfIndiviza != null && association != null && allInvoicesSum.HasValue)
                ? ((sumOfIndiviza.Value / association.Indiviza) * allInvoicesSum.Value)
                : null;

            return result;
        }

        public static decimal? RedistributeValuePerIndex(EstateExpenses estateExpense)
        {
            decimal? result = null;

            decimal? sumOfIndexes = TenantExpensesManager.GetSumOfIndexesForexpense(estateExpense.Id);
            if (estateExpense.SplitPerStairCase.HasValue && estateExpense.SplitPerStairCase.Value)
            {
                decimal? sumOfInvoices = null;
                foreach (StairCases stairCase in estateExpense.Estates.StairCases)
                {
                    var invoice = stairCase.Invoices.FirstOrDefault(i => i.Id_StairCase == stairCase.Id && i.Id_EstateExpense == estateExpense.Id);
                    if (invoice != null && invoice.Value.HasValue)
                    {
                        if (!sumOfInvoices.HasValue)
                        {
                            sumOfInvoices = 0m;
                        }
                        sumOfInvoices = sumOfInvoices + invoice.Value.Value;
                    }
                }

                if (sumOfInvoices.HasValue && estateExpense.PricePerExpenseUnit.HasValue && sumOfIndexes.HasValue)
                {
                    result = (sumOfInvoices.Value - (estateExpense.PricePerExpenseUnit.Value * sumOfIndexes.Value));
                }
            }
            else
            {
                var invoice = estateExpense.Invoices.FirstOrDefault(i => i.Id_StairCase == null && i.Id_EstateExpense == estateExpense.Id);

                if (sumOfIndexes.HasValue & invoice != null && invoice.Value.HasValue && estateExpense.PricePerExpenseUnit.HasValue)
                {
                    result = (invoice.Value - (estateExpense.PricePerExpenseUnit * sumOfIndexes.Value));
                }
            }

            return result;
        }

        public static string RedistributeValuePerIndexAsString(EstateExpenses estateExpense)
        {
            var result = RedistributeValuePerIndex(estateExpense);

            return result.HasValue ? result.Value.ToString() : string.Empty;
        }


        public static decimal? CalculateRedistributeValue(int estateExpenseId)
        {
            decimal? result = null;

            EstateExpenses estateExpense = EstateExpensesManager.GetById(estateExpenseId);

            if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerIndex)
            {
                result = RedistributionManager.RedistributeValuePerIndex(estateExpense);
            }
            else if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
            {
                var estate = AssociationsManager.GetById(estateExpenseId);
                result = RedistributionManager.RedistributeValueCotaIndiviza(estate, estateExpense);
            }

            return result;
        }

        public static decimal? CalculateRedistributeValueForStairCase(int estateExpenseId, Tenants tenant, TenantExpenses te)
        {
            decimal? result = null;

            EstateExpenses estateExpense = EstateExpensesManager.GetById(estateExpenseId);

            if (!estateExpense.RedistributeType.HasValue)
            {
                return result;
            }

            if (estateExpense.RedistributeType.Value == (int)RedistributionType.PerApartament)
            {
                result = RedistributionManager.RedistributeValuePerApartment(estateExpense, tenant, te);
            }
            else if (estateExpense.RedistributeType.Value == (int)RedistributionType.PerTenants)
            {
                result = RedistributionManager.RedistributeValuePerTenants(estateExpense, tenant, te);
            }
            else if (estateExpense.RedistributeType.Value == (int)RedistributionType.PerConsumption)
            {
                result = RedistributionManager.RedistributeValuePerConsumption(estateExpense, tenant, te);
            }

            return result;
        }

        private static decimal? RedistributeValuePerConsumption(EstateExpenses estateExpense, Tenants tenant, TenantExpenses te)
        {
            return null;
        }

        private static decimal? RedistributeValuePerTenants(EstateExpenses estateExpense, Tenants tenant, TenantExpenses te)
        {
            decimal? result = null;

            if (estateExpense.SplitPerStairCase.HasValue && estateExpense.SplitPerStairCase.Value)
            {
                decimal? sumOfInvoices = GetSumOfInvoices(estateExpense, tenant);
                var allTenantDependents = ApartmentsManager.GetDependentsNr(estateExpense.Id_Estate, tenant.Id_StairCase);

                if (sumOfInvoices.HasValue && allTenantDependents != 0)
                {
                    var sumOfIndices = TenantExpensesManager.GetSumOfIndexesForexpense(estateExpense.Id, tenant.Id_StairCase);
                    decimal sumToRedistribute = GetSumToRedistribute(estateExpense, sumOfInvoices, sumOfIndices);
                    result = DecimalExtensions.RoundUp((double)(sumToRedistribute * tenant.Dependents / allTenantDependents), 2);
                }
            }
            else
            {
                var allTenantDependents = ApartmentsManager.GetDependentsNr(estateExpense.Id_Estate);
                if (allTenantDependents != 0)
                {
                    var redistributeVal = CalculateRedistributeValue(estateExpense.Id);
                    if (redistributeVal != null && redistributeVal.HasValue && allTenantDependents != 0)
                    {
                        result = DecimalExtensions.RoundUp((double)(redistributeVal.Value * tenant.Dependents / allTenantDependents), 2);
                    }
                }
            }

            return result;
        }

        private static decimal GetSumToRedistribute(EstateExpenses estateExpense, decimal? sumOfInvoices, decimal? sumOfIndices)
        {
            decimal sumToRedistribute;
            if (estateExpense.PricePerExpenseUnit.HasValue && sumOfIndices.HasValue)
            {
                sumToRedistribute = sumOfInvoices.Value - (sumOfIndices.Value * estateExpense.PricePerExpenseUnit.Value);
            }
            else
            {
                sumToRedistribute = sumOfInvoices.Value;
            }

            return sumToRedistribute;
        }

        private static decimal? RedistributeValuePerApartment(EstateExpenses estateExpense, Tenants tenant, TenantExpenses te)
        {
            decimal? result = null;
            if (estateExpense.SplitPerStairCase.HasValue && estateExpense.SplitPerStairCase.Value)
            {
                decimal? sumOfInvoices = GetSumOfInvoices(estateExpense, tenant);
                var allApartmentsNr = AssociationsManager.GetNrOfApartments(estateExpense.Id_Estate, tenant.Id_StairCase);

                if (sumOfInvoices.HasValue && allApartmentsNr != 0)
                {
                    var sumOfIndices = TenantExpensesManager.GetSumOfIndexesForexpense(estateExpense.Id, tenant.Id_StairCase);
                    decimal sumToRedistribute = GetSumToRedistribute(estateExpense, sumOfInvoices, sumOfIndices);
                    result = DecimalExtensions.RoundUp((double)(sumToRedistribute / allApartmentsNr), 2);
                }
            }
            else
            {
                var redistributeVal = CalculateRedistributeValue(estateExpense.Id);
                var allApartmentsNr = estateExpense.Estates.Tenants.Count();
                if (redistributeVal != null && redistributeVal.HasValue && allApartmentsNr != 0)
                {
                    result = DecimalExtensions.RoundUp((double)(redistributeVal.Value / allApartmentsNr), 2);
                }
            }

            return result;
        }

        private static decimal? GetSumOfInvoices(EstateExpenses estateExpense, Tenants tenant)
        {
            decimal? sumOfInvoices = null;
            foreach (var invoice in estateExpense.Invoices.Where(i => i.Id_StairCase == tenant.Id_StairCase))
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

        public static string CalculateRedistributeValueAsString(int estateExpenseId)
        {
            var result = CalculateRedistributeValue(estateExpenseId);

            return result.HasValue ? result.Value.ToString() : string.Empty;
        }
    }
}

