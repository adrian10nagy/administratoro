
namespace Administratoro.BL.Managers
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Models;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;

    public static class ExpensesManager
    {
        private static AdministratoroEntities _administratoroEntities;

        private static AdministratoroEntities GetContext()
        {
            if (_administratoroEntities == null)
            {
                _administratoroEntities = new AdministratoroEntities();
            }

            return _administratoroEntities;
        }

        public static DbSet<Expenses> GetAllExpenses()
        {
            return GetContext().Expenses;
        }

        public static List<Expenses> GetAllExpensesAsList()
        {
            return GetContext().Expenses.ToList();
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

        public static Expenses GetExpenseByName(string expenseName)
        {
            var allTenantExpenses = GetAllExpenses();
            var expense = allTenantExpenses.Where(e => e.Name == expenseName).ToList();
            if (expense.Count() > 0)
            {
                return expense[0];
            }
            else
            {
                //// log error
                return null;
            }
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
                && ee.Month == month && ee.Year == year).ToList();
            EstateExpenses estateExpense;
            if (estateExpenses != null && estateExpenses.Count > 0)
            {
                estateExpense = estateExpenses[0];

                TenantExpenses tenantExpense = new TenantExpenses
                {
                    Value = null,
                    Id_Tenant = tenant.Id,
                    Id_EstateExpense = estateExpense.Id
                };

                GetContext().TenantExpenses.Add(tenantExpense);
                GetContext().SaveChanges();
            }
        }

        public static void AddTenantExpense(int apartmentid, int year, int month, object theExpense, decimal expenseValue)
        {
            List<Tenants> tenants = GetContext().Tenants.Where(t => t.Id == apartmentid).ToList();
            Expenses expense = (Expenses)theExpense;
            Tenants tenant;
            if (tenants != null && tenants.Count == 1)
            {
                tenant = tenants[0];
                List<EstateExpenses> estateExpenses = GetContext().EstateExpenses.Where(ee => ee.Id_Estate == tenant.id_Estate
                    && ee.Id_Expense == expense.Id && ee.Month == month && ee.Year == year).ToList();
                EstateExpenses estateExpense;
                if (estateExpenses != null && estateExpenses.Count == 1)
                {
                    estateExpense = estateExpenses[0];
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

        public static Expenses GetById(int expenseId)
        {
            return GetContext().Expenses.FirstOrDefault(e => e.Id == expenseId);
        }

        public static DataTable GetAllExpensesAsDatatable(int estateId)
        {
            int year = 2017;
            int month = 9;
            DataTable dt = new DataTable();

            // todo
            var tenants = ApartmentsManager.GetAllByEstateId(estateId);

            foreach (var tenant in tenants)
            {
                var expenses = ExpensesManager.GetAllExpensesByTenantAndMonth(tenant.Id, year, month);
                string query = string.Empty;
                if (expenses.Count == 0)
                {
                    ExpensesManager.AddDefaultTenantExpense(tenant, year, month);
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

        public static DataTable GetAllEpensesAsList(int estateId)
        {
            var rowData = GetAllExpensesAsDatatable(estateId);

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
                expenseReport.Administrare = adminResult;

                var coldWather = rowData.Rows[i]["Apă rece"];
                decimal? coldWatherResult = null;
                if (coldWather != null && !string.IsNullOrEmpty(coldWather.ToString()))
                {
                    coldWatherResult = Convert.ToDecimal(coldWather.ToString());
                }
                expenseReport.ColdWather = coldWatherResult;

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
                dt.Rows.Add(item.Ap, item.NrPers, item.Administrare, item.ColdWather, item.Cleaning, item.Gas, item.Heat, item.Trash, item.Ap);
            }

            return dt;
        }
    }
}
