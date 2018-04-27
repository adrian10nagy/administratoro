
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
                //todo -1 does not work for month 1(january)
                var lastMonthEE = GetContext(true).AssociationExpenses.FirstOrDefault(e => e.Month == ee.Month - 1 &&
                    e.Id_Estate == ee.Id_Estate && e.Id_Expense == ee.Id_Expense && e.Id_ExpenseType == ee.Id_ExpenseType &&
                    e.isDefault == ee.isDefault && e.Year == ee.Year && e.WasDisabled == ee.WasDisabled);

                if (lastMonthEE != null)
                {
                    result = GetByExpenseEstateIdAndApartmentId(lastMonthEE.Id, idApartment);
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

        public static ApartmentExpenses GetByAssociationExpenseApartmentAndCountOrder(int idExpenseEstate, int IdApartment, int countOrder)
        {
            return GetContext().ApartmentExpenses.FirstOrDefault(te => te.Id_EstateExpense == idExpenseEstate
                && te.Id_Tenant == IdApartment
                && te.CounterOrder == countOrder);
        }

        private static ApartmentExpenses GetById(int apartmentExpenseId)
        {
            return GetContext().ApartmentExpenses.FirstOrDefault(te => te.Id == apartmentExpenseId);
        }

        public static decimal? GetSumOfIndexesForexpense(int estateExpenseId)
        {
            return GetContext().ApartmentExpenses.Where(te => te.Id_EstateExpense == estateExpenseId).Sum(s => s.IndexNew - s.IndexOld);
        }

        public static decimal? GetSumOfIndexesForexpense(int estateExpenseId, int? stairCase)
        {
            return GetContext().ApartmentExpenses.Where(te => te.Id_EstateExpense == estateExpenseId && te.Apartments.Id_StairCase == stairCase).Sum(s => s.IndexNew - s.IndexOld);
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

        public static void AddCotaIndivizaApartmentExpenses(int idExpenseEstate, decimal totalValue)
        {
            AssociationExpenses ee = GetContext().AssociationExpenses.FirstOrDefault(e => e.Id == idExpenseEstate);

            if (ee != null)
            {
                AddCotaIndivizaApartmentExpenses(ee, totalValue, null);
            }
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

        public static void AddPerApartmentExpenses(int idExpenseEstate, decimal? valuePerApartment, List<Apartments> apartments = null)
        {
            var ee = AssociationExpensesManager.GetById(idExpenseEstate);
            if (ee != null)
            {
                List<Apartments> allApartments;

                if (apartments == null)
                {
                    allApartments = ApartmentsManager.GetAllByAssociationId(ee.Id_Estate);
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

        public static void UpdateOldIndexAndValue(ApartmentExpenses apartmentExpense, decimal? oldIndex, decimal? pricePerExpenseUnit)
        {
            ApartmentExpenses result = new ApartmentExpenses();
            result = GetContext().ApartmentExpenses.First(b => b.Id == apartmentExpense.Id);

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

                GetContext().Entry(result).CurrentValues.SetValues(result);
                GetContext().SaveChanges();
            }
        }

        public static void UpdatePerIndexValue(ApartmentExpenses apartmentExpense, decimal? pricePerExpenseUnit)
        {
            ApartmentExpenses result = new ApartmentExpenses();
            result = GetContext(true).ApartmentExpenses.First(b => b.Id == apartmentExpense.Id);

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
            var assC = CountersApartmentManager.GetByApartmentAndExpense(apartment.Id, ae.Id_Expense);
           
            var apartmentExpenses = ApartmentExpensesManager.GetByExpenseEstateIdAndApartmentIdAll(ae.Id, apartment.Id).ToList();

            if (assC != null && assC.CountersInsideApartment.HasValue)
            {
                if (apartmentExpenses.Count == 0)
                {
                    for (int i = 0; i < assC.CountersInsideApartment; i++)
                    {
                        ApartmentExpensesManager.AddDefaultApartmentExpense(apartment.Id, ae.Year, ae.Month, ae.Id, i, assC);
                    }
                }
                else if (apartmentExpenses.Count < assC.CountersInsideApartment.Value)
                {
                    for (int i = apartmentExpenses.Count; i < assC.CountersInsideApartment.Value; i++)
                    {
                        ApartmentExpensesManager.AddDefaultApartmentExpense(apartment.Id, ae.Year, ae.Month, ae.Id, i, assC);
                    }
                }
                else if (apartmentExpenses.Count > assC.CountersInsideApartment.Value)
                {
                    for (int i = assC.CountersInsideApartment.Value; i < apartmentExpenses.Count; i++)
                    {
                        ApartmentExpensesManager.RemoveApartmentExpense(apartment.Id, apartmentExpenses[i].Id_EstateExpense, apartmentExpenses[i].CounterOrder.Value);
                    }
                }
            }

            for (int i = 0; i < assC.CountersInsideApartment; i++)
            {
                var apartmentExpense = ApartmentExpensesManager.GetForPreviousMonthByOrder(ae.Id, apartment.Id, i);

                if(apartmentExpense == null)
                {
                    continue;
                }

                var lastMonthIndexApartmentExpense = ApartmentExpensesManager.GetForPreviousMonthByOrder(ae.Id, apartment.Id, i);

                if (lastMonthIndexApartmentExpense != null && !lastMonthIndexApartmentExpense.AssociationExpenses.IsClosed.HasValue)
                {
                    if (lastMonthIndexApartmentExpense == null && apartmentExpense.IndexOld == null)
                    {
                        ApartmentExpensesManager.UpdateOldIndexAndValue(apartmentExpense, 0, ae.PricePerExpenseUnit);
                    }
                    else if (lastMonthIndexApartmentExpense != null && apartmentExpense != null)
                    {
                        if (lastMonthIndexApartmentExpense.IndexNew != apartmentExpense.IndexOld
                            || !apartmentExpense.IndexOld.HasValue)
                        {
                            ApartmentExpensesManager.UpdateOldIndexAndValue(apartmentExpense, lastMonthIndexApartmentExpense.IndexNew, ae.PricePerExpenseUnit);
                        }
                    }

                    ApartmentExpensesManager.UpdatePerIndexValue(apartmentExpense, ae.PricePerExpenseUnit);
                }
            }

        }

        public static void UpdateNewIndexAndValue(int apartmentExpenseId, int idExpenseEstate, decimal? newIndex, bool shouldUpdateOld, decimal? oldIndex = null)
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
            AssociationExpenses associationExpense = AssociationExpensesManager.GetById(idExpenseEstate);

            if (apartmentExpense != null && associationExpense != null)
            {
                apartmentExpense.IndexNew = newIndex;
                if (shouldUpdateOld)
                {
                    apartmentExpense.IndexOld = oldIndex;
                }

                if (apartmentExpense.IndexOld != null && newIndex.HasValue && associationExpense.PricePerExpenseUnit.HasValue)
                {
                    apartmentExpense.Value = (apartmentExpense.IndexNew - apartmentExpense.IndexOld) * associationExpense.PricePerExpenseUnit;
                }
                else
                {
                    apartmentExpense.Value = null;
                }

                GetContext().Entry(apartmentExpense).CurrentValues.SetValues(apartmentExpense);
                GetContext().SaveChanges();

                //update previeous month old index
                var lastMonthIndexApartmentExpense = ApartmentExpensesManager.GetForIndexExpensPreviousMonthApartmentExpense(associationExpense.Id, apartmentExpense.Id_Tenant);
                if (lastMonthIndexApartmentExpense != null && apartmentExpense.IndexOld != null &&
                    lastMonthIndexApartmentExpense.IndexNew != apartmentExpense.IndexOld && !lastMonthIndexApartmentExpense.AssociationExpenses.IsClosed.HasValue)
                {
                    lastMonthIndexApartmentExpense.IndexNew = apartmentExpense.IndexOld;
                    GetContext().Entry(lastMonthIndexApartmentExpense).CurrentValues.SetValues(lastMonthIndexApartmentExpense);
                    GetContext().SaveChanges();
                }
            }
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
            else if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerApartments)
            {
                decimal? valuePerApartment = null;
                List<Apartments> apartments;

                if (stairCase == null || !stairCase.HasValue)
                {
                    apartments = ApartmentsManager.GetAllByAssociationId(associationExpense.Id_Estate);
                }
                else
                {
                    apartments = ApartmentsManager.GetAllByEstateIdAndStairCase(associationExpense.Id_Estate, stairCase.Value);
                }

                var allTenantDependents = apartments.Sum(t => t.Dependents);
                if (value.HasValue && allTenantDependents != 0)
                {
                    valuePerApartment = value.Value / allTenantDependents;
                }

                ApartmentExpensesManager.AddPerApartmentExpenses(associationExpense.Id, valuePerApartment, apartments);
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
            result = GetContext().ApartmentExpenses.First(b => b.Id == ((ApartmentExpenses)te).Id);

            if (result != null)
            {
                result.Value = ((ApartmentExpenses)te).Value;
                GetContext().Entry(result).CurrentValues.SetValues(result);

                GetContext().SaveChanges();
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
                var aeFormOld = ApartmentExpensesManager.GetForPreviousMonthByOrder(associationExpenseId, apartmentid, counterOrder);
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

        private static ApartmentExpenses GetForPreviousMonthByOrder(int idExpenseEstateCurentMonth, int apartmentid, int countOrder)
        {
            ApartmentExpenses result = null;
            var ee = AssociationExpensesManager.GetById(idExpenseEstateCurentMonth);
            if (ee != null)
            {
                //todo -1 does not work for month 1(january)
                var lastMonthEE = GetContext(true).AssociationExpenses.FirstOrDefault(ae => ae.Month == ee.Month - 1 &&
                    ae.Id_Estate == ee.Id_Estate && ae.Id_Expense == ee.Id_Expense && ae.Id_ExpenseType == ee.Id_ExpenseType &&
                    ae.isDefault == ee.isDefault && ae.Year == ee.Year && ae.WasDisabled == ee.WasDisabled);

                if (lastMonthEE != null)
                {
                    result = GetByAssociationExpenseApartmentAndCountOrder(lastMonthEE.Id, apartmentid, countOrder);
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

            var associationExpenses = AssociationExpensesManager.GetAllAssociationsByMonthAndYearNotDisabled(associationId, year, month).OrderBy(ee => ee.Id_ExpenseType).ToList();
            List<Apartments> apartments;
            if (!stairCase.HasValue)
            {
                apartments = association.Apartments.ToList();
            }
            else
            {
                apartments = ApartmentsManager.GetAllByEstateIdAndStairCase(association.Id, stairCase.Value);
            }

            apartments = apartments.OrderBy(a => a.Number).ToList();

            IEnumerable<Administratoro.DAL.Invoices> invoices = InvoicesManager.GetDiverseByAssociationYearMonth(associationId, year, month);
            int expensesFieldSize = associationExpenses.Count();

            // populate expenses- pre-step
            bool hasDiverse;
            bool hasRoundUpColumn = association.HasRoundUpColumn.HasValue && association.HasRoundUpColumn.Value;
            RaportPopulateExpensesList(raportDictionary, totalCol, expenseReportList, associationExpenses, apartments, association, invoices, out hasDiverse);
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
            hasDiverse = false;
            foreach (var apatrment in apartments)
            {
                ExpenseReport expenseReport = new ExpenseReport();
                expenseReport.Ap = apatrment.Number.ToString();
                expenseReport.Name = apatrment.Name;
                expenseReport.NrPers = apatrment.Dependents;
                expenseReport.CotaIndiviza = apatrment.CotaIndiviza.HasValue ? apatrment.CotaIndiviza.Value.ToString() : string.Empty;

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

                    IEnumerable<ApartmentExpenses> apartmentExpenses = ApartmentExpensesManager.GetByExpenseYearAndMonth(apatrment.Id, associationExpense.Id);
                    if (associationExpense.RedistributeType.HasValue)
                    {
                        apartmentExpenseRedistributionValue = RedistributionManager.CalculateRedistributeValueForStairCase(associationExpense.Id, apatrment, apartmentExpenses);
                    }
                    rowValue = CalculateRowValue(apartmentExpenses, apartmentExpenseRedistributionValue);

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

                    totalCol[(Expense)associationExpense.Expenses.Id] = rowValue == null ? totalCol[(Expense)associationExpense.Expenses.Id] :
                                totalCol[(Expense)associationExpense.Expenses.Id] + rowValue.Value;
                }

                raportDictionary.Add(counter, Expense.Diverse);

                hasDiverse = RaportPopulateExpensesListDiverse(apatrment, invoices, association, totalCol, expenseReport) || hasDiverse;

                expenseReportList.Add(expenseReport);
            }
        }

        private static decimal? CalculateRowValue(IEnumerable<ApartmentExpenses> apartmentExpenses, decimal? redistributionValue)
        {
            decimal? result = null;
            apartmentExpenses = apartmentExpenses.ToList();

            if (apartmentExpenses != null && apartmentExpenses.Count() != 0)
            {
                result = apartmentExpenses.Where(ae=>ae.Value.HasValue).Sum(ae=>ae.Value.Value);
            }

            if (redistributionValue.HasValue)
            {
                result = result.HasValue ? result + redistributionValue.Value : redistributionValue.Value;
            }

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

                if (invoice.id_Redistributiontype == (int)RedistributionType.PerApartments)
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

            dt.Columns.Add(new DataColumn("Total lună", typeof(string)));
            dt.Columns.Add(new DataColumn("Fond rulment", typeof(string)));
            dt.Columns.Add(new DataColumn("Fond reparații", typeof(string)));

            dt.Columns.Add(new DataColumn("Penalizări", typeof(string)));
            dt.Columns.Add(new DataColumn("Total general", typeof(string)));

            if (hasRoundUpColumn)
            {
                dt.Columns.Add(new DataColumn("Rotunjiri", typeof(string)));
            }

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
                generalSum = monthSum;
                generalSum = generalSum + 0 + 0 + 0 + 0;
                row.Add(monthSum.ToString());
                row.Add(string.Empty);
                row.Add(string.Empty);
                row.Add(string.Empty);
                row.Add(generalSum.ToString());

                if (hasRoundUpColumn)
                {
                    var value = (DecimalExtensions.RoundUp((double)generalSum, 0));
                    row.Add(value.ToString());
                }

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
            rowTotal.Add(generalMonthSum.ToString());
            rowTotal.Add(string.Empty);
            rowTotal.Add(string.Empty);
            rowTotal.Add(string.Empty);
            rowTotal.Add(generalMonthSum.ToString());

            if (hasRoundUpColumn)
            {
                var value = DecimalExtensions.RoundUp((double)generalMonthSum, 0);
                rowTotal.Add(value.ToString());
            }

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
            result = GetContext().ApartmentExpenses.First(b => b.Id == apartmentExpenseId);

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
    }
}
