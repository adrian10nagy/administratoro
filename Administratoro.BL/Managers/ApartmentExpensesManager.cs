using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Administratoro.BL.Constants;
using Administratoro.BL.Extensions;
using Administratoro.BL.Models;
using Administratoro.DAL;

namespace Administratoro.BL.Managers
{
    public static class ApartmentExpensesManager
    {
        private static AdministratoroEntities _administratoroEntities;

        #region Get
        private static AdministratoroEntities GetContext(bool shouldRefresh = false)
        {
            if (_administratoroEntities == null || shouldRefresh)
                _administratoroEntities = new AdministratoroEntities();

            return _administratoroEntities;
        }

        internal static ApartmentExpenses GetForIndexExpensPreviousMonthApartmentExpense(int idExpenseEstateCurentMonth,
            int idApartment)
        {
            ApartmentExpenses result = null;
            var ee = AssociationExpensesManager.GetById(idExpenseEstateCurentMonth);
            if (ee != null)
            {
                var prevMonthAe = AssociationExpensesManager.GetPreviousMonth(ee);

                if (prevMonthAe != null) result = GetByExpenseEstateIdAndApartmentId(prevMonthAe.Id, idApartment);
            }

            return result;
        }

        internal static ApartmentExpenses GetByExpenseEstateIdAndApartmentId(int idExpenseEstate, int idApartment)
        {
            return GetContext().ApartmentExpenses
                .FirstOrDefault(te => te.Id_EstateExpense == idExpenseEstate && te.Id_Tenant == idApartment);
        }

        internal static IEnumerable<ApartmentExpenses> GetByExpenseEstateIdAndApartmentIdAll(int idExpenseEstate,
            int idApartment)
        {
            return GetContext().ApartmentExpenses
                .Where(te => te.Id_EstateExpense == idExpenseEstate && te.Id_Tenant == idApartment);
        }

        internal static ApartmentExpenses Get(int idExpenseEstate, int idApartment, int? countOrder)
        {
            return GetContext().ApartmentExpenses.FirstOrDefault(te => te.Id_EstateExpense == idExpenseEstate
                                                                       && te.Id_Tenant == idApartment
                                                                       && te.CounterOrder == countOrder);
        }

        private static ApartmentExpenses GetById(int apartmentExpenseId)
        {
            return GetContext().ApartmentExpenses.FirstOrDefault(te => te.Id == apartmentExpenseId);
        }

