
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

        public static decimal? GetSumOfIndivizaForExpense(EstateExpenses estateExpense)
        {
            decimal? result = null;

            var tenants = GetContext().Tenants.Where(t => t.id_Estate == estateExpense.Id_Estate).ToList();

            if (tenants != null && tenants.Count > 0)
            {
                result = tenants.Sum(s => s.CotaIndiviza);
            }

            return result;
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
            result = GetContext().TenantExpenses.First(b => b.Id_EstateExpense == idExpenseEstate && b.Id_Tenant == apartmentid);

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

            foreach (var tenant in apartments)
            {
                TenantExpenses tte = GetContext().TenantExpenses.FirstOrDefault(tee => tee.Id_EstateExpense == expenseEstate.Id && tee.Id_Tenant == tenant.Id);
                decimal? valuePerCotaUnit = null;
                if (totalValue.HasValue && totalCota != 0)
                {
                    valuePerCotaUnit = totalValue.Value / totalCota;
                }

                if (tte != null)
                {
                    decimal? result = null;
                    if(valuePerCotaUnit.HasValue)
                    {
                        result = DecimalExtensions.RoundUp((double)(tenant.CotaIndiviza.Value * valuePerCotaUnit.Value), 2);
                    }
                    tte.Value = result;
                    
                    GetContext().Entry(tte).CurrentValues.SetValues(tte);
                }
                else
                {
                    TenantExpenses te = new TenantExpenses
                    {
                        Value = valuePerCotaUnit.HasValue ? tenant.CotaIndiviza * valuePerCotaUnit : null,
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

                if(tenants == null)
                {
                    allTenants = ApartmentsManager.GetAllByEstateId(ee.Id_Estate);
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
            result = GetContext().TenantExpenses.First(b => b.Id == tenantExpense.Id);

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


        public static void ConfigurePerIndex(int esexId, EstateExpenses ee, Administratoro.DAL.Tenants tenant)
        {
            var tenantExpense = TenantExpensesManager.GetByExpenseEstateIdAndapartmentid(esexId, tenant.Id);

            if (tenantExpense == null)
            {
                TenantExpensesManager.AddDefaultTenantExpense(tenant.Id, ee.Year, ee.Month, esexId);
                tenantExpense = TenantExpensesManager.GetByExpenseEstateIdAndapartmentid(esexId, tenant.Id);
            }

            if (ee != null && ee.Id_ExpenseType == (int)ExpenseType.PerIndex)
            {
                var lastMonthIndexTenantExpense = TenantExpensesManager.GetForIndexExpensPreviousMonthTenantExpense(ee.Id, tenant.Id);

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

        public static void UpdateNewIndexAndValue(int tenantExpenseId, int idExpenseEstate, decimal? newIndex)
        {
            TenantExpenses tenantExpense = TenantExpensesManager.GetById(tenantExpenseId);
            EstateExpenses estateExpenses = EstateExpensesManager.GetById(idExpenseEstate);

            if (tenantExpense != null && estateExpenses != null)
            {
                tenantExpense.IndexNew = newIndex;
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
            }
        }

        internal static TenantExpenses GetByExpenseYearAndMonth(int apartmentId, int estateExpenseId)
        {
            var result = GetContext(true).TenantExpenses.Where(te => te.Id_Tenant == apartmentId && te.Id_EstateExpense == estateExpenseId).ToList();

            return result.FirstOrDefault();
        }

        public static void UpdateTenantExpenses(EstateExpenses estateExpense, decimal? value, int? stairCase = null)
        {
            if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerIndex)
            {
                // no update needed
            }
            else if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
            {
                TenantExpensesManager.AddCotaIndivizaTenantExpenses(estateExpense, value, stairCase);
            }
            else if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerTenants)
            {
                decimal? valuePerTenant = null;
                List<Tenants> tenants;

                if (stairCase == null || !stairCase.HasValue)
                {
                    tenants = ApartmentsManager.GetAllByEstateId(estateExpense.Id_Estate);
                }
                else
                {
                    tenants = ApartmentsManager.GetAllByEstateIdAndStairCase(estateExpense.Id_Estate, stairCase.Value);
                }

                var allTenantDependents = tenants.Sum(t => t.Dependents);
                if (value.HasValue && allTenantDependents != 0)
                {
                    valuePerTenant = DecimalExtensions.RoundUp((double)(value.Value / allTenantDependents), 2);
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
                if (estateExpense.ExpenseTypes.Id == (int)ExpenseType.PerIndex)
                {
                    var te = TenantExpensesManager.GetForIndexExpensPreviousMonthTenantExpense(estateExpenseId, apartmentid);
                    oldIndex = (te != null) ? te.IndexNew : 0;
                    value = 100;
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

        public static void AddDefaultTenantExpense(Tenants tenant, int year, int month)
        {
            List<EstateExpenses> estateExpenses = GetContext().EstateExpenses.Where(ee => ee.Id_Estate == tenant.id_Estate
                && ee.Month == month && ee.Year == year && !ee.Expenses.specialType.HasValue).ToList();
            if (estateExpenses != null && estateExpenses.Count > 0)
            {
                TenantExpenses tenantExpense = new TenantExpenses
                {
                    Value = null,
                    Id_Tenant = tenant.Id,
                    Id_EstateExpense = estateExpenses[0].Id
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

                if (estateExpense != null )
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

        public static DataTable GetAllExpensesAsDatatable(int estateId, int year, int month)
        {
            DataTable dt = new DataTable();

            // todo
            var tenants = ApartmentsManager.GetAllByEstateId(estateId);

            foreach (var tenant in tenants)
            {
                var expenses = TenantExpensesManager.GetAllExpensesByTenantAndMonth(tenant.Id, year, month);
                string query = string.Empty;
                if (expenses.Count == 0)
                {
                    TenantExpensesManager.AddDefaultTenantExpense(tenant, year, month);
                }
                query = @"USE administratoro DECLARE @DynamicPivotQuery AS NVARCHAR(MAX)
                                DECLARE @ColumnName AS NVARCHAR(MAX)
                                DECLARE @query  AS NVARCHAR(MAX)

                                SELECT @ColumnName= ISNULL(@ColumnName + ',','') 
		                                + QUOTENAME(Name)
                                FROM (SELECT DISTINCT Name 
                                FROM Expenses AS Expense
		                        INNER JOIN EstateExpenses as EE 
		                        on  EE.Id_Expense = Expense.Id 
		                        WHERE EE.Id_Estate = " + estateId + @" and EE.isDefault = 0
                                AND EE.WasDisabled = 0 and EE.Month = " + month + @" and EE.Year = " + year + @"
		                        ) as Expense

                                set @DynamicPivotQuery = 
                                'select 
                                *
                                FROM(
                                select 	
                                T.Number as NrAp,
                                T.Dependents as NrPers,   	
                                E.Name as ename,
                                TE.Value as tevalue,

                                T.Number as Ap
                                FROM EstateExpenses EE
                                LEFT JOIN TenantExpenses TE
                                ON TE.Id_EstateExpense = EE.Id
                                LEFT Join Tenants T
                                ON T.Id = TE.Id_tenant
                                LEFT Join Expenses E
                                ON E.Id = EE.Id_Expense
                                WHERE T.ID = 
		                        " + tenant.Id + @"
		                         AND EE.year = " + year + @"
                                AND EE.month = " + month + @"
                                AND EE.WasDisabled = 0
                                )
                                AS P
                                pivot
                                (
	                                sum(P.tevalue)
	                                for P.ename in (' + @ColumnName + ')

                                )as PIV
                                '

                                EXEC sp_executesql @DynamicPivotQuery";
                SqlConnection cnn = new SqlConnection("data source=HOME\\SQLEXPRESS;initial catalog=Administratoro;integrated security=True;MultipleActiveResultSets=True;");
                SqlCommand cmd = new SqlCommand(query, cnn);
                SqlDataAdapter adp = new SqlDataAdapter(cmd);
                adp.Fill(dt);
            }

            return dt;
        }

        public static DataTable GetAllEpensesAsList(int estateId, int year, int month)
        {
            var rowData = GetAllExpensesAsDatatable(estateId, year, month);

            List<ExpenseReport> expenseReportList = new List<ExpenseReport>();
            for (int i = 0; i < rowData.Rows.Count; i++)
            {
                ExpenseReport expenseReport = new ExpenseReport();

                expenseReport.NrPers = (rowData.Rows[i]["NrPers"] != null) ? Convert.ToInt32(rowData.Rows[i]["NrPers"]) : 0;
                expenseReport.Ap = (rowData.Rows[i]["Ap"] != null) ? rowData.Rows[i]["Ap"].ToString() : string.Empty;
                var admin = rowData.Rows[i]["Administrare"];
                decimal? adminResult = null;
                if (admin != null && !string.IsNullOrEmpty(admin.ToString()))
                {
                    adminResult = Convert.ToDecimal(admin.ToString());
                }
                expenseReport.Administrator = adminResult;

                var coldWather = rowData.Rows[i]["Apă rece"];
                decimal? coldWatherResult = null;
                if (coldWather != null && !string.IsNullOrEmpty(coldWather.ToString()))
                {
                    coldWatherResult = Convert.ToDecimal(coldWather.ToString());
                }
                expenseReport.WatherCold = coldWatherResult;

                var cleaning = rowData.Rows[i]["Curățenie"];
                decimal? cleaningResult = null;
                if (cleaning != null && !string.IsNullOrEmpty(cleaning.ToString()))
                {
                    cleaningResult = Convert.ToDecimal(cleaning.ToString());
                }
                expenseReport.Cleaning = cleaningResult;


                var gas = rowData.Rows[i]["Gaz"];
                decimal? gasResult = null;
                if (gas != null && !string.IsNullOrEmpty(gas.ToString()))
                {
                    gasResult = Convert.ToDecimal(gas.ToString());
                }
                expenseReport.Gas = gasResult;

                var heat = rowData.Rows[i]["Încălzire"];
                decimal? heatResult = null;
                if (heat != null && !string.IsNullOrEmpty(heat.ToString()))
                {
                    heatResult = Convert.ToDecimal(heat.ToString());
                }
                expenseReport.Heat = heatResult;

                var trash = rowData.Rows[i]["Salubritate"];
                decimal? trashResult = null;
                if (trash != null && !string.IsNullOrEmpty(trash.ToString()))
                {
                    trashResult = Convert.ToDecimal(trash.ToString());
                }
                expenseReport.Trash = trashResult;

                expenseReportList.Add(expenseReport);
            }

            DataTable dt = new DataTable();

            dt.Columns.AddRange(new DataColumn[9] 
            { 
                new DataColumn("Ap", typeof(int)),
                new DataColumn("NrPers", typeof(int)),
                new DataColumn("Administrare",typeof(decimal)),
                new DataColumn("Apă rece",typeof(decimal)), 
                new DataColumn("Curățenie",typeof(decimal)), 
                new DataColumn("Gaz",typeof(decimal)), 
                new DataColumn("Încălzire",typeof(decimal)), 
                new DataColumn("Salubritate",typeof(decimal)), 
                new DataColumn("Ap.",typeof(int)) 
            });
            foreach (var item in expenseReportList)
            {
                dt.Rows.Add(item.Ap, item.NrPers, item.Administrator, item.WatherCold, item.Cleaning, item.Gas, item.Heat, item.Trash, item.Ap);
            }

            return dt;
        }

        public static DataTable GetMonthlyRaportAsDataTable(int associationId, int year, int month, int? stairCase)
        {
            DataTable dt = new DataTable();
            Dictionary<int, Expense> raportDictionary = new Dictionary<int, Expense>();
            Dictionary<Expense, decimal> totalCol = new Dictionary<Expense, decimal>();
            int fieldSize = 0;
            List<ExpenseReport> expenseReportList = new List<ExpenseReport>();

            var association = AssociationsManager.GetById(associationId);
            if (association != null)
            {
                var estateExpenses = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearNotDisabled(associationId, year, month);
                List<Tenants> tenants;
                if (!stairCase.HasValue)
                {
                    tenants = association.Tenants.ToList();
                }
                else
                {
                    tenants = ApartmentsManager.GetAllByEstateIdAndStairCase(association.Id, stairCase.Value);
                }

                fieldSize = estateExpenses.Count();

                // populate list
                RaportPopulateExpensesList(raportDictionary, totalCol, expenseReportList, estateExpenses, tenants);
                // add headers
                RaportAddHeaders(dt, estateExpenses);
                // add rows
                decimal generalMonthSum = RaportAddRows(dt, raportDictionary, fieldSize, expenseReportList);

                raportAddTotalRow(dt, totalCol, generalMonthSum);
            }

            return dt;
        }

        private static void RaportPopulateExpensesList(Dictionary<int, Expense> raportDictionary, Dictionary<Expense, decimal> totalCol, List<ExpenseReport> expenseReportList, List<EstateExpenses> estateExpenses, List<Tenants> tenants)
        {
            foreach (var tenant in tenants)
            {
                ExpenseReport expenseReport = new ExpenseReport();
                expenseReport.Ap = tenant.Number.ToString();
                expenseReport.NrPers = tenant.Dependents;
                expenseReport.CotaIndiviza = tenant.CotaIndiviza.HasValue ? tenant.CotaIndiviza.Value.ToString() : string.Empty;

                raportDictionary.Clear();
                int counter = 0;
                foreach (EstateExpenses estateExpense in estateExpenses)
                {
                    raportDictionary.Add(counter, (Expense)estateExpense.Expenses.Id);
                    if (!totalCol.ContainsKey((Expense)estateExpense.Expenses.Id))
                    {
                        totalCol.Add((Expense)estateExpense.Expenses.Id, 0.0m);
                    }
                    TenantExpenses te = TenantExpensesManager.GetByExpenseYearAndMonth(tenant.Id, estateExpense.Id);
                    if (te != null)
                    {
                        switch ((Expense)te.EstateExpenses.Expenses.Id)
                        {
                            case Expense.ApaCalda:
                                expenseReport.WatherWarm = te.Value;
                                totalCol[Expense.ApaCalda] = te.Value != null ? totalCol[Expense.ApaCalda] + te.Value.Value : totalCol[Expense.ApaCalda];
                                break;
                            case Expense.ApaRece:
                                expenseReport.WatherCold = te.Value;
                                totalCol[Expense.ApaRece] = te.Value != null ? totalCol[Expense.ApaRece] + te.Value.Value : totalCol[Expense.ApaRece];
                                break;
                            case Expense.Salubritate:
                                expenseReport.Trash = te.Value;
                                totalCol[Expense.Salubritate] = te.Value != null ? totalCol[Expense.Salubritate] + te.Value.Value : totalCol[Expense.Salubritate];
                                break;
                            case Expense.Administrator:
                                expenseReport.Administrator = te.Value;
                                totalCol[Expense.Administrator] = te.Value != null ? totalCol[Expense.Administrator] + te.Value.Value : totalCol[Expense.Administrator];
                                break;
                            case Expense.Gaz:
                                expenseReport.Gas = te.Value;
                                totalCol[Expense.Gaz] = te.Value != null ? totalCol[Expense.Gaz] + te.Value.Value : totalCol[Expense.Gaz];
                                break;
                            case Expense.PersonalServiciu:
                                expenseReport.Cleaning = te.Value;
                                totalCol[Expense.PersonalServiciu] = te.Value != null ? totalCol[Expense.PersonalServiciu] + te.Value.Value : totalCol[Expense.PersonalServiciu];
                                break;
                            case Expense.Incalzire:
                                expenseReport.Heat = te.Value;
                                totalCol[Expense.Incalzire] = te.Value != null ? totalCol[Expense.Incalzire] + te.Value.Value : totalCol[Expense.Incalzire];
                                break;
                            case Expense.EnergieElectrica:
                                expenseReport.Electricity = te.Value;
                                totalCol[Expense.EnergieElectrica] = te.Value != null ? totalCol[Expense.EnergieElectrica] + te.Value.Value : totalCol[Expense.EnergieElectrica];
                                break;
                            case Expense.Lift:
                                expenseReport.Elevator = te.Value;
                                totalCol[Expense.Lift] = te.Value != null ? totalCol[Expense.Lift] + te.Value.Value : totalCol[Expense.Lift];
                                break;
                            case Expense.IntretinereInstalatii:
                                expenseReport.Utilities = te.Value;
                                totalCol[Expense.IntretinereInstalatii] = te.Value != null ? totalCol[Expense.IntretinereInstalatii] + te.Value.Value : totalCol[Expense.IntretinereInstalatii];
                                break;
                            case Expense.Presedinte:
                                expenseReport.President = te.Value;
                                totalCol[Expense.Presedinte] = te.Value != null ? totalCol[Expense.Presedinte] + te.Value.Value : totalCol[Expense.Presedinte];
                                break;
                            case Expense.Cenzor:
                                expenseReport.Censor = te.Value;
                                totalCol[Expense.Cenzor] = te.Value != null ? totalCol[Expense.Cenzor] + te.Value.Value : totalCol[Expense.Cenzor];
                                break;
                            case Expense.Fochist:
                                expenseReport.Fireman = te.Value;
                                totalCol[Expense.Fochist] = te.Value != null ? totalCol[Expense.Fochist] + te.Value.Value : totalCol[Expense.Fochist];
                                break;
                            case Expense.IntretinereAscensor:
                                expenseReport.ElevatorUtility = te.Value;
                                break;
                        }
                    }

                    counter++;
                }

                expenseReportList.Add(expenseReport);
            }
        }

        private static void RaportAddHeaders(DataTable dt, List<EstateExpenses> estateExpenses)
        {
            dt.Columns.Add(new DataColumn("Ap", typeof(string)));
            dt.Columns.Add(new DataColumn("Pers", typeof(string)));
            dt.Columns.Add(new DataColumn("Cota ind.", typeof(string)));

            foreach (var estateExpense in estateExpenses)
            {
                dt.Columns.Add(new DataColumn(estateExpense.Expenses.Name, typeof(string)));
            }
            dt.Columns.Add(new DataColumn("Diverse", typeof(string)));
            dt.Columns.Add(new DataColumn("Total lună", typeof(string)));
            dt.Columns.Add(new DataColumn("Ajutor încălzire", typeof(string)));
            dt.Columns.Add(new DataColumn("Fond rulment", typeof(string)));
            dt.Columns.Add(new DataColumn("Fond reparații", typeof(string)));

            dt.Columns.Add(new DataColumn("Penalizări", typeof(string)));
            dt.Columns.Add(new DataColumn("Total general", typeof(string)));
            dt.Columns.Add(new DataColumn("Ap ", typeof(string)));
        }

        private static decimal RaportAddRows(DataTable dt, Dictionary<int, Expense> raportDictionary, int fieldSize, List<ExpenseReport> expenseReportList)
        {
            decimal generalMonthSum = 0.0m;
            foreach (var raportList in expenseReportList)
            {
                List<object> row = new List<object>();
                decimal monthSum = 0.0m;
                decimal generalSum = 0.0m;
                row.Add(raportList.Ap);
                row.Add(raportList.NrPers);
                row.Add(raportList.CotaIndiviza);

                for (int i = 0; i < fieldSize; i++)
                {
                    var item = raportDictionary.FirstOrDefault(r => r.Key == i);
                    string displayedText = string.Empty;

                    if (item.Value != 0)
                    {
                        decimal? result = GetExpenseFromRaportListOnOrder(item.Value, raportList);
                        displayedText = result != null ? result.ToString() : string.Empty;
                        if (result.HasValue)
                        {
                            monthSum = monthSum + result.Value;
                        }
                    }

                    row.Add(displayedText);
                }

                generalMonthSum = generalMonthSum + monthSum;
                monthSum = monthSum + 0; // diverse
                generalSum = monthSum;
                generalSum = generalSum + 0 + 0 + 0 + 0;
                row.Add(string.Empty);
                row.Add(monthSum.ToString());
                row.Add(string.Empty);
                row.Add(string.Empty);
                row.Add(string.Empty);
                row.Add(string.Empty);
                row.Add(generalSum.ToString());
                row.Add(raportList.Ap);


                dt.Rows.Add(row.ToArray());
            }
            return generalMonthSum;
        }

        private static void raportAddTotalRow(DataTable dt, Dictionary<Expense, decimal> totalCol, decimal generalMonthSum)
        {
            List<object> rowTotal = new List<object>();

            rowTotal.Add(string.Empty);
            rowTotal.Add(string.Empty);
            rowTotal.Add(string.Empty);
            foreach (var item in totalCol)
            {
                rowTotal.Add(item.Value);
            }
            rowTotal.Add("0,0");
            rowTotal.Add(generalMonthSum.ToString());
            rowTotal.Add(string.Empty);
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
                case Expense.Incalzire:
                    result = raportList.Heat.HasValue ? raportList.Heat : null;
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
            }


            return result;
        }
    }
}
