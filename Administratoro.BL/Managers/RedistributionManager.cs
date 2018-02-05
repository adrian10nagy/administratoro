﻿using Administratoro.BL.Constants;
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
            decimal? sumOfIndiviza = TenantExpensesManager.GetSumOfIndivizaForExpense(estateExpense);
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

        public static string CalculateRedistributeValueAsString(int estateExpenseId)
        {
            var result = CalculateRedistributeValue(estateExpenseId);

            return result.HasValue ? result.Value.ToString() : string.Empty;
        }
    }
}

