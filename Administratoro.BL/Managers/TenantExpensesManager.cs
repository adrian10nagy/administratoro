
namespace Administratoro.BL.Managers
{
    using Administratoro.BL.Constants;
    using Administratoro.DAL;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

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
                AddCotaIndivizaTenantExpenses(ee, totalValue);
            }
        }

        public static void AddCotaIndivizaTenantExpenses(EstateExpenses expenseEstate, decimal? totalValue)
        {
            List<Tenants> tenants = GetContext().Tenants.Where(tt => tt.id_Estate == expenseEstate.Id_Estate).ToList();
            decimal totalCota = tenants.Sum(te => te.CotaIndiviza);

            foreach (var tenant in tenants)
            {
                TenantExpenses tte = GetContext().TenantExpenses.FirstOrDefault(tee => tee.Id_EstateExpense == expenseEstate.Id && tee.Id_Tenant == tenant.Id);
                decimal? valuePerCotaUnit = null;
                if (totalValue.HasValue && totalCota != 0)
                {
                    valuePerCotaUnit = totalValue.Value / totalCota;
                }

                if (tte != null)
                {
                    tte.Value = valuePerCotaUnit.HasValue ? tenant.CotaIndiviza * valuePerCotaUnit : null;
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

        public static void AddPerTenantExpenses(int idExpenseEstate, decimal? valuePerTenant)
        {
            var ee = EstateExpensesManager.GetById(idExpenseEstate);
            if (ee != null)
            {
                List<Tenants> allTenants = ApartmentsManager.GetAllByEstateId(ee.Id_Estate);
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
                ExpensesManager.AddDefaultTenantExpense(tenant.Id, ee.Year, ee.Month, esexId);
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
    }
}
