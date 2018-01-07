
namespace Administratoro.BL.Managers
{
    using Administratoro.DAL;
    using System.Collections.Generic;
    using System.Linq;

    public static class CountersManager
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

        public static void Addcounter(List<Counters> counters)
        {
            foreach (Counters counter in counters)
            {
                GetContext().Counters.Add(counter);
            }

            GetContext().SaveChanges();
        }

        public static void AddOrUpdateApartmentCounters(List<ApartmentCounters> counters)
        {
            foreach (ApartmentCounters apCounter in counters)
            {
                var ap = GetContext(true).ApartmentCounters.FirstOrDefault(a=>a.Id == apCounter.Id);
                if(ap!=null)
                {
                    UpdateApartmentCounters(ap.Id, apCounter);
                }
                else
                {
                    GetContext().ApartmentCounters.Add(apCounter);
                }
            }

            GetContext().SaveChanges();
        }

        private static void UpdateApartmentCounters(int apCounterId, ApartmentCounters newApCounter)
        {
            var oldApCounter = GetContext(true).ApartmentCounters.FirstOrDefault(c => c.Id == apCounterId);

            if (oldApCounter != null)
            {
                oldApCounter.Id_Apartment = newApCounter.Id_Apartment;
                oldApCounter.Id_Counters = newApCounter.Id_Counters;
                GetContext().Entry(oldApCounter).CurrentValues.SetValues(oldApCounter);

                GetContext().SaveChanges();
            }
        }

        public static void Addcounter(Counters counter)
        {
            GetContext().Counters.Add(counter);

            GetContext().SaveChanges();
        }

        public static Counters GetById(int counterId)
        {
            return GetContext().Counters.FirstOrDefault(c => c.Id == counterId);
        }

        public static void Update(Counters newCounter, int counterId)
        {
            var counter = GetContext(true).Counters.FirstOrDefault(c => c.Id == counterId);

            if (counter != null)
            {
                counter.Value = newCounter.Value;
                GetContext().Entry(counter).CurrentValues.SetValues(counter);

                GetContext().SaveChanges();
            }
        }
    }
}
