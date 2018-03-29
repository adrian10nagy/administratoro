
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

        public static void Addcounter(List<AssociationCounters> counters)
        {
            foreach (AssociationCounters counter in counters)
            {
                GetContext().AssociationCounters.Add(counter);
            }

            GetContext().SaveChanges();
        }

        public static void AddOrUpdateAssociationCountersApartment(List<AssociationCountersApartment> counters)
        {
            // if id = -1 -> add id
            // if counterid = -1 remove id
            // else update it
            foreach (AssociationCountersApartment apCounter in counters)
            {
                if (apCounter.Id == -1 && apCounter.Id_Counters != -1)
                {
                    GetContext().AssociationCountersApartment.Add(apCounter);
                    GetContext().SaveChanges();
                }
                else
                {
                    var ap = GetContext(true).AssociationCountersApartment.FirstOrDefault(a => a.Id == apCounter.Id);

                    if (ap != null && apCounter.Id_Counters == -1)
                    {
                        var apRemove = GetContext(true).AssociationCountersApartment.FirstOrDefault(a => a.Id == apCounter.Id);
                        GetContext().AssociationCountersApartment.Remove(apRemove);
                        GetContext().SaveChanges();

                    }
                    else if (ap != null)
                    {
                        UpdateAssociationCountersApartment(ap.Id, apCounter);
                    }
                }
            }

        }

        private static void UpdateAssociationCountersApartment(int apCounterId, AssociationCountersApartment newApCounter)
        {
            var oldApCounter = GetContext(true).AssociationCountersApartment.FirstOrDefault(c => c.Id == apCounterId);

            if (oldApCounter != null)
            {
                oldApCounter.Id_Counters = newApCounter.Id_Counters;
                GetContext().Entry(oldApCounter).CurrentValues.SetValues(oldApCounter);

                GetContext().SaveChanges();
            }
        }

        public static void Addcounter(AssociationCounters counter)
        {
            GetContext().AssociationCounters.Add(counter);

            GetContext().SaveChanges();
        }

        public static AssociationCounters GetById(int counterId)
        {
            return GetContext().AssociationCounters.FirstOrDefault(c => c.Id == counterId);
        }

        public static void Update(AssociationCounters newCounter, int counterId)
        {
            var counter = GetContext(true).AssociationCounters.FirstOrDefault(c => c.Id == counterId);

            if (counter != null)
            {
                counter.Value = newCounter.Value;
                GetContext().Entry(counter).CurrentValues.SetValues(counter);

                GetContext().SaveChanges();
            }
        }

        public static List<AssociationCounters> GetAllByExpenseType(int associationId, int expense)
        {
            return GetContext(true).AssociationCounters.Where(c => c.Id_Estate == associationId && c.Id_Expense == expense).ToList();
        }

        public static List<AssociationCounters> GetByApartment(int apartmentId)
        {
            var result = new List<AssociationCounters>();

            var allAssociationCountersApartment = GetContext(true).AssociationCountersApartment.Where(ac => ac.Id_Apartment == apartmentId).ToList();

            foreach (var ac in allAssociationCountersApartment)
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
