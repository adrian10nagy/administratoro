

namespace Administratoro.BL.Managers
{
    using DAL;
    using DAL.SDK;
    using System;

    public static class RegistriesHomeDailyManager
    {
        private static AdministratoroEntities _administratoroEntities;

        public static int? Add(DAL.RegistriesHomeDaily registryHomeDaily)
        {
            int? id = null;
            if (registryHomeDaily != null)
            {
                id = Kit.Instance.RegistriesHomeDaily.Add(registryHomeDaily);
            }

            return id;
        }

        public static DAL.RegistriesHomeDaily Get(int assId, DateTime date)
        {
            return Kit.Instance.RegistriesHomeDaily.Get(assId, date);
        }

        public static DAL.RegistriesHomeDaily GetById(int id)
        {
            return Kit.Instance.RegistriesHomeDaily.GetById(id);
        }

        public static void RefreshOpenClosePricesFromDate(int dailyRegId)
        {
            var dailyRegistry = GetById(dailyRegId);
            if (dailyRegId == null) { return; }

            //false
        }
    }
}
