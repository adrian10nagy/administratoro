
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

    public static class TenantExpensesManager
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

        public static TenantExpenses GetForIndexExpensPreviousMonthTenantExpense(int idExpenseEstateCurentMonth, int idTenant)
        {
            TenantExpenses result = null;
            var ee = EstateExpensesManager.GetById(idExpenseEstateCurentMonth);
            if (ee != null)
            {
                //todo -1 does not work for month 1(january)
                var lastMonthEE = GetContext(true).EstateExpenses.FirstOrDefault(e => e.Month == ee.Month - 1 &&
                    e.Id_Estate == ee.Id_Estate && e.Id_Expense == ee.Id_Expense && e.Id_ExpenseType == ee.Id_ExpenseType &&
                    e.isDefault == ee.isDefault && e.Year == ee.Year && e.WasDisabled == ee.WasDisabled);

                if (lastMonthEE != null)
                {
                    result = GetByExpenseEstateIdAndapartmentid(lastMonthEE.Id, idTenant);
                }
            }

            return result;
        }

        public static TenantExpenses GetByExpenseEstateIdAndapartmentid(int idExpenseEstate, int idTenant)
        {
            return GetContext().TenantExpenses.FirstOrDefault(te => te.Id_EstateExpense == idExpenseEstate && te.Id_Tenant == idTenant);
        }

        private static TenantExpenses GetById(int tenantExpenseId)
        {
            return GetContext().TenantExpenses.FirstOrDefault(te => te.Id == tenantExpenseId);
        }

        public static decimal? GetSumOfIndexesForexpense(int estateExpenseId)
        {
            return GetContext().TenantExpenses.Where(te => te.Id_EstateExpense == estateExpenseId).Sum(s => s.IndexNew - s.IndexOld);
        }

        public static decimal? GetSumOfIndexesForexpense(int estateExpenseId, int? stairCase)
        {
            return GetContext().TenantExpenses.Where(te => te.Id_EstateExpense == estateExpenseId && te.Tenants.Id_StairCase == stairCase).Sum(s => s.IndexNew - s.IndexOld);
        }

        public static void AddTenantExpense(int apartmentid, int expenseEstateId, decimal expenseValue)
        {
            TenantExpenses te = new TenantExpenses
            {
                Value = expenseValue,
                Id_Tenant = apartmentid,
                Id_EstateExpense = expenseEstateId,

            };
            GetContext().TenantExpenses.Add(te);
            GetContext().SaveChanges();
        }

        public static void UpdateTenantExpense(int idExpenseEstate, int apartmentid, decimal value)
        {
            TenantExpenses result = new TenantExpenses();
            result = GetContext().TenantExpenses.FirstOrDefault(b => b.Id_EstateExpense == idExpenseEstate && b.Id_Tenant == apartmentid);

            if (result != null)
            {
                result.Value = value;
                GetContext().Entry(result).CurrentValues.SetValues(result);

                GetContext().SaveChanges();
            }
        }

        public static void RemoveTenantExpense(int apartmentid, int year, int month, object expense)
        {
            if (expense != null && expense is Expenses)
            {
                var e = (Expenses)expense;
                Tenants tenant = GetContext().Tenants.First(t => t.Id == apartmentid);
                if (tenant != null)
                {
                    EstateExpenses estateExpenses = GetContext().EstateExpenses.First(ee => ee.Id_Expense == e.Id
                        && ee.Id_Estate == tenant.id_Estate && ee.Year == year && ee.Month == month);
                    if (estateExpenses != null)
                    {
                        TenantExpenses tenantExpenses = GetContext().TenantExpenses.Where(tex => tex.Id_EstateExpense == estateExpenses.Id
                            && tex.Id_Tenant == apartmentid).FirstOrDefault();
                        if (tenantExpenses != null && tenantExpenses.Value != null)
                        {
                            GetContext().TenantExpenses.Remove(tenantExpenses);
                            GetContext().SaveChanges();
                        }
                    }
                }
            }
        }

        public static void RemoveTenantExpense(int apartmentid, int estateExpenseId)
        {
            TenantExpenses tenantExpenses = GetContext().TenantExpenses.Where(tex => tex.Id_EstateExpense == estateExpenseId
                           && tex.Id_Tenant == apartmentid).FirstOrDefault();
            if (tenantExpenses != null && tenantExpenses.Value != null)
            {
                GetContext().TenantExpenses.Remove(tenantExpenses);
                GetContext().SaveChanges();
            }
        }

        public static void AddCotaIndivizaTenantExpenses(int idExpenseEstate, decimal totalValue)
        {
            EstateExpenses ee = GetContext().EstateExpenses.FirstOrDefault(e => e.Id == idExpenseEstate);

            if (ee != null)
            {
                AddCotaIndivizaTenantExpenses(ee, totalValue, null);
            }
        }

        public static void AddCotaIndivizaTenantExpenses(EstateExpenses expenseEstate, decimal? totalValue, int? stairCase)
        {
            // apartments
            List<Tenants> apartments = new List<Tenants>();
            if (stairCase.HasValue)
            {
                apartments = GetContext().Tenants.Where(a => a.id_Estate == expenseEstate.Id_Estate && a.Id_StairCase == stairCase).ToList();
            }
            else
            {
                apartments = GetContext().Tenants.Where(a => a.id_Estate == expenseEstate.Id_Estate).ToList();
            }

            decimal totalCota = apartments.Sum(te => te.CotaIndiviza.Value);
            decimal? valuePerCotaUnit = null;
            if (totalValue.HasValue && totalCota != 0)
            {
                valuePerCotaUnit = totalValue.Value / totalCota;
            }

            foreach (var tenant in apartments)
            {
                decimal? theValue = null;
                TenantExpenses tte = GetContext().TenantExpenses.FirstOrDefault(tee => tee.Id_EstateExpense == expenseEstate.Id && tee.Id_Tenant == tenant.Id);
                if (valuePerCotaUnit.HasValue && tenant.CotaIndiviza.HasValue)
                {
                    theValue = tenant.CotaIndiviza.Value * valuePerCotaUnit.Value;
                }

                if (tte != null)
                {
                    tte.Value = theValue;

                    GetContext().Entry(tte).CurrentValues.SetValues(tte);
                }
                else
                {
                    TenantExpenses te = new TenantExpenses
                    {
                        Value = theValue,
                        Id_Tenant = tenant.Id,
                        Id_EstateExpense = expenseEstate.Id,

                    };
                    GetContext().TenantExpenses.Add(te);
                }

                GetContext().SaveChanges();
            }
        }

        public static void AddPerTenantExpenses(int idExpenseEstate, decimal? valuePerTenant, List<Tenants> tenants = null)
        {
            var ee = EstateExpensesManager.GetById(idExpenseEstate);
            if (ee != null)
            {
                List<Tenants> allTenants;

                if (tenants == null)
                {
                    allTenants = ApartmentsManager.GetAllByAssociationId(ee.Id_Estate);
                }
                else
                {
                    allTenants = tenants;
                }

                foreach (var tenant in allTenants)
                {
                    if (tenant != null)
                    {
                        TenantExpenses tte = TenantExpensesManager.GetByExpenseEstateIdAndapartmentid(idExpenseEstate, tenant.Id);
                        if (tte != null)
                        {
                            tte.Value = (valuePerTenant.HasValue) ? valuePerTenant * tenant.Dependents : null;
                            GetContext().Entry(tte).CurrentValues.SetValues(tte);
                        }
                        else
                        {
                            TenantExpenses te = new TenantExpenses
                            {
                                Value = valuePerTenant.HasValue ? valuePerTenant * tenant.Dependents : null,
                                Id_Tenant = tenant.Id,
                                Id_EstateExpense = ee.Id,
                            };
                            GetContext().TenantExpenses.Add(te);
                        }

                        GetContext().SaveChanges();
                    }
                }
            }
        }

        public static void UpdateOldIndexAndValue(TenantExpenses tenantExpense, decimal? oldIndex, decimal? pricePerExpenseUnit)
        {
            TenantExpenses result = new TenantExpenses();
            result = GetContext().TenantExpenses.First(b => b.Id == tenantExpense.Id);

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

        public static void UpdatePerIndexValue(TenantExpenses tenantExpense, decimal? pricePerExpenseUnit)
        {
            TenantExpenses result = new TenantExpenses();
            result = GetContext(true).TenantExpenses.First(b => b.Id == tenantExpense.Id);

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


        public static void ConfigurePerIndex(EstateExpenses ee, Administratoro.DAL.Tenants tenant)
        {
            var tenantExpense = TenantExpensesManager.GetByExpenseEstateIdAndapartmentid(ee.Id, tenant.Id);

            if (tenantExpense == null)
            {
                TenantExpensesManager.AddDefaultTenantExpense(tenant.Id, ee.Year, ee.Month, ee.Id);
                tenantExpense = TenantExpensesManager.GetByExpenseEstateIdAndapartmentid(ee.Id, tenant.Id);
            }

            if (ee != null && ee.Id_ExpenseType == (int)ExpenseType.PerIndex)
            {
                var lastMonthIndexTenantExpense = TenantExpensesManager.GetForIndexExpensPreviousMonthTenantExpense(ee.Id, tenant.Id);

                if (lastMonthIndexTenantExpense != null && !lastMonthIndexTenantExpense.EstateExpenses.IsClosed.HasValue)
                {
                    if (lastMonthIndexTenantExpense == null && tenantExpense.IndexOld == null)
                    {
                        TenantExpensesManager.UpdateOldIndexAndValue(tenantExpense, 0, ee.PricePerExpenseUnit);
                    }
                    else if (lastMonthIndexTenantExpense != null && tenantExpense != null)
                    {
                        if (lastMonthIndexTenantExpense.IndexNew != tenantExpense.IndexOld
                            || !tenantExpense.IndexOld.HasValue)
                        {
                            TenantExpensesManager.UpdateOldIndexAndValue(tenantExpense, lastMonthIndexTenantExpense.IndexNew, ee.PricePerExpenseUnit);
                        }
                    }

                    TenantExpensesManager.UpdatePerIndexValue(tenantExpense, ee.PricePerExpenseUnit);
                }
            }
        }

        public static void UpdateNewIndexAndValue(int tenantExpenseId, int idExpenseEstate, decimal? newIndex, bool shouldUpdateOld, decimal? oldIndex = null)
        {
            if (newIndex.HasValue)
            {
                newIndex = Math.Round(newIndex.Value, ConfigConstants.IndexPrecision);
            }

            if (oldIndex.HasValue)
            {
                oldIndex = Math.Round(oldIndex.Value, ConfigConstants.IndexPrecision);
            }

            TenantExpenses tenantExpense = TenantExpensesManager.GetById(tenantExpenseId);
            EstateExpenses estateExpenses = EstateExpensesManager.GetById(idExpenseEstate);

            if (tenantExpense != null && estateExpenses != null)
            {
                tenantExpense.IndexNew = newIndex;
                if (shouldUpdateOld)
                {
                    tenantExpense.IndexOld = oldIndex;
                }

                if (tenantExpense.IndexOld != null && newIndex.HasValue && estateExpenses.PricePerExpenseUnit.HasValue)
                {
                    tenantExpense.Value = (tenantExpense.IndexNew - tenantExpense.IndexOld) * estateExpenses.PricePerExpenseUnit;
                }
                else
                {
                    tenantExpense.Value = null;
                }

                GetContext().Entry(tenantExpense).CurrentValues.SetValues(tenantExpense);
                GetContext().SaveChanges();

                //update previeous month old index
                var lastMonthIndexTenantExpense = TenantExpensesManager.GetForIndexExpensPreviousMonthTenantExpense(estateExpenses.Id, tenantExpense.Id_Tenant);
                if (lastMonthIndexTenantExpense != null && tenantExpense.IndexOld != null &&
                    lastMonthIndexTenantExpense.IndexNew != tenantExpense.IndexOld && !lastMonthIndexTenantExpense.EstateExpenses.IsClosed.HasValue)
                {
                    lastMonthIndexTenantExpense.IndexNew = tenantExpense.IndexOld;
                    GetContext().Entry(lastMonthIndexTenantExpense).CurrentValues.SetValues(lastMonthIndexTenantExpense);
                    GetContext().SaveChanges();
                }
            }
        }

        internal static TenantExpenses GetByExpenseYearAndMonth(int apartmentId, int estateExpenseId)
        {
            var result = GetContext(true).TenantExpenses.Where(te => te.Id_Tenant == apartmentId && te.Id_EstateExpense == estateExpenseId).ToList();

            return result.FirstOrDefault();
        }

        public static void UpdateTenantExpenses(EstateExpenses estateExpense, decimal? value, int? stairCase = null)
        {
            if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
            {
                TenantExpensesManager.AddCotaIndivizaTenantExpenses(estateExpense, value, stairCase);
            }
            else if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerTenants)
            {
                decimal? valuePerTenant = null;
                List<Tenants> tenants;

                if (stairCase == null || !stairCase.HasValue)
                {
                    tenants = ApartmentsManager.GetAllByAssociationId(estateExpense.Id_Estate);
                }
                else
                {
                    tenants = ApartmentsManager.GetAllByEstateIdAndStairCase(estateExpense.Id_Estate, stairCase.Value);
                }

                var allTenantDependents = tenants.Sum(t => t.Dependents);
                if (value.HasValue && allTenantDependents != 0)
                {
                    valuePerTenant = value.Value / allTenantDependents;
                }

                TenantExpensesManager.AddPerTenantExpenses(estateExpense.Id, valuePerTenant, tenants);
            }
        }


        public static List<TenantExpenses> GetAllExpensesByTenantAndMonth(int apartmentid, int year, int month)
        {
            var te = GetContext().TenantExpenses.Where(e => e.Id_Tenant == apartmentid
                && e.EstateExpenses.Month == month && e.EstateExpenses.Year == year)
                .ToListAsync().Result;

            return te;
        }

        public static TenantExpenses GetExpenseByTenantMonth(int apartmentid, int year, int month, object expense)
        {
            var allTenantExpenses = GetAllExpensesByTenantAndMonth(apartmentid, year, month);
            var specificExpense = allTenantExpenses.Where(e => e.EstateExpenses.Id_Expense == ((Expenses)expense).Id).ToList();
            TenantExpenses result = null;

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

        public static void UpdateTenantExpense(int apartmentid, int year, int month, object te)
        {
            TenantExpenses result = new TenantExpenses();
            result = GetContext().TenantExpenses.First(b => b.Id == ((TenantExpenses)te).Id);

            if (result != null)
            {
                result.Value = ((TenantExpenses)te).Value;
                GetContext().Entry(result).CurrentValues.SetValues(result);

                GetContext().SaveChanges();
            }
        }

        public static void AddDefaultTenantExpense(int apartmentid, int year, int month, int estateExpenseId)
        {
            var estateExpense = GetContext().EstateExpenses.FirstOrDefault(ee => ee.Id == estateExpenseId);

            if (estateExpense != null)
            {
                decimal? value = null;
                decimal? oldIndex = null;
                var te = TenantExpensesManager.GetForIndexExpensPreviousMonthTenantExpense(estateExpenseId, apartmentid);
                if (te != null)
                {
                    if (estateExpense.ExpenseTypes.Id == (int)ExpenseType.PerIndex)
                    {
                        oldIndex = te.IndexNew;
                        //value = te.Value;
                    }
                    else if (estateExpense.ExpenseTypes.Id == (int)ExpenseType.Individual)
                    {
                        value = te.Value;
                    }
                }

                TenantExpenses tenantExpense = new TenantExpenses
                {
                    Value = value,
                    Id_Tenant = apartmentid,
                    Id_EstateExpense = estateExpenseId,
                    IndexOld = oldIndex
                };

                GetContext().TenantExpenses.Add(tenantExpense);
                GetContext().SaveChanges();
            }
        }

        public static void AddTenantExpense(int apartmentid, int year, int month, object theExpense, decimal expenseValue)
        {
            Tenants tenant = GetContext().Tenants.FirstOrDefault(t => t.Id == apartmentid);
            Expenses expense = theExpense as Expenses;

            if (tenant != null)
            {
                EstateExpenses estateExpense = GetContext().EstateExpenses.FirstOrDefault(ee => ee.Id_Estate == tenant.id_Estate
                    && ee.Id_Expense == expense.Id && ee.Month == month && ee.Year == year);

                if (estateExpense != null)
                {
                    TenantExpenses tenantExpense = new TenantExpenses
                    {
                        Value = expenseValue,
                        Id_Tenant = apartmentid,
                        Id_EstateExpense = estateExpense.Id
                    };

                    GetContext().TenantExpenses.Add(tenantExpense);
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
            var estateExpenses = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearNotDisabled(associationId, year, month).OrderBy(ee => ee.Id_ExpenseType).ToList();
            List<Tenants> apartments;
            if (!stairCase.HasValue)
            {
                apartments = association.Tenants.ToList();
            }
            else
            {
                apartments = ApartmentsManager.GetAllByEstateIdAndStairCase(association.Id, stairCase.Value);
            }

            apartments = apartments.OrderBy(a => a.Number).ToList();

            var invoices = InvoicesManager.GetDiverseByAssociationYearMonth(associationId, year, month);
            int expensesFieldSize = estateExpenses.Count();

            // populate expenses- pre-step
            bool hasDiverse;
            bool hasRoundUpColumn = association.HasRoundUpColumn.HasValue && association.HasRoundUpColumn.Value;
            RaportPopulateExpensesList(raportDictionary, totalCol, expenseReportList, estateExpenses, apartments, association, invoices, out hasDiverse);
            if (hasDiverse)
            {
                expensesFieldSize++;
            }

            // add headers
            RaportAddHeaders(dt, estateExpenses, hasDiverse, hasRoundUpColumn);

            // add rows
            decimal generalMonthSum = RaportAddRows(dt, raportDictionary, expensesFieldSize, expenseReportList, hasRoundUpColumn);

            raportAddTotalRow(dt, totalCol, generalMonthSum, hasRoundUpColumn);

            return dt;
        }

        private static void RaportPopulateExpensesList(Dictionary<int, Expense> raportDictionary, Dictionary<Expense, decimal> totalCol,
            List<ExpenseReport> expenseReportList, List<EstateExpenses> estateExpenses, List<Tenants> tenants,
            Estates association, List<Invoices> invoices, out bool hasDiverse)
        {
            hasDiverse = false;
            foreach (var tenant in tenants)
            {
                ExpenseReport expenseReport = new ExpenseReport();
                expenseReport.Ap = tenant.Number.ToString();
                expenseReport.Name = tenant.Name;
                expenseReport.NrPers = tenant.Dependents;
                expenseReport.CotaIndiviza = tenant.CotaIndiviza.HasValue ? tenant.CotaIndiviza.Value.ToString() : string.Empty;

                raportDictionary.Clear();
                int counter = 0;
                foreach (EstateExpenses estateExpense in estateExpenses)
                {
                    decimal? tenantExpenseRedistributionValue = null;
                    decimal? rowValue = null;
                    raportDictionary.Add(counter, (Expense)estateExpense.Expenses.Id);
                    counter++;

                    if (!totalCol.ContainsKey((Expense)estateExpense.Expenses.Id))
                    {
                        totalCol.Add((Expense)estateExpense.Expenses.Id, 0.0m);
                    }

                    TenantExpenses te = TenantExpensesManager.GetByExpenseYearAndMonth(tenant.Id, estateExpense.Id);
                    if (estateExpense.RedistributeType.HasValue)
                    {
                        tenantExpenseRedistributionValue = RedistributionManager.CalculateRedistributeValueForStairCase(estateExpense.Id, tenant, te);
                    }
                    rowValue = CalculateRowValue(te, tenantExpenseRedistributionValue);

                    switch ((Expense)estateExpense.Expenses.Id)
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

                    totalCol[(Expense)estateExpense.Expenses.Id] = rowValue == null ? totalCol[(Expense)estateExpense.Expenses.Id] :
                                totalCol[(Expense)estateExpense.Expenses.Id] + rowValue.Value;
                }

                raportDictionary.Add(counter, Expense.Diverse);

                hasDiverse = RaportPopulateExpensesListDiverse(tenant, invoices, association, totalCol, expenseReport) || hasDiverse;

                expenseReportList.Add(expenseReport);
            }
        }

        private static decimal? CalculateRowValue(TenantExpenses te, decimal? redistributionValue)
        {
            decimal? result = null;

            if (te != null && te.Value.HasValue)
            {
                result = te.Value.Value;
            }

            if (redistributionValue.HasValue)
            {
                result = result.HasValue ? result + redistributionValue.Value : redistributionValue.Value;
            }

            return result;
        }

        private static bool RaportPopulateExpensesListDiverse(Tenants tenant, List<Invoices> invoices, Estates association,
            Dictionary<Expense, decimal> totalCol, ExpenseReport expenseReport)
        {
            bool hasDiverse = false;
            List<Tenants> allAssociationTenants = association.Tenants.ToList();

            decimal? result = null;
            foreach (Invoices invoice in invoices)
            {
                if (!invoice.Value.HasValue)
                {
                    continue;
                }

                if (invoice.id_Redistributiontype == (int)RedistributionType.PerTenants)
                {
                    int? nrOfDependents = null;
                    if (invoice.Id_StairCase.HasValue)
                    {
                        nrOfDependents = allAssociationTenants.Where(t => t.Id_StairCase == invoice.Id_StairCase).Select(i => i.Dependents).Sum();
                    }

                    decimal? valueToAdd = null;
                    if (nrOfDependents.HasValue)
                    {
                        if (nrOfDependents != 0)
                        {
                            valueToAdd = ((invoice.Value.Value * tenant.Dependents) / nrOfDependents.Value);
                        }
                    }
                    else
                    {
                        var allDependents = allAssociationTenants.Select(t => t.Dependents).Sum();

                        if (allDependents != 0)
                        {
                            valueToAdd = ((invoice.Value.Value * tenant.Dependents) / allDependents);
                        }
                    }

                    result = result.HasValue ? (result + valueToAdd) : valueToAdd;

                }
                else if (invoice.id_Redistributiontype == (int)RedistributionType.PerApartament)
                {
                    if (invoice.Id_StairCase.HasValue && !tenant.Id_StairCase.HasValue)
                    {
                        continue;
                    }

                    if (invoice.Id_StairCase.HasValue && tenant.Id_StairCase.HasValue && tenant.Id_StairCase.Value != invoice.Id_StairCase.Value)
                    {
                        continue;
                    }

                    if (allAssociationTenants.Count == 0)
                    {
                        continue;
                    }

                    int? nrApartments = null;
                    if (invoice.Id_StairCase.HasValue)
                    {
                        nrApartments = allAssociationTenants.Where(t => t.Id_StairCase == invoice.Id_StairCase.Value).Count();
                    }
                    else
                    {
                        nrApartments = allAssociationTenants.Count();
                    }

                    decimal? valueToAdd = invoice.Value.Value / nrApartments;
                    result = result.HasValue ? (result + valueToAdd) : valueToAdd;
                }
                else if (invoice.id_Redistributiontype == (int)RedistributionType.PerCotaIndiviza)
                {
                    if (invoice.Id_StairCase.HasValue && !tenant.Id_StairCase.HasValue)
                    {
                        continue;
                    }

                    if (invoice.Id_StairCase.HasValue && tenant.Id_StairCase.HasValue && tenant.Id_StairCase.Value != invoice.Id_StairCase.Value)
                    {
                        continue;
                    }
                    var tenants = allAssociationTenants;
                    if (invoice.Id_StairCase.HasValue)
                    {
                        tenants = allAssociationTenants.Where(t => t.Id_StairCase == invoice.Id_StairCase).ToList();
                    }

                    if (tenants.Count == 0)
                    {
                        continue;
                    }

                    var valueToAdd = RedistributionManager.RedistributeValueCotaIndivizaForSpecificTenants(tenant, invoice, tenants);

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

        private static void RaportAddHeaders(DataTable dt, List<EstateExpenses> estateExpenses, bool addDiverse, bool hasRoundUpColumn)
        {
            dt.Columns.Add(new DataColumn("Ap", typeof(string)));
            dt.Columns.Add(new DataColumn("Nume", typeof(string)));
            dt.Columns.Add(new DataColumn("Pers", typeof(string)));
            dt.Columns.Add(new DataColumn("Cota ind.", typeof(string)));

            foreach (var estateExpense in estateExpenses)
            {
                dt.Columns.Add(new DataColumn(estateExpense.Expenses.Name, typeof(string)));
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

        public static void ConfigureIndividual(EstateExpenses ee, Tenants tenant)
        {
            var tenantExpense = TenantExpensesManager.GetByExpenseEstateIdAndapartmentid(ee.Id, tenant.Id);

            if (tenantExpense == null)
            {
                TenantExpensesManager.AddDefaultTenantExpense(tenant.Id, ee.Year, ee.Month, ee.Id);
                tenantExpense = TenantExpensesManager.GetByExpenseEstateIdAndapartmentid(ee.Id, tenant.Id);
            }
        }

        public static void UpdateTenantExpense(int tenantExpenseId, decimal? newValue)
        {
            TenantExpenses result = new TenantExpenses();
            result = GetContext().TenantExpenses.First(b => b.Id == tenantExpenseId);

            if (result != null)
            {
                result.Value = newValue;
                GetContext().Entry(result).CurrentValues.SetValues(result);

                GetContext().SaveChanges();
            }
        }

        internal static void UpdateValueForPriceUpdate(int estateExpenseId, decimal? newPricePerUnit)
        {
            IEnumerable<TenantExpenses> tenantExpenses = GetTenantsExpenseByEstateExpenseId(estateExpenseId).ToList();

            foreach (var tenantExpense in tenantExpenses)
            {
                decimal? newValue = null;
                decimal? consumption = null;

                if(tenantExpense.IndexOld.HasValue && tenantExpense.IndexNew.HasValue)
                {
                    consumption = tenantExpense.IndexNew.Value - tenantExpense.IndexOld.Value;
                }
                
                if(consumption.HasValue && newPricePerUnit.HasValue)
                {
                    newValue = consumption.Value * newPricePerUnit.Value;
                }
                
                UpdateTenantExpense(tenantExpense.Id, newValue);
            }
        }

        public static IEnumerable<TenantExpenses> GetTenantsExpenseByEstateExpenseId(int estateExpenseId)
        {
            return GetContext(true).TenantExpenses.Where(te => te.Id_EstateExpense == estateExpenseId);
        }
    }
}
