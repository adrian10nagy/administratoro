
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

        #region remove

        private static void Remove(AssociationCountersApartment apRemove)
        {
            GetContext().AssociationCountersApartment.Remove(apRemove);
            GetContext().SaveChanges();
        }

        private static void Remove(AssociationCountersStairCase scRemove)
        {
            GetContext().AssociationCountersStairCase.Remove(scRemove);
            GetContext().SaveChanges();
        }

        #endregion

        #region add

        public static void Addcounter(List<AssociationCounters> counters)
        {
            foreach (AssociationCounters counter in counters)
            {
                AddCounterStairCase(counter);
                Add(counter);
            }
        }

        private static void AddCounterStairCase(AssociationCounters counter)
        {
            foreach (var assCounterSC in counter.AssociationCountersStairCase)
            {
                Add(assCounterSC);
            }
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
                    Add(apCounter);
                }
                else
                {
                    var ap = GetContext(true).AssociationCountersApartment.FirstOrDefault(a => a.Id == apCounter.Id);

                    if (ap != null && apCounter.Id_Counters == -1)
                    {
                        var apRemove = GetContext(true).AssociationCountersApartment.FirstOrDefault(a => a.Id == apCounter.Id);
                        Remove(apRemove);

                    }
                    else if (ap != null)
                    {
                        UpdateAssociationCountersApartment(ap.Id, apCounter);
                    }
                }
            }

        }

        public static void Add(AssociationCounters counter)
        {
            GetContext().AssociationCounters.Add(counter);
            GetContext().SaveChanges();
        }

        private static void Add(AssociationCountersStairCase assCounterSC)
        {
            GetContext().AssociationCountersStairCase.Add(assCounterSC);
            GetContext().SaveChanges();
        }

        private static void Add(AssociationCountersApartment apCounter)
        {
            GetContext().AssociationCountersApartment.Add(apCounter);
            GetContext().SaveChanges();
        }

        #endregion

        public static AssociationCounters GetById(int counterId)
        {
            return GetContext().AssociationCounters.FirstOrDefault(c => c.Id == counterId);
        }

        public static void Update(AssociationCounters newCounter)
        {
            var exitingCounter = GetContext(true).AssociationCounters.FirstOrDefault(c => c.Id == newCounter.Id);

            if (exitingCounter != null && exitingCounter.Value != newCounter.Value)
            {
                exitingCounter.Value = newCounter.Value;

                GetContext().Entry(exitingCounter).CurrentValues.SetValues(exitingCounter);
                GetContext().SaveChanges();
            }

            UpdateAssociationCountersStairCase(newCounter);
        }

        private static void UpdateAssociationCountersApartment(int apCounterId, AssociationCountersApartment newApCounter)
        {
            var oldApCounter = GetContext(true).AssociationCountersApartment.FirstOrDefault(c => c.Id == apCounterId);

            if (oldApCounter != null)
            {
                oldApCounter.Id_Counters = newApCounter.Id_Counters;
                oldApCounter.CountersInsideApartment = newApCounter.CountersInsideApartment;
                GetContext().Entry(oldApCounter).CurrentValues.SetValues(oldApCounter);

                GetContext().SaveChanges();
            }
        }

        private static void UpdateAssociationCountersStairCase(AssociationCounters newCounter)
        {
            var exitingCounter = GetContext(true).AssociationCounters.FirstOrDefault(c => c.Id == newCounter.Id);

            var stairCaseToBeAdded = newCounter.AssociationCountersStairCase.Select(n => n.Id_StairCase)
                .Except(exitingCounter.AssociationCountersStairCase.Select(o => o.Id_StairCase));

            var stairCaseToBeDeleted = exitingCounter.AssociationCountersStairCase.Select(n => n.Id_StairCase)
                .Except(newCounter.AssociationCountersStairCase.Select(o => o.Id_StairCase));

            var add = newCounter.AssociationCountersStairCase.Where(ne => stairCaseToBeAdded.Contains(ne.Id_StairCase)).ToList();
            var del = exitingCounter.AssociationCountersStairCase.Where(ne => stairCaseToBeDeleted.Contains(ne.Id_StairCase)).ToList();

            foreach (var assCounterSC in add)
            {
                assCounterSC.Id_AssCounter = exitingCounter.Id;
                Add(assCounterSC);
            }

            foreach (var assCounterSC in del)
            {
                Remove(assCounterSC);
            }
        }



        public static IEnumerable<AssociationCounters> GetAllByExpenseType(int associationId, int expense)
        {
            return GetContext(true).AssociationCounters.Where(c => c.Id_Estate == associationId && c.Id_Expense == expense);
        }

        public static IEnumerable<AssociationCounters> GetByApartment(int apartmentId)
        {
            var result = new List<AssociationCounters>();

            var allAssociationCountersApartment = GetContext(true).AssociationCountersApartment.Where(ac => ac.Id_Apartment == apartmentId);

            foreach (var ac in allAssociationCountersApartment)
            {
                var counter = GetById(ac.Id_Counters);

                if (counter != null)
                {
                    result.Add(counter);
                }
            }

            return result;
        }

        public static AssociationCounters GetByExpenseAndStairCase(Invoices invoice, int? stairCase)
        {
            if (invoice == null)
            {
                return null;
            }

            return GetByExpenseAndStairCase(invoice.AssociationExpenses, stairCase);
        }

        public static AssociationCounters GetByExpenseAndStairCase(AssociationExpenses associationExpenses, int? stairCase)
        {
            if (associationExpenses == null)
            {
                return null;
            }

            return GetByExpenseAndStairCase(associationExpenses.Id_Estate, associationExpenses.Id_Expense, stairCase);
        }

        public static AssociationCounters GetByExpenseAndStairCase(int associationId, int expenseId, int? stairCase)
        {
            AssociationCounters result = null;

            var allAC = GetAllByExpenseType(associationId, expenseId);

            result = allAC.FirstOrDefault(a => a.AssociationCountersStairCase.Any(x => x.Id_StairCase == stairCase));

            if (result == null & allAC.Count() == 1)
            {
                result = allAC.FirstOrDefault();
            }

            return result;
        }
    }
}
