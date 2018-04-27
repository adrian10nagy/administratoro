

namespace Administratoro.BL.Managers
{
    using Administratoro.DAL;
    using System.Collections.Generic;
    using System.Linq;

    public static class CountersApartmentManager
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
    }
}
