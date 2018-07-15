
namespace Administratoro.BL.Managers
{
    using DAL;
    using System.Linq;

    public static class ApartmentCountersManager
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

        internal static AssociationCountersApartment GetByApartmentAndExpense(int apartmentid, int expenseId)
        {
            return GetContext(true).AssociationCountersApartment.FirstOrDefault(aca => aca.Id_Apartment == apartmentid && aca.AssociationCounters.Id_Expense == expenseId);
        }

        public static AssociationCountersApartment Get(int apartmentId, int associationCounterId)
        {
            return GetContext(true).AssociationCountersApartment.FirstOrDefault(a => a.Id_Apartment == apartmentId && a.Id_Counters == associationCounterId);
        }
    }
}
