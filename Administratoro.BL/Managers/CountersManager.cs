
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
            // if id = -1 -> add id
            // if counterid = -1 remove id
            // else update it
            foreach (ApartmentCounters apCounter in counters)
            {
                if (apCounter.Id == -1 && apCounter.Id_Counters != -1)
                {
                    GetContext().ApartmentCounters.Add(apCounter);
                    GetContext().SaveChanges();
                }
                else
                {
                    var ap = GetContext(true).ApartmentCounters.FirstOrDefault(a => a.Id == apCounter.Id);

                    if (ap != null && apCounter.Id_Counters == -1)
                    {
                        var apRemove = GetContext(true).ApartmentCounters.FirstOrDefault(a => a.Id == apCounter.Id);
                        GetContext().ApartmentCounters.Remove(apRemove);
                        GetContext().SaveChanges();

                    }
                    else if (ap != null)
                    {
                        UpdateApartmentCounters(ap.Id, apCounter);
                    }
                }
            }

        }

        private static void UpdateApartmentCounters(int apCounterId, ApartmentCounters newApCounter)
        {
            var oldApCounter = GetContext(true).ApartmentCounters.FirstOrDefault(c => c.Id == apCounterId);

            if (oldApCounter != null)
            {
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

        public static List<Counters> GetAllByExpenseType(int estateId, int expense)
        {
            return GetContext(true).Counters.Where(c => c.Id_Estate == estateId && c.Id_Expense == expense).ToList();
        }

        public static List<Counters> GetByApartment(int apartmentId)
        {
            var result= new List<Counters>();

            var allApartmentcounters = GetContext(true).ApartmentCounters.Where(ac=>ac.Id_Apartment == apartmentId).ToList();

            foreach (var ac in allApartmentcounters)
	        {
                var counter = GetById(ac.Id_Counters);

                if(counter!=null)
                {
                    result.Add(counter);
                }
	        }

            return result;
        }
    }
}