        internal static decimal? GetSumOfIndexesOnSameCounter(int associationExpenseId, int? stairCase)
        {
            var apps = GetApartmentsOnSameCounter(associationExpenseId, stairCase).Select(a => a.Id);

            return GetContext().ApartmentExpenses
                .Where(ae => ae.Id_EstateExpense == associationExpenseId && apps.Contains(ae.Apartments.Id))
                .Sum(s => s.IndexNew - s.IndexOld);
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
                case Expense.IncalzireRat:
                    result = raportList.HeatRat.HasValue ? raportList.HeatRat : null;
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

        public static IEnumerable<ApartmentExpenses> GetPerMonthYear(int year, int month, int apartmentId)
        {
            return GetContext(true).ApartmentExpenses.Where(ae => ae.Id_Tenant == apartmentId &&
                ae.AssociationExpenses.Month == month && ae.AssociationExpenses.Year == year);
        }

        #endregion

        internal static void RemoveApartmentExpense(int apartmentid, int estateExpenseId, int counterOrder)
        {
            var apartmentExpenses = GetContext().ApartmentExpenses.FirstOrDefault(tex =>
                tex.Id_EstateExpense == estateExpenseId
                && tex.Id_Tenant == apartmentid && tex.CounterOrder == counterOrder);
            if (apartmentExpenses != null)
            {
                GetContext().ApartmentExpenses.Remove(apartmentExpenses);
                GetContext().SaveChanges();
            }
        }

        internal static void AddCotaIndivizaApartmentExpenses(IEnumerable<Apartments> apartments,
            AssociationExpenses associationExpense, decimal? totalValue)
        {
            var totalCota = apartments.Where(a => a.CotaIndiviza.HasValue).Sum(te => te.CotaIndiviza.Value);

            decimal? valuePerCotaUnit = null;
            if (totalValue.HasValue && totalCota != 0) valuePerCotaUnit = totalValue.Value / totalCota;

            foreach (var apartment in apartments)
            {
                decimal? theValue = null;
                var tte = GetContext().ApartmentExpenses.FirstOrDefault(tee =>
                    tee.Id_EstateExpense == associationExpense.Id && tee.Id_Tenant == apartment.Id);
                if (valuePerCotaUnit.HasValue && apartment.CotaIndiviza.HasValue)
                    theValue = apartment.CotaIndiviza.Value * valuePerCotaUnit.Value;

                if (tte != null)
                {
                    tte.Value = theValue;

                    GetContext().Entry(tte).CurrentValues.SetValues(tte);
                }
                else
                {
                    var te = new ApartmentExpenses
                    {
                        Value = theValue,
                        Id_Tenant = apartment.Id,
                        Id_EstateExpense = associationExpense.Id
                    };
                    GetContext().ApartmentExpenses.Add(te);
                }

                GetContext().SaveChanges();
            }
        }

        internal static void AddPerDependentsExpenses(int idExpenseEstate, decimal? valuePerApartment,
            IEnumerable<Apartments> apartments = null)
        {
            var ee = AssociationExpensesManager.GetById(idExpenseEstate);
            if (ee == null) return;

            var allApartments = apartments;

            if (apartments == null) allApartments = ApartmentsManager.Get(ee.Id_Estate);

            foreach (var apartment in allApartments)
            {
                if (apartment == null) { return; }

                var tte = GetByExpenseEstateIdAndApartmentId(idExpenseEstate, apartment.Id);
                if (tte != null)
                {
                    tte.Value = valuePerApartment.HasValue ? valuePerApartment * apartment.Dependents : null;
                    GetContext().Entry(tte).CurrentValues.SetValues(tte);
                }
                else
                {
                    var te = new ApartmentExpenses
                    {
                        Value = valuePerApartment.HasValue ? valuePerApartment * apartment.Dependents : null,
                        Id_Tenant = apartment.Id,
                        Id_EstateExpense = ee.Id
                    };
                    GetContext().ApartmentExpenses.Add(te);
                }

                GetContext().SaveChanges();
            }
        }

        internal static void AddPerApartmentsExpenses(int idExpenseEstate, decimal? valuePerApartment,
            IEnumerable<Apartments> apartments = null)
        {
            var ee = AssociationExpensesManager.GetById(idExpenseEstate);
            if (ee != null && apartments != null)
                foreach (var apartment in apartments)
                    if (apartment != null)
                    {
                        var tte = GetByExpenseEstateIdAndApartmentId(idExpenseEstate, apartment.Id);
                        if (tte != null)
                        {
                            tte.Value = valuePerApartment;
                            GetContext().Entry(tte).CurrentValues.SetValues(tte);
                        }
                        else
                        {
                            var te = new ApartmentExpenses
                            {
                                Value = valuePerApartment,
                                Id_Tenant = apartment.Id,
                                Id_EstateExpense = ee.Id
                            };
                            GetContext().ApartmentExpenses.Add(te);
                        }

                        GetContext().SaveChanges();
                    }
        }

        internal static void UpdateOldIndexAndValue(ApartmentExpenses apartmentExpense, decimal? oldIndex,
            decimal? pricePerExpenseUnit)
        {
            ApartmentExpenses result = GetContext().ApartmentExpenses.FirstOrDefault(b => b.Id == apartmentExpense.Id);

            if (result != null)
            {
                result.IndexOld = oldIndex;
                if (result.IndexOld != null && result.IndexNew != null && pricePerExpenseUnit.HasValue)
                    result.Value = (result.IndexNew - result.IndexOld) * pricePerExpenseUnit;
                else
                    result.Value = null;

                PerformUpdate(result);
                GetContext().SaveChanges();
            }
        }

        internal static void UpdatePerIndexValue(ApartmentExpenses apartmentExpense, decimal? pricePerExpenseUnit)
        {
            var result = GetContext(true).ApartmentExpenses.FirstOrDefault(b => b.Id == apartmentExpense.Id);

            if (result != null)
            {
                if (result.IndexOld != null && result.IndexNew != null && pricePerExpenseUnit.HasValue)
                    result.Value = (result.IndexNew - result.IndexOld) * pricePerExpenseUnit;
                else
                    result.Value = null;

                GetContext().Entry(result).CurrentValues.SetValues(result);
                GetContext().SaveChanges();
            }
        }

        public static void ConfigurePerIndex(AssociationExpenses ae, Apartments apartment)
        {
            var assC = ApartmentCountersManager.GetByApartmentAndExpense(apartment.Id, ae.Id_Expense);
            var unitPrice = UnitPricesManager.GetPrice(ae.Id, apartment.Id_StairCase);

            var apartmentExpenses = GetByExpenseEstateIdAndApartmentIdAll(ae.Id, apartment.Id).ToList();

            if (assC != null && assC.CountersInsideApartment.HasValue)
            {
                // add default ones
                if (apartmentExpenses.Count == 0)
                    for (var i = 0; i < assC.CountersInsideApartment; i++)
                        AddDefaultApartmentExpense(apartment.Id, ae.Id, i, assC);
                // in case of adding one more in apartment, add in expense
                else if (apartmentExpenses.Count < assC.CountersInsideApartment.Value)
                    for (var i = apartmentExpenses.Count; i < assC.CountersInsideApartment.Value; i++)
                        AddDefaultApartmentExpense(apartment.Id, ae.Id, i, assC);
                // in case of removing one in apartment, remove in expense
                else if (apartmentExpenses.Count > assC.CountersInsideApartment.Value)
                    for (var i = assC.CountersInsideApartment.Value; i < apartmentExpenses.Count; i++)
                        if (apartmentExpenses[i].CounterOrder.HasValue)
                            RemoveApartmentExpense(apartment.Id, apartmentExpenses[i].Id_EstateExpense,
                                apartmentExpenses[i].CounterOrder.Value);
            }

            if (assC == null) return;

            for (var i = 0; i < assC.CountersInsideApartment; i++)
            {
                var apartmentExpense = Get(ae.Id, apartment.Id, i);
                var lastMonthIndexApartmentExpense = GetForPreviousMonth(ae.Id, apartment.Id, i);

                if (assC.CountersInsideApartment == 1)
                {
                    if (apartmentExpense == null) apartmentExpense = Get(ae.Id, apartment.Id, null);

                    if (lastMonthIndexApartmentExpense == null)
                        lastMonthIndexApartmentExpense =
                            GetForIndexExpensPreviousMonthApartmentExpense(ae.Id, apartment.Id);
                }

                if (apartmentExpense != null && apartmentExpense.IndexNew.HasValue &&
                    apartmentExpense.IndexOld.HasValue && apartmentExpense.Value.HasValue && unitPrice.HasValue &&
                    (apartmentExpense.IndexNew - apartmentExpense.IndexOld) * unitPrice.Value !=
                    apartmentExpense.Value)
                {
                    var theValue = (apartmentExpense.IndexNew - apartmentExpense.IndexOld) * unitPrice.Value;
                    UpdateApartmentExpense(apartmentExpense.Id, theValue);
                }


                if (apartmentExpense == null || lastMonthIndexApartmentExpense == null) continue;

                if (IsMonthClosed(lastMonthIndexApartmentExpense))
                {
                    if (lastMonthIndexApartmentExpense.IndexNew != apartmentExpense.IndexOld
                        || !apartmentExpense.IndexOld.HasValue)
                        UpdateOldIndexAndValue(apartmentExpense, lastMonthIndexApartmentExpense.IndexNew,
                            unitPrice);

                    UpdatePerIndexValue(apartmentExpense, unitPrice);
                }
            }
        }

        public static void UpdateNewIndexAndValue(int apartmentExpenseId, int associationExpenseId, decimal? newIndex,
            bool shouldUpdateOld, decimal? oldIndex = null)
        {
            if (newIndex.HasValue) newIndex = Math.Round(newIndex.Value, ConfigConstants.IndexPrecision);

            if (oldIndex.HasValue) oldIndex = Math.Round(oldIndex.Value, ConfigConstants.IndexPrecision);

            var apartmentExpense = GetById(apartmentExpenseId);

            if (apartmentExpense != null)
            {
                var unitPrice =
                    UnitPricesManager.GetPrice(associationExpenseId, apartmentExpense.Apartments.Id_StairCase);
                apartmentExpense.IndexNew = newIndex;
                if (shouldUpdateOld) apartmentExpense.IndexOld = oldIndex;

                if (apartmentExpense.IndexOld != null && newIndex.HasValue && unitPrice.HasValue)
                    apartmentExpense.Value = (apartmentExpense.IndexNew - apartmentExpense.IndexOld) * unitPrice.Value;
                else
                    apartmentExpense.Value = null;

                PerformUpdate(apartmentExpense);

                //update previeous month old index
                var lastMonthIndexApartmentExpense =
                    GetForPreviousMonth(apartmentExpense.Id_Tenant, apartmentExpenseId);
                if (lastMonthIndexApartmentExpense != null && apartmentExpense.IndexOld != null &&
                    lastMonthIndexApartmentExpense.IndexNew != apartmentExpense.IndexOld &&
                    IsMonthClosed(lastMonthIndexApartmentExpense))
                    UpdateIndexNew(apartmentExpense, lastMonthIndexApartmentExpense);
            }
        }

        internal static void PerformUpdate(ApartmentExpenses apartmentExpense)
        {
            GetContext().Entry(apartmentExpense).CurrentValues.SetValues(apartmentExpense);
            GetContext().SaveChanges();
        }

        internal static bool IsMonthClosed(ApartmentExpenses lastMonthIndexApartmentExpense)
        {
            return !(lastMonthIndexApartmentExpense.AssociationExpenses.IsClosed.HasValue &&
                     lastMonthIndexApartmentExpense.AssociationExpenses.IsClosed.Value);
        }

        internal static void UpdateIndexNew(ApartmentExpenses apartmentExpense,
            ApartmentExpenses lastMonthIndexApartmentExpense)
        {
            lastMonthIndexApartmentExpense.IndexNew = apartmentExpense.IndexOld;
            GetContext().Entry(lastMonthIndexApartmentExpense).CurrentValues.SetValues(lastMonthIndexApartmentExpense);
            GetContext().SaveChanges();
        }

        internal static ApartmentExpenses GetForPreviousMonth(int apartmentId, int apartmentExpenseId)
        {
            ApartmentExpenses result = null;

            var apartmentExpense = GetById(apartmentExpenseId);
            if (apartmentExpense != null && apartmentExpense.AssociationExpenses != null)
                result = GetForPreviousMonth(apartmentExpense.AssociationExpenses.Id, apartmentId,
                    apartmentExpense.CounterOrder);

            return result;
        }

        internal static IEnumerable<ApartmentExpenses> GetByExpenseYearAndMonth(int apartmentId,
            int associationExpenseId)
        {
            return GetContext(true).ApartmentExpenses.Where(te =>
                te.Id_Tenant == apartmentId && te.Id_EstateExpense == associationExpenseId);
        }

        internal static void UpdateApartmentExpenses(AssociationExpenses associationExpense, decimal? value,
            int? stairCase = null, int? idAssCounter = null)
        {
            if (associationExpense.SplitPerStairCase.HasValue && associationExpense.SplitPerStairCase.Value)
            {
                if (idAssCounter.HasValue)
                {
                    var apartments = GetApartmentsByCounter(idAssCounter.Value);
                    if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
                        AddCotaIndivizaApartmentExpenses(apartments, associationExpense, value);
                    else if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerNrTenants)
                        UpdateApartmentExpensePerTenants(associationExpense, value, apartments);
                    else if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerApartament)
                        UpdateApartmentExpensePerApartment(associationExpense, value, apartments);
                }
            }
            else
            {
                if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
                {
                    // apartments
                    var apartments = new List<Apartments>();
                    if (stairCase.HasValue)
                        apartments = GetContext().Apartments.Where(a =>
                            a.id_Estate == associationExpense.Id_Estate && a.Id_StairCase == stairCase).ToList();
                    else
                        apartments = GetContext().Apartments.Where(a => a.id_Estate == associationExpense.Id_Estate)
                            .ToList();

                    AddCotaIndivizaApartmentExpenses(apartments, associationExpense, value);
                }
                else if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerNrTenants)
                { 
                    //var apartments =
                    //    ApartmentsManager.GetAllThatAreRegisteredWithSpecificCounters(associationExpense.Id_Estate,
                    //        associationExpense.Id, stairCase);
                    var apartments = ApartmentsManager.Get(associationExpense.Id_Estate);


                    UpdateApartmentExpensePerTenants(associationExpense, value, apartments);
                }
                else if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerApartament)
                {
                    var apartments = ApartmentsManager.Get(associationExpense.Id_Estate);
                    UpdateApartmentExpensePerApartment(associationExpense, value, apartments);
                }
            }
        }

        private static void UpdateApartmentExpensePerTenants(AssociationExpenses associationExpense, decimal? value,
            IEnumerable<Apartments> apartments)
        {
            decimal? valuePerApartment = null;
            var allTenantDependents = apartments.Sum(t => t.Dependents);
            if (value.HasValue && allTenantDependents != 0) valuePerApartment = value.Value / allTenantDependents;

            AddPerDependentsExpenses(associationExpense.Id, valuePerApartment, apartments);
        }

        private static void UpdateApartmentExpensePerApartment(AssociationExpenses associationExpense, decimal? value,
            IEnumerable<Apartments> apartments)
        {
            decimal? valuePerApartment = null;
            if (value.HasValue && apartments.Count() != 0) valuePerApartment = value.Value / apartments.Count();

            AddPerApartmentsExpenses(associationExpense.Id, valuePerApartment, apartments);
        }

        /// <summary>
        ///     usually 1 apartment-expense per month/expense..
        ///     if multiple (3) counters (CountersInsideApartment) for a specific expense, add 3 apartment-expense
        /// </summary>
        internal static void AddDefaultApartmentExpense(int apartmentid, int associationExpenseId, int counterOrder,
            AssociationCountersApartment assC)
        {
            if (assC != null && assC.CountersInsideApartment >= counterOrder)
            {
                // get last month based on order if 3 cunters, bring 1, after bring 2, bring 3

                decimal? oldIndex = null;
                var aeFormOld = GetForPreviousMonth(associationExpenseId, apartmentid, counterOrder);
                if (aeFormOld != null) oldIndex = aeFormOld.IndexNew;

                var apartmentExpense = new ApartmentExpenses
                {
                    Value = null,
                    Id_Tenant = apartmentid,
                    Id_EstateExpense = associationExpenseId,
                    IndexOld = oldIndex,
                    CounterOrder = counterOrder
                };

                GetContext().ApartmentExpenses.Add(apartmentExpense);
                GetContext().SaveChanges();
            }
        }

        private static ApartmentExpenses GetForPreviousMonth(int idAssociationExpenseCurentMonth, int apartmentid,
            int? countOrder)
        {
            ApartmentExpenses result = null;
            var ee = AssociationExpensesManager.GetById(idAssociationExpenseCurentMonth);
            if (ee != null)
            {
                //todo -1 does not work for month 1(january)
                var prevMonthAe = AssociationExpensesManager.GetPreviousMonth(ee);

                if (prevMonthAe != null) result = Get(prevMonthAe.Id, apartmentid, countOrder);
            }

            return result;
        }

        internal static void AddDefaultApartmentExpenseForIndividual(int apartmentid, int associationExpenseId)
        {
            var associationExpense =
                GetContext().AssociationExpenses.FirstOrDefault(ee => ee.Id == associationExpenseId);
            // GetAllExpensesByApartmentAndMonth nr OrderedEnumerableRowCollection counters
            // for all the counters
            if (associationExpense != null)
            {
                decimal? value = null;
                decimal? oldIndex = null;
                var te = GetForIndexExpensPreviousMonthApartmentExpense(associationExpenseId, apartmentid);
                if (te != null)
                {
                    if (associationExpense.ExpenseTypes.Id == (int)ExpenseType.PerIndex)
                        oldIndex = te.IndexNew;
                    else if (associationExpense.ExpenseTypes.Id == (int)ExpenseType.Individual) value = te.Value;
                }

                var apartmentExpense = new ApartmentExpenses
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

        public static DataTable GetMonthlyRaportAsDataTable(int associationId, int year, int month, int? stairCase)
        {
            var dt = new DataTable();
            var raportDictionary = new Dictionary<int, Expense>();
            var totalCol = new Dictionary<Expense, decimal>();
            var expenseReportList = new List<ExpenseReport>();

            var association = AssociationsManager.GetById(associationId);
            if (association == null) return dt;

            var associationExpenses = AssociationExpensesManager
                .GetByMonthAndYearNotDisabled(associationId, year, month).OrderBy(ee => ee.Id_ExpenseType).ToList();
            List<Apartments> apartments;
            if (!stairCase.HasValue)
                apartments = association.Apartments.ToList();
            else
                apartments = ApartmentsManager.Get(association.Id, stairCase.Value);

            apartments = apartments.OrderBy(a => a.Number).ToList();

            var invoices = InvoicesManager.GetDiverseByAssociationYearMonth(associationId, year, month);

            // populate expenses- pre-step
            bool hasDiverse;
            var hasRoundUpColumn = association.HasRoundUpColumn.HasValue && association.HasRoundUpColumn.Value;
            RaportPopulateExpensesList(raportDictionary, totalCol, expenseReportList, associationExpenses, apartments,
                association, invoices, out hasDiverse);

            var expensesFieldSize = associationExpenses.Count();
            if (hasDiverse) expensesFieldSize++;

            // add headers
            RaportAddHeaders(dt, associationExpenses, hasDiverse, hasRoundUpColumn);

            // add rows
            var generalMonthSum = RaportAddRows(dt, raportDictionary, expensesFieldSize, expenseReportList,
                hasRoundUpColumn);

            raportAddTotalRow(dt, totalCol, generalMonthSum, hasRoundUpColumn);

            return dt;
        }

        private static void RaportPopulateExpensesList(Dictionary<int, Expense> raportDictionary,
            Dictionary<Expense, decimal> totalCol,
            List<ExpenseReport> expenseReportList, List<AssociationExpenses> associationExpenses,
            List<Apartments> apartments,
            Associations association, IEnumerable<Invoices> invoices, out bool hasDiverse)
        {
            var sumOfIndiviza = ApartmentsManager.GetSumOfIndivizaForAllApartments(association.Id);

            hasDiverse = false;
            foreach (var apartment in apartments)
            {
                decimal? apartmentCotaIndivizaPart = null;

                if (sumOfIndiviza != 0 && apartment.CotaIndiviza.HasValue)
                    apartmentCotaIndivizaPart = apartment.CotaIndiviza.Value / sumOfIndiviza;

                var expenseReport = new ExpenseReport
                {
                    Ap = apartment.Number.ToString(),
                    Name = apartment.Name,
                    NrPers = apartment.Dependents,
                    CotaIndiviza = apartment.CotaIndiviza.HasValue
                        ? apartment.CotaIndiviza.Value.ToString(CultureInfo.InvariantCulture)
                        : string.Empty
                };

                raportDictionary.Clear();
                var counter = 0;
                foreach (var associationExpense in associationExpenses)
                {
                    decimal? rowValue = null;
                    raportDictionary.Add(counter, (Expense)associationExpense.Expenses.Id);
                    counter++;

                    if (!totalCol.ContainsKey((Expense)associationExpense.Expenses.Id))
                        totalCol.Add((Expense)associationExpense.Expenses.Id, 0.0m);

                    var apartmentExpenses = GetByExpenseYearAndMonth(apartment.Id, associationExpense.Id).ToList();
                    var apartmentExpenseRedistributionValue =
                        RedistributionManager.CalculateRedistributeValueForStairCase(associationExpense.Id, apartment,
                            apartmentExpenses);
                    rowValue = CalculateRowValue(apartmentExpenses, apartmentExpenseRedistributionValue,
                        associationExpense, apartmentCotaIndivizaPart);

                    AddRowvalueToReturnObject(expenseReport, associationExpense, rowValue);

                    totalCol[(Expense)associationExpense.Expenses.Id] = !rowValue.HasValue || rowValue.Value == 0
                        ? totalCol[(Expense)associationExpense.Expenses.Id]
                        : totalCol[(Expense)associationExpense.Expenses.Id] + rowValue.Value;
                }

                raportDictionary.Add(counter, Expense.Diverse);

                hasDiverse =
                    RaportPopulateExpensesListDiverse(apartment, invoices, association, totalCol, expenseReport) ||
                    hasDiverse;

                expenseReportList.Add(expenseReport);
            }
        }

        private static void AddRowvalueToReturnObject(ExpenseReport expenseReport,
            AssociationExpenses associationExpense, decimal? rowValue)
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
                case Expense.IncalzireRat:
                    expenseReport.HeatRat = rowValue;
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

        private static decimal? CalculateRowValue(List<ApartmentExpenses> apartmentExpenses,
            decimal? redistributionValue,
            AssociationExpenses associationExpense, decimal? apartmentCotaIndivizaPart)
        {
            decimal? result = null;

            if (apartmentExpenses != null && apartmentExpenses.Any())
                result = apartmentExpenses.Where(ae => ae.Value.HasValue).Sum(ae => ae.Value.Value);

            if (redistributionValue.HasValue)
                result = result.HasValue ? result + redistributionValue.Value : redistributionValue.Value;

            if (associationExpense.Id_Expense == (int)Expense.ApaRece && associationExpense.Invoices.Any())
            {
                var invoiceSubcategory = InvoicesSubcategoriesManager.GetByInvoiceId(
                    associationExpense.Invoices.FirstOrDefault().Id, (int)InvoiceSubcategoryType.CanalMeteo);
                if (invoiceSubcategory != null && invoiceSubcategory.Value.HasValue &&
                    apartmentCotaIndivizaPart.HasValue)
                {
                    var meteoSum = apartmentCotaIndivizaPart * invoiceSubcategory.Value.Value;

                    result = result.HasValue ? result + meteoSum : meteoSum;
                }
            }

            result = result.HasValue ? result : 0.0m;

            return result;
        }

        private static bool RaportPopulateExpensesListDiverse(Apartments apartment, IEnumerable<Invoices> invoices,
            Associations association,
            Dictionary<Expense, decimal> totalCol, ExpenseReport expenseReport)
        {
            var hasDiverse = false;
            var allAssociationApartments = association.Apartments.ToList();

            decimal? result = null;
            foreach (var invoice in invoices)
            {
                if (!invoice.Value.HasValue) continue;

                if (invoice.id_Redistributiontype == (int)RedistributionType.PerDependents)
                {
                    int? nrOfDependents = null;
                    if (invoice.Id_StairCase.HasValue)
                        nrOfDependents = allAssociationApartments.Where(t => t.Id_StairCase == invoice.Id_StairCase)
                            .Select(i => i.Dependents).Sum();

                    decimal? valueToAdd = null;
                    if (nrOfDependents.HasValue)
                    {
                        if (nrOfDependents != 0)
                            valueToAdd = invoice.Value.Value * apartment.Dependents / nrOfDependents.Value;
                    }
                    else
                    {
                        var allDependents = allAssociationApartments.Select(t => t.Dependents).Sum();

                        if (allDependents != 0) valueToAdd = invoice.Value.Value * apartment.Dependents / allDependents;
                    }

                    result = result.HasValue ? result + valueToAdd : valueToAdd;
                }
                else if (invoice.id_Redistributiontype == (int)RedistributionType.PerApartament)
                {
                    if (invoice.Id_StairCase.HasValue && !apartment.Id_StairCase.HasValue) continue;

                    if (invoice.Id_StairCase.HasValue && apartment.Id_StairCase.HasValue &&
                        apartment.Id_StairCase.Value != invoice.Id_StairCase.Value) continue;

                    if (allAssociationApartments.Count == 0) continue;

                    int? nrApartments;
                    if (invoice.Id_StairCase.HasValue)
                    {
                        nrApartments = allAssociationApartments
                            .Count(t => invoice.Id_StairCase.HasValue && t.Id_StairCase == invoice.Id_StairCase.Value);
                    }
                    else
                    {
                        nrApartments = allAssociationApartments.Count();
                    }

                    var valueToAdd = invoice.Value.Value / nrApartments;
                    result = result.HasValue ? result + valueToAdd : valueToAdd;
                }
                else if (invoice.id_Redistributiontype == (int)RedistributionType.PerCotaIndiviza)
                {
                    if (invoice.Id_StairCase.HasValue && !apartment.Id_StairCase.HasValue) continue;

                    if (invoice.Id_StairCase.HasValue && apartment.Id_StairCase.HasValue &&
                        apartment.Id_StairCase.Value != invoice.Id_StairCase.Value) continue;
                    var apartments = allAssociationApartments;
                    if (invoice.Id_StairCase.HasValue)
                        apartments = allAssociationApartments.Where(t => t.Id_StairCase == invoice.Id_StairCase)
                            .ToList();

                    if (apartments.Count == 0) continue;

                    var valueToAdd =
                        RedistributionManager.RedistributeValueCotaIndivizaForSpecificApartments(apartment, invoice,
                            apartments);

                    result = result.HasValue ? result + valueToAdd : valueToAdd;
                }
                else if (invoice.id_Redistributiontype == (int)RedistributionType.PerCotaIndiviza)
                {
                }
            }

            if (result.HasValue)
            {
                var addedValue = DecimalExtensions.RoundUp((double)result, 2);
                expenseReport.Diverse = addedValue;
                if (!totalCol.ContainsKey(Expense.Diverse)) totalCol.Add(Expense.Diverse, 0.0m);

                totalCol[Expense.Diverse] = totalCol[Expense.Diverse] + addedValue;
                hasDiverse = true;
            }

            return hasDiverse;
        }

        private static void RaportAddHeaders(DataTable dt, List<AssociationExpenses> associationExpenses,
            bool addDiverse, bool hasRoundUpColumn)
        {
            dt.Columns.Add(new DataColumn("Ap", typeof(string)));
            dt.Columns.Add(new DataColumn("Nume", typeof(string)));
            dt.Columns.Add(new DataColumn("Pers", typeof(string)));
            dt.Columns.Add(new DataColumn("Cota ind.", typeof(string)));

            foreach (var associationExpense in associationExpenses)
                dt.Columns.Add(new DataColumn(associationExpense.Expenses.Name, typeof(string)));

            if (addDiverse) dt.Columns.Add(new DataColumn("Diverse", typeof(string)));

            if (hasRoundUpColumn) dt.Columns.Add(new DataColumn("Rotunjiri", typeof(string)));
            dt.Columns.Add(new DataColumn("Total lună", typeof(string)));
            dt.Columns.Add(new DataColumn("Fond rulment", typeof(string)));
            dt.Columns.Add(new DataColumn("Fond reparații", typeof(string)));

            dt.Columns.Add(new DataColumn("Penalizări", typeof(string)));
            dt.Columns.Add(new DataColumn("Total general", typeof(string)));


            dt.Columns.Add(new DataColumn("Ap ", typeof(string)));
        }

        private static decimal RaportAddRows(DataTable dt, Dictionary<int, Expense> raportDictionary, int fieldSize,
            List<ExpenseReport> expenseReportList, bool hasRoundUpColumn)
        {
            var generalMonthSum = 0.0m;
            foreach (var raportList in expenseReportList)
            {
                var row = new List<object>();
                var monthSum = 0.0m;
                decimal generalSum;
                row.Add(raportList.Ap);
                row.Add(raportList.Name);
                row.Add(raportList.NrPers);
                row.Add(raportList.CotaIndiviza);

                for (var i = 0; i < fieldSize; i++)
                {
                    var item = raportDictionary.FirstOrDefault(r => r.Key == i);
                    var displayedText = string.Empty;

                    if (item.Value != 0)
                    {
                        var result = GetExpenseFromRaportListOnOrder(item.Value, raportList);
                        if (result.HasValue)
                        {
                            if (item.Value == Expense.AjutorÎncălzire)
                                monthSum = monthSum - result.Value;
                            else
                                monthSum = monthSum + result.Value;
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
                    monthSum = DecimalExtensions.RoundUp((double)monthSum, 0);

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

        private static void raportAddTotalRow(DataTable dt, Dictionary<Expense, decimal> totalCol,
            decimal generalMonthSum, bool hasRoundUpColumn)
        {
            var rowTotal = new List<object>();

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

        public static void ConfigureIndividual(AssociationExpenses ae, Apartments apartment)
        {
            var apartmentExpense = GetByExpenseEstateIdAndApartmentId(ae.Id, apartment.Id);

            if (apartmentExpense == null) AddDefaultApartmentExpenseForIndividual(apartment.Id, ae.Id);
        }

        public static void UpdateApartmentExpense(int apartmentExpenseId, decimal? newValue)
        {
            var result = new ApartmentExpenses();
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
            IEnumerable<ApartmentExpenses> apartmentExpenses =
                GetApartmentsExpenseByAssociationExpenseId(associationExpenseId).ToList();

            foreach (var apartmentExpense in apartmentExpenses)
            {
                decimal? newValue = null;
                decimal? consumption = null;

                if (apartmentExpense.IndexOld.HasValue && apartmentExpense.IndexNew.HasValue)
                    consumption = apartmentExpense.IndexNew.Value - apartmentExpense.IndexOld.Value;

                if (consumption.HasValue && newPricePerUnit.HasValue)
                    newValue = consumption.Value * newPricePerUnit.Value;

                UpdateApartmentExpense(apartmentExpense.Id, newValue);
            }
        }

        internal static IEnumerable<ApartmentExpenses> GetApartmentsExpenseByAssociationExpenseId(
            int associationExpenseId)
        {
            return GetContext(true).ApartmentExpenses.Where(te => te.Id_EstateExpense == associationExpenseId);
        }

        internal static decimal? GetConsumption(AssociationExpenses associationExpense, int? stairCase)
        {
            decimal? result = null;

            var assCounter = AssociationCountersManager.GetByExpenseAndStairCase(associationExpense, stairCase);
            if (assCounter != null)
            {
                var apWithSpecificCounter = GetApartmentsByCounter(assCounter).Select(a => a.Id);
                var sumOfIndexes = associationExpense.ApartmentExpenses
                    .Where(tex => apWithSpecificCounter.Contains(tex.Apartments.Id)).Sum(t => t.IndexNew - t.IndexOld);
                if (sumOfIndexes.HasValue)
                {
                    var pricePerUnit = UnitPricesManager.GetPrice(associationExpense.Id, stairCase);
                    if (pricePerUnit.HasValue) result = sumOfIndexes * pricePerUnit.Value;
                }
            }

            return result;
        }

        internal static IEnumerable<Apartments> GetApartmentsByCounter(int associationCounterId)
        {
            return GetApartmentsByCounter(AssociationCountersManager.GetById(associationCounterId));
        }

        internal static IEnumerable<Apartments> GetApartmentsByCounter(AssociationCounters associationCounter)
        {
            var result = Enumerable.Empty<Apartments>();

            if (associationCounter != null)
            {
                var allStairCases = associationCounter.AssociationCountersStairCase.Select(ass => ass.Id_StairCase)
                    .ToList();

                var allAps = new List<Apartments>();
                foreach (var associationCountersStairCase in associationCounter.AssociationCountersStairCase)
                    foreach (var associationCountersApartment in associationCountersStairCase.AssociationCounters
                        .AssociationCountersApartment)
                        if (allStairCases.Contains(null) ||
                            allStairCases.Contains(associationCountersApartment.Apartments.Id_StairCase))
                            allAps.Add(associationCountersApartment.Apartments);

                result = allAps.Distinct();
            }

            return result;
        }

        internal static IEnumerable<Apartments> GetApartmentsOnSameCounter(int associationExpenseId, int? stairCase)
        {
            var result = Enumerable.Empty<Apartments>();
            var associationExpense = AssociationExpensesManager.GetById(associationExpenseId);

            if (associationExpense != null)
            {
                var associationCounter =
                    AssociationCountersManager.GetByExpenseAndStairCase(associationExpense, stairCase);

                if (associationCounter != null) result = GetApartmentsByCounter(associationCounter);
            }

            return result;
        }

        public static void ConfigurePerIndex(AssociationExpenses ee, List<Apartments> apartments)
        {
            foreach (var apartment in apartments) ConfigurePerIndex(ee, apartment);
        }
    }
}