﻿
using System.Security.Cryptography;

namespace Administratoro.BL.Managers
{
    using DAL;
    using System.Collections.Generic;
    using System.Linq;
    using Constants;

    public static class ApartmentsManager
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

        public static void Update(Apartments apartment)
        {
            var result = GetContext().Apartments.FirstOrDefault(b => b.Id == apartment.Id);

            if (result != null)
            {
                result.Email = apartment.Email;
                result.ExtraInfo = apartment.ExtraInfo;
                result.Telephone = apartment.Telephone;
                result.Dependents = apartment.Dependents;
                result.Name = apartment.Name;
                result.Password = apartment.Password;
                result.CotaIndiviza = apartment.CotaIndiviza;
                result.FondReparatii = apartment.FondReparatii;
                result.FondRulment = apartment.FondRulment;
                GetContext().Entry(result).CurrentValues.SetValues(apartment);

                GetContext().SaveChanges();
            }
        }

        public static void UpdateFonds(int apartamentId, DebtType debtType, decimal amount)
        {
            var result = GetContext().Apartments.FirstOrDefault(b => b.Id == apartamentId);

            if (result != null)
            {
                if (debtType == DebtType.RulmentFond)
                {
                    if(result.FondRulment == null)
                    {
                        result.FondRulment = 0m;
                    }
                    result.FondRulment = result.FondRulment + amount;
                }
                else if (debtType == DebtType.Repairfond)
                {
                    if (result.FondReparatii == null)
                    {
                        result.FondReparatii = 0m;
                    }
                    result.FondReparatii = result.FondReparatii + amount;
                }

                GetContext().Entry(result).CurrentValues.SetValues(result);

                GetContext().SaveChanges();
            }
        }

        public static Apartments Add(Apartments apartment)
        {
            var result = GetContext().Apartments.Add(apartment);
            GetContext().SaveChanges();

            return result;
        }

        public static List<Apartments> GetAllThatAreRegisteredWithSpecificCounters(int associationId, int assExpenseId, int? stairCase = null)
        {
            var result = new List<Apartments>();

            AssociationExpenses associationExpense = AssociationExpensesManager.GetById(assExpenseId);
            if (associationExpense != null)
            {
                List<Apartments> allApartments;
                if (stairCase.HasValue)
                {
                    allApartments = Get(associationId, stairCase.Value);
                }
                else
                {
                    allApartments = Get(associationId);
                }

                foreach (var apartment in allApartments)
                {
                    IEnumerable<AssociationCounters> counters = AssociationCountersManager.GetByApartment(apartment.Id);

                    if (counters.Any(c => c.Id_Expense == associationExpense.Expenses.Id))
                    {
                        result.Add(apartment);
                    }

                }
            }

            return result;
        }

        public static List<Apartments> Get(int associationId)
        {
            return DAL.SDK.Kit.Instance.Apartments.GetByAss(associationId);
        }

        public static List<Apartments> Get(int associationId, int stairCaseId)
        {
            return DAL.SDK.Kit.Instance.Apartments.GetByAss(associationId, stairCaseId);
        }

        public static decimal GetSumOfIndivizaForAllApartments(int associationId)
        {
            return DAL.SDK.Kit.Instance.Apartments.GetSumOfIndiviza(associationId);
        }

        public static IEnumerable<Apartments> GetAllEnabledForHeatHelp(int associationId)
        {
            return DAL.SDK.Kit.Instance.Apartments.GetAllEnabledForHeatHelp(associationId);
        }

        public static int GetDependentsNr(int associationId, int? stairCase)
        {
            return Administratoro.DAL.SDK.Kit.Instance.Apartments.GetDependentsNr(associationId, stairCase);
        }

        public static Apartments GetById(int id)
        {
            return DAL.SDK.Kit.Instance.Apartments.Get(id);
        }

        public static Apartments GetByIdAndPassword(int id, string password)
        {
            if (id.ToString().StartsWith("100"))
            {
                id = int.Parse(id.ToString().Substring(2, id.ToString().Length - 2));
            }

            return GetContext(true).Apartments.FirstOrDefault(a => a.Id == id && a.Password == password);
        }

        public static IEnumerable<Apartments> GetForIndividual(int associationId, int associationExpenseId)
        {
            var result = Enumerable.Empty<Apartments>();
            var associationExpense = AssociationExpensesManager.GetById(associationExpenseId);
            if (associationExpense != null)
            {
                if (associationExpense.Id_Expense == (int)Expense.AjutorÎncălzire)
                {
                    result = GetAllEnabledForHeatHelp(associationId);
                }
                else
                {
                    result = GetAllThatAreRegisteredWithSpecificCounters(associationId, associationExpenseId);
                }
            }

            return result;
        }

        internal static IEnumerable<Apartments> GetByAssociationId(int associationId)
        {
            return GetContext(true).Apartments.Where(a => a.id_Estate == associationId);
        }
    }
}
