using System;
using System.Collections.Generic;
using System.Linq;
using Administratoro.DAL;
using Administratoro.BL.Constants;

namespace Administratoro.BL.Managers
{
    public static class ApartmentDebtsManager
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

        #region Get

        public static ApartmentsDebt Get(int apartmentId, int year, int month, DebtType debtType)
        {
            return GetContext(true).ApartmentsDebt.FirstOrDefault(a => a.Id_Apartament == apartmentId &&
                                                                       a.Year == year && a.Month == month &&
                                                                       a.Id_debtType == (int)debtType);
        }

        public static IEnumerable<ApartmentsDebt> Get(int apartmentId, bool isPayed)
        {
            return GetContext(true).ApartmentsDebt.Where(a => a.Id_Apartament == apartmentId && a.IsPayed == isPayed);
        }

        internal static void CalculateAndAddAll(int associationId, int year, int month)
        {
            var apartments = ApartmentsManager.GetByAssociationId(associationId);
            var debtTypes = GetAllTypes();

            foreach (var apartment in apartments)
            {
                foreach (var debtType in debtTypes)
                {
                    decimal? value = GetValue(apartment, year, month, debtType);
                    Add(apartment.Id, debtType.Id, year, month, value);
                }
            }
        }

        private static decimal? GetValue(Apartments apartment, int year, int month, DebtTypes debtType)
        {
            decimal? value = null;
            int twoMonthsAgo = month - 2;
            int yearOfTwoMonthsAgo = year;

            if (month - 2 < 1)
            {
                yearOfTwoMonthsAgo = year - 1;
                twoMonthsAgo = month - 2 + 12;
            }

            switch (debtType.Id)
            {
                case (int)DebtType.LunarExpense:
                    break;
                case (int)DebtType.Restants:
                    value = GetAllUnpaiedOfType(apartment, yearOfTwoMonthsAgo, twoMonthsAgo + 1,
                        DebtType.LunarExpense).Sum(ad => ad.Value);
                    break;
                case (int)DebtType.Penalties:
                    if (!apartment.Associations.penaltyRate.HasValue) { return null; }

                    decimal? intermediatevalue = 0m;

                    var apDebts = GetAllUnpaiedOfType(apartment, yearOfTwoMonthsAgo, twoMonthsAgo,
                        DebtType.LunarExpense);
                    foreach (var apDebt in apDebts)
                    {
                        if (!apDebt.Value.HasValue) continue;
                        var valueToAdd = apDebt.Value.Value * 30 * apartment.Associations.penaltyRate.Value;
                        intermediatevalue = intermediatevalue + valueToAdd;
                    }

                    break;
                case (int)DebtType.RulmentFond:
                    //see how we should store it
                    // determin date of finish 
                    break;
                case (int)DebtType.Repairfond:
                    //see how we should store it
                    // determin date of finish 
                    break;
            }

            return value;
        }

        private static List<DebtTypes> GetAllTypes()
        {
            return GetContext(true).DebtTypes.ToList();
        }

        internal static IEnumerable<ApartmentsDebt> GetAllUnpaiedOfType(Apartments apartment, int year, int month,
            DebtType debtType)
        {
            return GetContext(true).ApartmentsDebt.Where(ad => ad.Id_debtType == (int)debtType &&
                ad.Id_Apartament == apartment.Id && ad.Year <= year && ad.Month <= month && !ad.IsPayed);
        }

        #endregion

        internal static void Add(int apartmentId, int debtTypeId, int year, int month, decimal? value)
        {
            ApartmentsDebt apDebt = new ApartmentsDebt
                {
                    Id_Apartament = apartmentId,
                    Id_debtType = debtTypeId,
                    IsPayed = false,
                    Year = year,
                    Month = month,
                    Value = value
                };

            GetContext().ApartmentsDebt.Add(apDebt);
            GetContext().SaveChanges();
        }

        public static void Pay(int apartmentId, List<Tuple<int, int, int, decimal, decimal?>> debtsList)
        {
            foreach (var tuple in debtsList)
            {
                if (tuple.Item3 == (int)DebtType.AdvancePay)
                {
                    Add(apartmentId, tuple.Item3, tuple.Item1, tuple.Item2, tuple.Item4);
                }

                var apDebt = Get(apartmentId, tuple.Item1, tuple.Item2, (DebtType)tuple.Item3);
                if (apDebt == null) continue;

                if (tuple.Item5.HasValue)
                {
                    apDebt.IsPayed = false;
                    apDebt.RemainingToPay = tuple.Item5.Value;
                    GetContext().Entry(apDebt).CurrentValues.SetValues(apDebt);

                    GetContext().SaveChanges();
                }
                else if (apDebt.Value == tuple.Item4)
                {
                    apDebt.IsPayed = true;
                    GetContext().Entry(apDebt).CurrentValues.SetValues(apDebt);

                    GetContext().SaveChanges();
                }
            }
        }

    }
}
