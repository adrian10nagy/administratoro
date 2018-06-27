﻿
namespace Administratoro.BL.Managers
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Extensions;
    using Administratoro.BL.Models;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.SqlClient;
    using System.Linq;

    public static class ApartmentExpensesManager
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

        public static ApartmentExpenses GetForIndexExpensPreviousMonthApartmentExpense(int idExpenseEstateCurentMonth, int idApartment)
        {
            ApartmentExpenses result = null;
            var ee = AssociationExpensesManager.GetById(idExpenseEstateCurentMonth);
            if (ee != null)
            {
                var prevMonthAE = AssociationExpensesManager.GetPreviousMonth(ee);

                if (prevMonthAE != null)
                {
                    result = GetByExpenseEstateIdAndApartmentId(prevMonthAE.Id, idApartment);
                }
            }

            return result;
        }

        public static ApartmentExpenses GetByExpenseEstateIdAndApartmentId(int idExpenseEstate, int IdApartment)
        {
            return GetContext().ApartmentExpenses.FirstOrDefault(te => te.Id_EstateExpense == idExpenseEstate && te.Id_Tenant == IdApartment);
        }

        public static IEnumerable<ApartmentExpenses> GetByExpenseEstateIdAndApartmentIdAll(int idExpenseEstate, int IdApartment)
        {
            return GetContext().ApartmentExpenses.Where(te => te.Id_EstateExpense == idExpenseEstate && te.Id_Tenant == IdApartment);
        }

        public static ApartmentExpenses Get(int idExpenseEstate, int IdApartment, int? countOrder)
        {
            return GetContext().ApartmentExpenses.FirstOrDefault(te => te.Id_EstateExpense == idExpenseEstate
                && te.Id_Tenant == IdApartment
                && te.CounterOrder == countOrder);
        }

        private static ApartmentExpenses GetById(int apartmentExpenseId)
        {
            return GetContext().ApartmentExpenses.FirstOrDefault(te => te.Id == apartmentExpenseId);
        }

        public static decimal? GetSumOfIndexesForExpense(int estateExpenseId)
        {
            return GetContext().ApartmentExpenses.Where(te => te.Id_EstateExpense == estateExpenseId).Sum(s => s.IndexNew - s.IndexOld);
        }

        public static decimal? GetSumOfIndexesForExpense(int estateExpenseId, int? stairCase)
        {
            return GetContext().ApartmentExpenses.Where(te => te.Id_EstateExpense == estateExpenseId && te.Apartments.Id_StairCase == stairCase).Sum(s => s.IndexNew - s.IndexOld);
        }

        public static decimal? GetSumOfIndexesOnSameCounter(int associationExpenseId, int? stairCase)
        {
            var apps = GetApartmentsOnSameCounter(associationExpenseId, stairCase).Select(a => a.Id);

            return GetContext().ApartmentExpenses.Where(ae => ae.Id_EstateExpense == associationExpenseId && apps.Contains(ae.Apartments.Id)).Sum(s => s.IndexNew - s.IndexOld);
        }

        public static void AddApartmentExpense(int apartmentid, int associationExpenseId, decimal expenseValue)
        {
            ApartmentExpenses te = new ApartmentExpenses
            {
                Value = expenseValue,
                Id_Tenant = apartmentid,
                Id_EstateExpense = associationExpenseId,

            };
            GetContext().ApartmentExpenses.Add(te);
            GetContext().SaveChanges();
        }

        public static void UpdateApartmentExpense(int idExpenseEstate, int apartmentid, decimal value)
        {
            ApartmentExpenses result = new ApartmentExpenses();
            result = GetContext().ApartmentExpenses.FirstOrDefault(b => b.Id_EstateExpense == idExpenseEstate && b.Id_Tenant == apartmentid);

            if (result != null)
            {
                result.Value = value;
                GetContext().Entry(result).CurrentValues.SetValues(result);

                GetContext().SaveChanges();
            }
        }

        public static void RemoveApartmentExpense(int apartmentid, int estateExpenseId, int counterOrder)
        {
            ApartmentExpenses apartmentExpenses = GetContext().ApartmentExpenses.FirstOrDefault(tex => tex.Id_EstateExpense == estateExpenseId
                           && tex.Id_Tenant == apartmentid && tex.CounterOrder == counterOrder);
            if (apartmentExpenses != null)
            {
                GetContext().ApartmentExpenses.Remove(apartmentExpenses);
                GetContext().SaveChanges();
            }
        }

        public static void RemoveApartmentExpense(int apartmentid, int estateExpenseId)
        {
            ApartmentExpenses apartmentExpenses = GetContext().ApartmentExpenses.Where(tex => tex.Id_EstateExpense == estateExpenseId
                           && tex.Id_Tenant == apartmentid).FirstOrDefault();
            if (apartmentExpenses != null && apartmentExpenses.Value != null)
            {
                GetContext().ApartmentExpenses.Remove(apartmentExpenses);
                GetContext().SaveChanges();
            }
        }

        public static void AddCotaIndivizaApartmentExpenses(int idAssociationExpense, decimal totalValue)
        {
            AssociationExpenses ee = GetApExById(idAssociationExpense);

            if (ee != null)
            {
                AddCotaIndivizaApartmentExpenses(ee, totalValue, null);
            }
        }

        private static AssociationExpenses GetApExById(int idAssociationExpense)
        {
            return GetContext().AssociationExpenses.FirstOrDefault(e => e.Id == idAssociationExpense);
        }

        public static void AddCotaIndivizaApartmentExpenses(AssociationExpenses associationExpense, decimal? totalValue, int? stairCase)
        {
            // apartments
            List<Apartments> apartments = new List<Apartments>();
            if (stairCase.HasValue)
            {
                apartments = GetContext().Apartments.Where(a => a.id_Estate == associationExpense.Id_Estate && a.Id_StairCase == stairCase).ToList();
            }
            else
            {
                apartments = GetContext().Apartments.Where(a => a.id_Estate == associationExpense.Id_Estate).ToList();
            }

            decimal totalCota = apartments.Sum(te => te.CotaIndiviza.Value);
            decimal? valuePerCotaUnit = null;
            if (totalValue.HasValue && totalCota != 0)
            {
                valuePerCotaUnit = totalValue.Value / totalCota;
            }

            foreach (var apartment in apartments)
            {
                decimal? theValue = null;
                ApartmentExpenses tte = GetContext().ApartmentExpenses.FirstOrDefault(tee => tee.Id_EstateExpense == associationExpense.Id && tee.Id_Tenant == apartment.Id);
                if (valuePerCotaUnit.HasValue && apartment.CotaIndiviza.HasValue)
                {
                    theValue = apartment.CotaIndiviza.Value * valuePerCotaUnit.Value;
                }

                if (tte != null)
                {
                    tte.Value = theValue;

                    GetContext().Entry(tte).CurrentValues.SetValues(tte);
                }
                else
                {
                    ApartmentExpenses te = new ApartmentExpenses
                    {
                        Value = theValue,
                        Id_Tenant = apartment.Id,
                        Id_EstateExpense = associationExpense.Id,

                    };
                    GetContext().ApartmentExpenses.Add(te);
                }

                GetContext().SaveChanges();
            }
        }

        public static void AddPerDependentsExpenses(int idExpenseEstate, decimal? valuePerApartment, List<Apartments> apartments = null)
        {
            var ee = AssociationExpensesManager.GetById(idExpenseEstate);
            if (ee != null)
            {
                List<Apartments> allApartments;

                if (apartments == null)
                {
                    allApartments = ApartmentsManager.Get(ee.Id_Estate);
                }
                else
                {
                    allApartments = apartments;
                }

                foreach (var apartment in allApartments)
                {
                    if (apartment != null)
                    {
                        ApartmentExpenses tte = ApartmentExpensesManager.GetByExpenseEstateIdAndApartmentId(idExpenseEstate, apartment.Id);
                        if (tte != null)
                        {
                            tte.Value = (valuePerApartment.HasValue) ? valuePerApartment * apartment.Dependents : null;
                            GetContext().Entry(tte).CurrentValues.SetValues(tte);
                        }
                        else
                        {
                            ApartmentExpenses te = new ApartmentExpenses
                            {
                                Value = valuePerApartment.HasValue ? valuePerApartment * apartment.Dependents : null,
                                Id_Tenant = apartment.Id,
                                Id_EstateExpense = ee.Id,
                            };
                            GetContext().ApartmentExpenses.Add(te);
                        }

                        GetContext().SaveChanges();
                    }
                }
            }
        }

        public static void AddPerApartmentsExpenses(int idExpenseEstate, decimal? valuePerApartment, List<Apartments> apartments = null)
        {
            var ee = AssociationExpensesManager.GetById(idExpenseEstate);
            if (ee != null)
            {
                foreach (var apartment in apartments)
                {
                    if (apartment != null)
                    {
                        ApartmentExpenses tte = ApartmentExpensesManager.GetByExpenseEstateIdAndApartmentId(idExpenseEstate, apartment.Id);
                        if (tte != null)
                        {
                            tte.Value = valuePerApartment;
                            GetContext().Entry(tte).CurrentValues.SetValues(tte);
                        }
                        else
                        {
                            ApartmentExpenses te = new ApartmentExpenses
                            {
                                Value = valuePerApartment,
                                Id_Tenant = apartment.Id,
                                Id_EstateExpense = ee.Id,
                            };
                            GetContext().ApartmentExpenses.Add(te);
                        }

                        GetContext().SaveChanges();
                    }
                }
            }
        }

        public static void UpdateOldIndexAndValue(ApartmentExpenses apartmentExpense, decimal? oldIndex, decimal? pricePerExpenseUnit)
        {
            ApartmentExpenses result = new ApartmentExpenses();
            result = GetContext().ApartmentExpenses.FirstOrDefault(b => b.Id == apartmentExpense.Id);

            if (result != null)
            {
                result.IndexOld = oldIndex;
                if (result.IndexOld != null && result.IndexNew != null && pricePerExpenseUnit.HasValue)
                {
                    result.Value = (result.IndexNew - result.IndexOld) * pricePerExpenseUnit;
                }
                else
                {
                    result.Value = null;
                }

                PerformUpdate(result);
                GetContext().SaveChanges();
            }
        }

        public static void UpdatePerIndexValue(ApartmentExpenses apartmentExpense, decimal? pricePerExpenseUnit)
        {
            ApartmentExpenses result = new ApartmentExpenses();
            result = GetContext(true).ApartmentExpenses.FirstOrDefault(b => b.Id == apartmentExpense.Id);

            if (result != null)
            {
                if (result.IndexOld != null && result.IndexNew != null && pricePerExpenseUnit.HasValue)
                {
                    result.Value = (result.IndexNew - result.IndexOld) * pricePerExpenseUnit;
                }
                else
                {
                    result.Value = null;
                }

                GetContext().Entry(result).CurrentValues.SetValues(result);
                GetContext().SaveChanges();
            }
        }


        public static void ConfigurePerIndex(AssociationExpenses ae, Apartments apartment)
        {
            var assC = ApartmentCountersManager.GetByApartmentAndExpense(apartment.Id, ae.Id_Expense);
            var unitPrice = UnitPricesManager.GetPrice(ae.Id, apartment.Id_StairCase);

            var apartmentExpenses = ApartmentExpensesManager.GetByExpenseEstateIdAndApartmentIdAll(ae.Id, apartment.Id).ToList();

            if (assC != null && assC.CountersInsideApartment.HasValue)
            {
                // add default ones
                if (apartmentExpenses.Count == 0)
                {
                    for (int i = 0; i < assC.CountersInsideApartment; i++)
                    {
                        ApartmentExpensesManager.AddDefaultApartmentExpense(apartment.Id, ae.Year, ae.Month, ae.Id, i, assC);
                    }
                }
                // in case of adding one more in apartment, add in expense
                else if (apartmentExpenses.Count < assC.CountersInsideApartment.Value)
                {
                    for (int i = apartmentExpenses.Count; i < assC.CountersInsideApartment.Value; i++)
                    {
                        ApartmentExpensesManager.AddDefaultApartmentExpense(apartment.Id, ae.Year, ae.Month, ae.Id, i, assC);
                    }
                }
                // in case of removing one in apartment, remove in expense
                else if (apartmentExpenses.Count > assC.CountersInsideApartment.Value)
                {
                    for (int i = assC.CountersInsideApartment.Value; i < apartmentExpenses.Count; i++)
                    {
                        if (apartmentExpenses[i].CounterOrder.HasValue)
                        {
                            ApartmentExpensesManager.RemoveApartmentExpense(apartment.Id, apartmentExpenses[i].Id_EstateExpense, apartmentExpenses[i].CounterOrder.Value);
                        }
                    }
                }
            }

            for (int i = 0; i < assC.CountersInsideApartment; i++)
            {
                var apartmentExpense = ApartmentExpensesManager.Get(ae.Id, apartment.Id, i);
                var lastMonthIndexApartmentExpense = ApartmentExpensesManager.GetForPreviousMonth(ae.Id, apartment.Id, i);

                if (assC.CountersInsideApartment == 1)
                {
                    if (apartmentExpense == null)
                    {
                        apartmentExpense = ApartmentExpensesManager.Get(ae.Id, apartment.Id, null);
                    }

                    if (lastMonthIndexApartmentExpense == null)
                    {
                        lastMonthIndexApartmentExpense = ApartmentExpensesManager.GetForIndexExpensPreviousMonthApartmentExpense(ae.Id, apartment.Id);
                    }
                }

                if (apartmentExpense != null && apartmentExpense.IndexNew.HasValue && apartmentExpense.IndexOld.HasValue && apartmentExpense.Value.HasValue && unitPrice.HasValue &&
                    (apartmentExpense.IndexNew - apartmentExpense.IndexOld) * unitPrice.Value != apartmentExpense.Value)
                {
                    var theValue = (apartmentExpense.IndexNew - apartmentExpense.IndexOld) * unitPrice.Value;
                    UpdateApartmentExpense(apartmentExpense.Id, theValue);
                }


                if (apartmentExpense == null || lastMonthIndexApartmentExpense == null)
                {
                    continue;
                }

                if (lastMonthIndexApartmentExpense != null && IsMonthClosed(lastMonthIndexApartmentExpense))
                {
                    if (lastMonthIndexApartmentExpense == null && apartmentExpense.IndexOld == null)
                    {
                        ApartmentExpensesManager.UpdateOldIndexAndValue(apartmentExpense, 0, unitPrice);
                    }
                    else if (lastMonthIndexApartmentExpense != null && apartmentExpense != null)
                    {
                        if (lastMonthIndexApartmentExpense.IndexNew != apartmentExpense.IndexOld
                            || !apartmentExpense.IndexOld.HasValue)
                        {
                            ApartmentExpensesManager.UpdateOldIndexAndValue(apartmentExpense, lastMonthIndexApartmentExpense.IndexNew, unitPrice);
                        }
                    }

                    ApartmentExpensesManager.UpdatePerIndexValue(apartmentExpense, unitPrice);
                }
            }

        }

        public static void UpdateNewIndexAndValue(int apartmentExpenseId, int associationExpenseId, decimal? newIndex, bool shouldUpdateOld, decimal? oldIndex = null)
        {
            if (newIndex.HasValue)
            {
                newIndex = Math.Round(newIndex.Value, ConfigConstants.IndexPrecision);
            }

            if (oldIndex.HasValue)
            {
                oldIndex = Math.Round(oldIndex.Value, ConfigConstants.IndexPrecision);
            }

            ApartmentExpenses apartmentExpense = ApartmentExpensesManager.GetById(apartmentExpenseId);

            if (apartmentExpense != null)
            {
                var unitPrice = UnitPricesManager.GetPrice(associationExpenseId, apartmentExpense.Apartments.Id_StairCase);
                apartmentExpense.IndexNew = newIndex;
                if (shouldUpdateOld)
                {
                    apartmentExpense.IndexOld = oldIndex;
                }

                if (apartmentExpense.IndexOld != null && newIndex.HasValue && unitPrice.HasValue)
                {
                    apartmentExpense.Value = (apartmentExpense.IndexNew - apartmentExpense.IndexOld) * unitPrice.Value;
                }
                else
                {
                    apartmentExpense.Value = null;
                }

                PerformUpdate(apartmentExpense);

                //update previeous month old index
                var lastMonthIndexApartmentExpense = ApartmentExpensesManager.GetForPreviousMonth(apartmentExpense.Id_Tenant, apartmentExpenseId);
                if (lastMonthIndexApartmentExpense != null && apartmentExpense.IndexOld != null &&
                    lastMonthIndexApartmentExpense.IndexNew != apartmentExpense.IndexOld &&
                    IsMonthClosed(lastMonthIndexApartmentExpense))
                {
                    UpdateIndexNew(apartmentExpense, lastMonthIndexApartmentExpense);
                }
            }
        }

        private static void PerformUpdate(ApartmentExpenses apartmentExpense)
        {
            GetContext().Entry(apartmentExpense).CurrentValues.SetValues(apartmentExpense);
            GetContext().SaveChanges();
        }

        public static bool IsMonthClosed(ApartmentExpenses lastMonthIndexApartmentExpense)
        {
            return (!lastMonthIndexApartmentExpense.AssociationExpenses.IsClosed.HasValue ||
                                (lastMonthIndexApartmentExpense.AssociationExpenses.IsClosed.HasValue && !lastMonthIndexApartmentExpense.AssociationExpenses.IsClosed.Value));
        }

        public static void UpdateIndexNew(ApartmentExpenses apartmentExpense, ApartmentExpenses lastMonthIndexApartmentExpense)
        {
            lastMonthIndexApartmentExpense.IndexNew = apartmentExpense.IndexOld;
            GetContext().Entry(lastMonthIndexApartmentExpense).CurrentValues.SetValues(lastMonthIndexApartmentExpense);
            GetContext().SaveChanges();
        }

        private static ApartmentExpenses GetForPreviousMonth(int apartmentId, int apartmentExpenseId)
        {
            ApartmentExpenses result = null;

            var apartmentExpense = ApartmentExpensesManager.GetById(apartmentExpenseId);
            if (apartmentExpense != null && apartmentExpense.AssociationExpenses != null)
            {
                result = GetForPreviousMonth(apartmentExpense.AssociationExpenses.Id, apartmentId, apartmentExpense.CounterOrder);
            }

            return result;
        }

        internal static IEnumerable<ApartmentExpenses> GetByExpenseYearAndMonth(int apartmentId, int associationExpenseId)
        {
            return GetContext(true).ApartmentExpenses.Where(te => te.Id_Tenant == apartmentId && te.Id_EstateExpense == associationExpenseId);
        }

        public static void UpdateApartmentExpenses(AssociationExpenses associationExpense, decimal? value, int? stairCase = null)
        {
            if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
            {
                ApartmentExpensesManager.AddCotaIndivizaApartmentExpenses(associationExpense, value, stairCase);
            }
            else if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerNrTenants)
            {
                decimal? valuePerApartment = null;
                List<Apartments> apartments = ApartmentsManager.GetAllThatAreRegisteredWithSpecificCounters(associationExpense.Id_Estate, associationExpense.Id, stairCase);

                var allTenantDependents = apartments.Sum(t => t.Dependents);
                if (value.HasValue && allTenantDependents != 0)
                {
                    valuePerApartment = value.Value / allTenantDependents;
                }

                ApartmentExpensesManager.AddPerDependentsExpenses(associationExpense.Id, valuePerApartment, apartments);
            }
            else if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerApartament)
            {
                var apartments = ApartmentsManager.Get(associationExpense.Id_Estate);
                decimal? valuePerApartment = null;
                if (value.HasValue && apartments.Count != 0)
                {
                    valuePerApartment = value.Value / apartments.Count;
                }

                ApartmentExpensesManager.AddPerApartmentsExpenses(associationExpense.Id, valuePerApartment, apartments);
            }
        }


        public static List<ApartmentExpenses> GetAllExpensesByApartmentAndMonth(int apartmentid, int year, int month)
        {
            var te = GetContext().ApartmentExpenses.Where(e => e.Id_Tenant == apartmentid
                && e.AssociationExpenses.Month == month && e.AssociationExpenses.Year == year)
                .ToListAsync().Result;

            return te;
        }

        public static ApartmentExpenses GetExpenseByApartmentMonth(int apartmentid, int year, int month, object expense)
        {
            var allApartmentExpenses = GetAllExpensesByApartmentAndMonth(apartmentid, year, month);
            var specificExpense = allApartmentExpenses.Where(e => e.AssociationExpenses.Id_Expense == ((Expenses)expense).Id).ToList();
            ApartmentExpenses result = null;

            if (specificExpense != null && specificExpense.Count == 1)
            {
                result = specificExpense[0];
            }
            else
            {
                // log error / cleanup
            }

            return result;
        }

        public static void UpdateApartmentExpense(int apartmentid, int year, int month, object te)
        {
            ApartmentExpenses result = new ApartmentExpenses();
            result = GetContext().ApartmentExpenses.FirstOrDefault(b => b.Id == ((ApartmentExpenses)te).Id);

            if (result != null)
            {
                result.Value = ((ApartmentExpenses)te).Value;
                PerformUpdate(result);
            }
        }

        /// <summary>
        /// usually 1 apartment-expense per month/expense.. 
        /// if multiple (3) counters (CountersInsideApartment) for a specific expense, add 3 apartment-expense
        /// </summary>
        public static void AddDefaultApartmentExpense(int apartmentid, int year, int month, int associationExpenseId, int counterOrder, AssociationCountersApartment assC)
        {
            if (assC != null && assC.CountersInsideApartment >= counterOrder)
            {
                // get last month based on order if 3 cunters, bring 1, after bring 2, bring 3


                decimal? value = null;
                decimal? oldIndex = null;
                var aeFormOld = ApartmentExpensesManager.GetForPreviousMonth(associationExpenseId, apartmentid, counterOrder);
                if (aeFormOld != null)
                {
                    oldIndex = aeFormOld.IndexNew;
                }

                ApartmentExpenses apartmentExpense = new ApartmentExpenses
                {
                    Value = value,
                    Id_Tenant = apartmentid,
                    Id_EstateExpense = associationExpenseId,
                    IndexOld = oldIndex,
                    CounterOrder = counterOrder
                };

                GetContext().ApartmentExpenses.Add(apartmentExpense);
                GetContext().SaveChanges();
            }
        }

        private static ApartmentExpenses GetForPreviousMonth(int idAssociationExpenseCurentMonth, int apartmentid, int? countOrder)
        {
            ApartmentExpenses result = null;
            var ee = AssociationExpensesManager.GetById(idAssociationExpenseCurentMonth);
            if (ee != null)
            {
                //todo -1 does not work for month 1(january)
                var prevMonthAE = AssociationExpensesManager.GetPreviousMonth(ee);

                if (prevMonthAE != null)
                {
                    result = Get(prevMonthAE.Id, apartmentid, countOrder);
                }
            }

            return result;
        }

        public static void AddDefaultApartmentExpenseForIndividual(int apartmentid, int year, int month, int associationExpenseId)
        {
            var associationExpense = GetContext().AssociationExpenses.FirstOrDefault(ee => ee.Id == associationExpenseId);
            // GetAllExpensesByApartmentAndMonth nr OrderedEnumerableRowCollection counters
            // for all the counters
            if (associationExpense != null)
            {
                decimal? value = null;
                decimal? oldIndex = null;
                var te = ApartmentExpensesManager.GetForIndexExpensPreviousMonthApartmentExpense(associationExpenseId, apartmentid);
                if (te != null)
                {
                    if (associationExpense.ExpenseTypes.Id == (int)ExpenseType.PerIndex)
                    {
                        oldIndex = te.IndexNew;
                        //value = te.Value;
                    }
                    else if (associationExpense.ExpenseTypes.Id == (int)ExpenseType.Individual)
                    {
                        value = te.Value;
                    }
                }

                ApartmentExpenses apartmentExpense = new ApartmentExpenses
                {
                    Value = value,
                    Id_Tenant = apartmentid,
                    Id_EstateExpense = associationExpenseId,
                    IndexOld = oldIndex
                };

                GetContext().ApartmentExpenses.Add(apartmentExpense);
                GetContext().SaveChanges();
            }
        }

        public static void AddApartmentExpense(int apartmentid, int year, int month, object theExpense, decimal expenseValue)
        {
            Apartments apartment = GetContext().Apartments.FirstOrDefault(t => t.Id == apartmentid);
            Expenses expense = theExpense as Expenses;

            if (apartment != null)
            {
                AssociationExpenses associationExpense = GetContext().AssociationExpenses.FirstOrDefault(ee => ee.Id_Estate == apartment.id_Estate
                    && ee.Id_Expense == expense.Id && ee.Month == month && ee.Year == year);

                if (associationExpense != null)
                {
                    ApartmentExpenses apartmentExpense = new ApartmentExpenses
                    {
                        Value = expenseValue,
                        Id_Tenant = apartmentid,
                        Id_EstateExpense = associationExpense.Id
                    };

                    GetContext().ApartmentExpenses.Add(apartmentExpense);
                    GetContext().SaveChanges();
                }
            }
        }

        public static DataTable GetMonthlyRaportAsDataTable(int associationId, int year, int month, int? stairCase)
        {
            DataTable dt = new DataTable();
            Dictionary<int, Expense> raportDictionary = new Dictionary<int, Expense>();
            Dictionary<Expense, decimal> totalCol = new Dictionary<Expense, decimal>();
            List<ExpenseReport> expenseReportList = new List<ExpenseReport>();

            var association = AssociationsManager.GetById(associationId);
            if (association == null)
            {
                return dt;
            }

            var associationExpenses = AssociationExpensesManager.GetByMonthAndYearNotDisabled(associationId, year, month).OrderBy(ee => ee.Id_ExpenseType).ToList();
            List<Apartments> apartments;
            if (!stairCase.HasValue)
            {
                apartments = association.Apartments.ToList();
            }
            else
            {
                apartments = ApartmentsManager.Get(association.Id, stairCase.Value);
            }

            apartments = apartments.OrderBy(a => a.Number).ToList();

            IEnumerable<Administratoro.DAL.Invoices> invoices = InvoicesManager.GetDiverseByAssociationYearMonth(associationId, year, month);

            // populate expenses- pre-step
            bool hasDiverse;
            bool hasRoundUpColumn = association.HasRoundUpColumn.HasValue && association.HasRoundUpColumn.Value;
            RaportPopulateExpensesList(raportDictionary, totalCol, expenseReportList, associationExpenses, apartments, association, invoices, out hasDiverse);

            int expensesFieldSize = associationExpenses.Count();
            if (hasDiverse)
            {
                expensesFieldSize++;
            }

            // add headers
            RaportAddHeaders(dt, associationExpenses, hasDiverse, hasRoundUpColumn);

            // add rows
            decimal generalMonthSum = RaportAddRows(dt, raportDictionary, expensesFieldSize, expenseReportList, hasRoundUpColumn);

            raportAddTotalRow(dt, totalCol, generalMonthSum, hasRoundUpColumn);

            return dt;
        }

        private static void RaportPopulateExpensesList(Dictionary<int, Expense> raportDictionary, Dictionary<Expense, decimal> totalCol,
            List<ExpenseReport> expenseReportList, List<AssociationExpenses> associationExpenses, List<Apartments> apartments,
            Associations association, IEnumerable<Invoices> invoices, out bool hasDiverse)
        {
            decimal sumOfIndiviza = ApartmentsManager.GetSumOfIndivizaForAllApartments(association.Id);

            hasDiverse = false;
            foreach (var apartment in apartments)
            {
                decimal? apartmentCotaIndivizaPart = null;

                if (sumOfIndiviza != 0) { apartmentCotaIndivizaPart = apartment.CotaIndiviza.Value / sumOfIndiviza; }

                ExpenseReport expenseReport = new ExpenseReport();
                expenseReport.Ap = apartment.Number.ToString();
                expenseReport.Name = apartment.Name;
                expenseReport.NrPers = apartment.Dependents;
                expenseReport.CotaIndiviza = apartment.CotaIndiviza.HasValue ? apartment.CotaIndiviza.Value.ToString() : string.Empty;

                raportDictionary.Clear();
                int counter = 0;
                foreach (AssociationExpenses associationExpense in associationExpenses)
                {
                    decimal? apartmentExpenseRedistributionValue = null;
                    decimal? rowValue = null;
                    raportDictionary.Add(counter, (Expense)associationExpense.Expenses.Id);
                    counter++;

                    if (!totalCol.ContainsKey((Expense)associationExpense.Expenses.Id))
                    {
                        totalCol.Add((Expense)associationExpense.Expenses.Id, 0.0m);
                    }

                    IEnumerable<ApartmentExpenses> apartmentExpenses = ApartmentExpensesManager.GetByExpenseYearAndMonth(apartment.Id, associationExpense.Id);
                    if (associationExpense.RedistributeType.HasValue)
                    {
                        apartmentExpenseRedistributionValue = RedistributionManager.CalculateRedistributeValueForStairCase(associationExpense.Id, apartment, apartmentExpenses);
                    }
                    rowValue = CalculateRowValue(apartmentExpenses, apartmentExpenseRedistributionValue, associationExpense, apartmentCotaIndivizaPart);

                    AddRowvalueToReturnObject(expenseReport, associationExpense, rowValue);

                    totalCol[(Expense)associationExpense.Expenses.Id] = !rowValue.HasValue || rowValue.Value == 0 ? totalCol[(Expense)associationExpense.Expenses.Id] :
                                totalCol[(Expense)associationExpense.Expenses.Id] + rowValue.Value;
                }

                raportDictionary.Add(counter, Expense.Diverse);

                hasDiverse = RaportPopulateExpensesListDiverse(apartment, invoices, association, totalCol, expenseReport) || hasDiverse;

                expenseReportList.Add(expenseReport);
            }
        }

        private static void AddRowvalueToReturnObject(ExpenseReport expenseReport, AssociationExpenses associationExpense, decimal? rowValue)
        {
            switch ((Expense)associationExpense.Expenses.Id)
            {
                case Expense.ApaCalda:
                    expenseReport.WatherWarm = rowValue;
                    break;
                case Expense.ApaRece:
                    expenseReport.WatherCold = rowValue;
                    break;
                case Expense.Salubritate:
                    expenseReport.Trash = rowValue;
                    break;
                case Expense.Administrator:
                    expenseReport.Administrator = rowValue;
                    break;
                case Expense.Gaz:
                    expenseReport.Gas = rowValue;
                    break;
                case Expense.PersonalServiciu:
                    expenseReport.Cleaning = rowValue;
                    break;
                case Expense.IncalzireRAT:
                    expenseReport.HeatRAT = rowValue;
                    break;
                case Expense.EnergieElectrica:
                    expenseReport.Electricity = rowValue;
                    break;
                case Expense.Lift:
                    expenseReport.Elevator = rowValue;
                    break;
                case Expense.IntretinereInstalatii:
                    expenseReport.Utilities = rowValue;
                    break;
                case Expense.Presedinte:
                    expenseReport.President = rowValue;
                    break;
                case Expense.Cenzor:
                    expenseReport.Censor = rowValue;
                    break;
                case Expense.Fochist:
                    expenseReport.Fireman = rowValue;
                    break;
                case Expense.IntretinereAscensor:
                    expenseReport.ElevatorUtility = rowValue;
                    break;
                case Expense.AjutorÎncălzire:
                    expenseReport.HeatHelp = rowValue;
                    break;
            }
        }

        private static decimal? CalculateRowValue(IEnumerable<ApartmentExpenses> apartmentExpenses, decimal? redistributionValue,
            AssociationExpenses associationExpense, decimal? apartmentCotaIndivizaPart)
        {
            decimal? result = null;

            if (apartmentExpenses != null && apartmentExpenses.Count() != 0)
            {
                result = apartmentExpenses.Where(ae => ae.Value.HasValue).Sum(ae => ae.Value.Value);
            }

            if (redistributionValue.HasValue)
            {
                result = result.HasValue ? result + redistributionValue.Value : redistributionValue.Value;
            }

            if (associationExpense.Id_Expense == (int)Expense.ApaRece && associationExpense.Invoices.Any())
            {
                decimal? meteoSum = null;
                var invoiceSubcategory = InvoicesSubcategoriesManager.GetByInvoiceId(associationExpense.Invoices.FirstOrDefault().Id, (int)InvoiceSubcategoryType.CanalMeteo);
                if (invoiceSubcategory != null && invoiceSubcategory.Value.HasValue && apartmentCotaIndivizaPart.HasValue)
                {
                    meteoSum = apartmentCotaIndivizaPart * invoiceSubcategory.Value.Value;

                    if (meteoSum.HasValue)
                    {
                        result = result.HasValue ? result + meteoSum : meteoSum;
                    }
                }

            }

            result = result.HasValue ? result : 0.0m;

            return result;
        }

        private static bool RaportPopulateExpensesListDiverse(Apartments apartment, IEnumerable<Invoices> invoices, Associations association,
            Dictionary<Expense, decimal> totalCol, ExpenseReport expenseReport)
        {
            bool hasDiverse = false;
            List<Apartments> allAssociationApartments = association.Apartments.ToList();

            decimal? result = null;
            foreach (Invoices invoice in invoices)
            {
                if (!invoice.Value.HasValue)
                {
                    continue;
                }

                if (invoice.id_Redistributiontype == (int)RedistributionType.PerDependents)
                {
                    int? nrOfDependents = null;
                    if (invoice.Id_StairCase.HasValue)
                    {
                        nrOfDependents = allAssociationApartments.Where(t => t.Id_StairCase == invoice.Id_StairCase).Select(i => i.Dependents).Sum();
                    }

                    decimal? valueToAdd = null;
                    if (nrOfDependents.HasValue)
                    {
                        if (nrOfDependents != 0)
                        {
                            valueToAdd = ((invoice.Value.Value * apartment.Dependents) / nrOfDependents.Value);
                        }
                    }
                    else
                    {
                        var allDependents = allAssociationApartments.Select(t => t.Dependents).Sum();

                        if (allDependents != 0)
                        {
                            valueToAdd = ((invoice.Value.Value * apartment.Dependents) / allDependents);
                        }
                    }

                    result = result.HasValue ? (result + valueToAdd) : valueToAdd;

                }
                else if (invoice.id_Redistributiontype == (int)RedistributionType.PerApartament)
                {
                    if (invoice.Id_StairCase.HasValue && !apartment.Id_StairCase.HasValue)
                    {
                        continue;
                    }

                    if (invoice.Id_StairCase.HasValue && apartment.Id_StairCase.HasValue && apartment.Id_StairCase.Value != invoice.Id_StairCase.Value)
                    {
                        continue;
                    }

                    if (allAssociationApartments.Count == 0)
                    {
                        continue;
                    }

                    int? nrApartments = null;
                    if (invoice.Id_StairCase.HasValue)
                    {
                        nrApartments = allAssociationApartments.Where(t => t.Id_StairCase == invoice.Id_StairCase.Value).Count();
                    }
                    else
                    {
                        nrApartments = allAssociationApartments.Count();
                    }

                    decimal? valueToAdd = invoice.Value.Value / nrApartments;
                    result = result.HasValue ? (result + valueToAdd) : valueToAdd;
                }
                else if (invoice.id_Redistributiontype == (int)RedistributionType.PerCotaIndiviza)
                {
                    if (invoice.Id_StairCase.HasValue && !apartment.Id_StairCase.HasValue)
                    {
                        continue;
                    }

                    if (invoice.Id_StairCase.HasValue && apartment.Id_StairCase.HasValue && apartment.Id_StairCase.Value != invoice.Id_StairCase.Value)
                    {
                        continue;
                    }
                    var apartments = allAssociationApartments;
                    if (invoice.Id_StairCase.HasValue)
                    {
                        apartments = allAssociationApartments.Where(t => t.Id_StairCase == invoice.Id_StairCase).ToList();
                    }

                    if (apartments.Count == 0)
                    {
                        continue;
                    }

                    var valueToAdd = RedistributionManager.RedistributeValueCotaIndivizaForSpecificApartments(apartment, invoice, apartments);

                    result = result.HasValue ? (result + valueToAdd) : valueToAdd;
                }
                else if (invoice.id_Redistributiontype == (int)RedistributionType.PerCotaIndiviza)
                {

                }
            }

            if (result.HasValue)
            {
                var addedValue = DecimalExtensions.RoundUp((double)result, 2);
                expenseReport.Diverse = addedValue;
                if (!totalCol.ContainsKey(Expense.Diverse))
                {
                    totalCol.Add(Expense.Diverse, 0.0m);
                }

                totalCol[Expense.Diverse] = totalCol[Expense.Diverse] + addedValue;
                hasDiverse = true;
            }

            return hasDiverse;
        }

        private static void RaportAddHeaders(DataTable dt, List<AssociationExpenses> associationExpenses, bool addDiverse, bool hasRoundUpColumn)
        {
            dt.Columns.Add(new DataColumn("Ap", typeof(string)));
            dt.Columns.Add(new DataColumn("Nume", typeof(string)));
            dt.Columns.Add(new DataColumn("Pers", typeof(string)));
            dt.Columns.Add(new DataColumn("Cota ind.", typeof(string)));

            foreach (var associationExpense in associationExpenses)
            {
                dt.Columns.Add(new DataColumn(associationExpense.Expenses.Name, typeof(string)));
            }

            if (addDiverse)
            {
                dt.Columns.Add(new DataColumn("Diverse", typeof(string)));
            }

            if (hasRoundUpColumn)
            {
                dt.Columns.Add(new DataColumn("Rotunjiri", typeof(string)));
            }
            dt.Columns.Add(new DataColumn("Total lună", typeof(string)));
            dt.Columns.Add(new DataColumn("Fond rulment", typeof(string)));
            dt.Columns.Add(new DataColumn("Fond reparații", typeof(string)));

            dt.Columns.Add(new DataColumn("Penalizări", typeof(string)));
            dt.Columns.Add(new DataColumn("Total general", typeof(string)));



            dt.Columns.Add(new DataColumn("Ap ", typeof(string)));
        }

        private static decimal RaportAddRows(DataTable dt, Dictionary<int, Expense> raportDictionary, int fieldSize, List<ExpenseReport> expenseReportList, bool hasRoundUpColumn)
        {
            decimal generalMonthSum = 0.0m;
            foreach (var raportList in expenseReportList)
            {
                List<object> row = new List<object>();
                decimal monthSum = 0.0m;
                decimal generalSum = 0.0m;
                row.Add(raportList.Ap);
                row.Add(raportList.Name);
                row.Add(raportList.NrPers);
                row.Add(raportList.CotaIndiviza);

                for (int i = 0; i < fieldSize; i++)
                {
                    var item = raportDictionary.FirstOrDefault(r => r.Key == i);
                    string displayedText = string.Empty;

                    if (item.Value != 0)
                    {
                        decimal? result = GetExpenseFromRaportListOnOrder(item.Value, raportList);
                        if (result.HasValue)
                        {
                            if (item.Value == Expense.AjutorÎncălzire)
                            {
                                monthSum = monthSum - result.Value;
                            }
                            else
                            {
                                monthSum = monthSum + result.Value;
                            }
                        }

                        displayedText = result != null ? Math.Round(result.Value, 2).ToString() : string.Empty;
                    }

                    row.Add(displayedText);
                }

                generalMonthSum = generalMonthSum + monthSum;
                monthSum = Math.Round(monthSum, 2);

                // add round coulmn
                if (hasRoundUpColumn)
                {
                    var monthSumBefore = monthSum;
                    monthSum = (DecimalExtensions.RoundUp((double)monthSum, 0));

                    var roundedValue = monthSum - monthSumBefore;
                    row.Add(roundedValue.ToString());
                }

                generalSum = monthSum;
                generalSum = generalSum + 0 + 0 + 0 + 0;

                // add other rows
                row.Add(monthSum.ToString());
                row.Add(string.Empty);
                row.Add(string.Empty);
                row.Add(string.Empty);
                row.Add(generalSum.ToString());
                row.Add(raportList.Ap);

                dt.Rows.Add(row.ToArray());
            }

            return generalMonthSum;
        }

        private static void raportAddTotalRow(DataTable dt, Dictionary<Expense, decimal> totalCol, decimal generalMonthSum, bool hasRoundUpColumn)
        {
            List<object> rowTotal = new List<object>();

            rowTotal.Add(string.Empty);
            rowTotal.Add(string.Empty);
            rowTotal.Add(string.Empty);
            rowTotal.Add(string.Empty);

            foreach (var col in totalCol)
            {
                var value = Math.Round(col.Value, 2);
                rowTotal.Add(value);
            }

            generalMonthSum = Math.Round(generalMonthSum, 2);
            rowTotal.Add(string.Empty);
            if (hasRoundUpColumn)
            {
                generalMonthSum = DecimalExtensions.RoundUp((double)generalMonthSum, 0);
                rowTotal.Add(generalMonthSum.ToString());
            }
            rowTotal.Add(string.Empty);
            rowTotal.Add(string.Empty);
            rowTotal.Add(string.Empty);
            rowTotal.Add(generalMonthSum.ToString());



            rowTotal.Add(string.Empty);

            dt.Rows.Add(rowTotal.ToArray());
        }

        private static decimal? GetExpenseFromRaportListOnOrder(Expense expense, ExpenseReport raportList)
        {
            decimal? result = null;

            switch (expense)
            {
                case Expense.Administrator:
                    result = raportList.Administrator.HasValue ? raportList.Administrator : null;
                    break;
                case Expense.ApaCalda:
                    result = raportList.WatherWarm.HasValue ? raportList.WatherWarm : null;
                    break;
                case Expense.ApaRece:
                    result = raportList.WatherCold.HasValue ? raportList.WatherCold : null;
                    break;
                case Expense.Cenzor:
                    result = raportList.Censor.HasValue ? raportList.Censor : null;
                    break;
                case Expense.EnergieElectrica:
                    result = raportList.Electricity.HasValue ? raportList.Electricity : null;
                    break;
                case Expense.Fochist:
                    result = raportList.Fireman.HasValue ? raportList.Fireman : null;
                    break;
                case Expense.Gaz:
                    result = raportList.Gas.HasValue ? raportList.Gas : null;
                    break;
                case Expense.IncalzireRAT:
                    result = raportList.HeatRAT.HasValue ? raportList.HeatRAT : null;
                    break;
                case Expense.IntretinereInstalatii:
                    result = raportList.Utilities.HasValue ? raportList.Utilities : null;
                    break;
                case Expense.Lift:
                    result = raportList.Elevator.HasValue ? raportList.Elevator : null;
                    break;
                case Expense.PersonalServiciu:
                    result = raportList.Cleaning.HasValue ? raportList.Cleaning : null;
                    break;
                case Expense.Presedinte:
                    result = raportList.President.HasValue ? raportList.President : null;
                    break;
                case Expense.Salubritate:
                    result = raportList.Trash.HasValue ? raportList.Trash : null;
                    break;
                case Expense.Diverse:
                    result = raportList.Diverse.HasValue ? raportList.Diverse : null;
                    break;
                case Expense.AjutorÎncălzire:
                    result = raportList.HeatHelp.HasValue ? raportList.HeatHelp : null;
                    break;
            }

            return result;
        }

        public static void ConfigureIndividual(AssociationExpenses ae, Apartments apartment)
        {
            var apartmentExpense = ApartmentExpensesManager.GetByExpenseEstateIdAndApartmentId(ae.Id, apartment.Id);

            if (apartmentExpense == null)
            {
                ApartmentExpensesManager.AddDefaultApartmentExpenseForIndividual(apartment.Id, ae.Year, ae.Month, ae.Id);
            }
        }

        public static void UpdateApartmentExpense(int apartmentExpenseId, decimal? newValue)
        {
            ApartmentExpenses result = new ApartmentExpenses();
            result = GetContext().ApartmentExpenses.FirstOrDefault(b => b.Id == apartmentExpenseId);

            if (result != null)
            {
                result.Value = newValue;
                GetContext().Entry(result).CurrentValues.SetValues(result);

                GetContext().SaveChanges();
            }
        }

        internal static void UpdateValueForPriceUpdate(int associationExpenseId, decimal? newPricePerUnit)
        {
            IEnumerable<ApartmentExpenses> apartmentExpenses = GetApartmentsExpenseByAssociationExpenseId(associationExpenseId).ToList();

            foreach (var apartmentExpense in apartmentExpenses)
            {
                decimal? newValue = null;
                decimal? consumption = null;

                if (apartmentExpense.IndexOld.HasValue && apartmentExpense.IndexNew.HasValue)
                {
                    consumption = apartmentExpense.IndexNew.Value - apartmentExpense.IndexOld.Value;
                }

                if (consumption.HasValue && newPricePerUnit.HasValue)
                {
                    newValue = consumption.Value * newPricePerUnit.Value;
                }

                UpdateApartmentExpense(apartmentExpense.Id, newValue);
            }
        }

        public static IEnumerable<ApartmentExpenses> GetApartmentsExpenseByAssociationExpenseId(int associationExpenseId)
        {
            return GetContext(true).ApartmentExpenses.Where(te => te.Id_EstateExpense == associationExpenseId);
        }

        public static decimal? GetConsumption(AssociationExpenses associationExpense, int? stairCase)
        {
            decimal? result = null;

            var assCounter = AssociationCountersManager.GetByExpenseAndStairCase(associationExpense, stairCase);
            if (assCounter != null)
            {
                var apWithSpecificCounter = GetApartmentsByCounter(assCounter).Select(a => a.Id);
                var sumOfIndexes = associationExpense.ApartmentExpenses.Where(tex => apWithSpecificCounter.Contains(tex.Apartments.Id)).Sum(t => t.IndexNew - t.IndexOld);
                if (sumOfIndexes.HasValue)
                {
                    var pricePerUnit = UnitPricesManager.GetPrice(associationExpense.Id, stairCase);
                    if (pricePerUnit.HasValue)
                    {
                        result = sumOfIndexes * pricePerUnit.Value;
                    }
                }
            }

            return result;
        }


        public static IEnumerable<Apartments> GetApartmentsByCounter(AssociationCounters associationCounter)
        {
            var result = Enumerable.Empty<Apartments>();

            if (associationCounter != null)
            {
                var allStairCases = associationCounter.AssociationCountersStairCase.Select(ass => ass.Id_StairCase).ToList();

                List<Apartments> allAps = new List<Apartments>();
                foreach (AssociationCountersStairCase associationCountersStairCase in associationCounter.AssociationCountersStairCase)
                {
                    foreach (AssociationCountersApartment associationCountersApartment in associationCountersStairCase.AssociationCounters.AssociationCountersApartment)
                    {
                        if (allStairCases.Contains(null) || allStairCases.Contains(associationCountersApartment.Apartments.Id_StairCase))
                        {
                            allAps.Add(associationCountersApartment.Apartments);
                        }
                    }

                }

                result = allAps.Distinct();
            }

            return result;
        }

        public static IEnumerable<Apartments> GetApartmentsOnSameCounter(int associationExpenseId, int? stairCase)
        {
            var result = Enumerable.Empty<Apartments>();
            var associationExpense = AssociationExpensesManager.GetById(associationExpenseId);

            if (associationExpense != null)
            {
                var associationCounter = AssociationCountersManager.GetByExpenseAndStairCase(associationExpense, stairCase);

                if (associationCounter != null)
                {
                    result = GetApartmentsByCounter(associationCounter);
                }
            }

            return result;
        }

        private static AssociationCounters GetAsCtById(int id)
        {
            return GetContext().AssociationCounters.FirstOrDefault(e => e.Id == id);
        }


        public static void ConfigurePerIndex(AssociationExpenses ee, List<Apartments> apartments)
        {
            foreach (var apartment in apartments)
            {
                ApartmentExpensesManager.ConfigurePerIndex(ee, apartment);
            }
        }
    }
}
