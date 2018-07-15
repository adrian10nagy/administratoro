

namespace Administratoro.BL.Managers
{
    using DAL;
    using System.Collections.Generic;
    using System.Linq;

    public static class InvoiceIndexesManager
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

        public static IEnumerable<InvoiceIndexes> Get(int invoiceId)
        {
            return GetContext().InvoiceIndexes.Where(t => t.Id_Invoice == invoiceId);
        }

        public static IEnumerable<InvoiceIndexes> Get(Invoices invoice, int? stairCase)
        {
            var result = new List<InvoiceIndexes>();
            // find which counter has on that stairCase
            var assocCounter = AssociationCountersManager.GetByExpenseAndStairCase(invoice, stairCase);

            if (assocCounter != null)
            {
                // get Invoice inces
                result = GetByInvoiceAndCounter(invoice, assocCounter);
            }

            return result;
        }

        public static List<InvoiceIndexes> GetByInvoiceAndCounter(Invoices invoice, AssociationCounters assocCounter)
        {
            return GetContext().InvoiceIndexes.Where(t => t.Id_Invoice == invoice.Id && t.AssociationCounters.Id == assocCounter.Id).ToList();
        }

        public static InvoiceIndexes GetByInvoiceAndCounterFirst(Invoices invoice, AssociationCounters assocCounter)
        {
            return GetContext().InvoiceIndexes.FirstOrDefault(t => t.Id_Invoice == invoice.Id && t.AssociationCounters.Id == assocCounter.Id);
        }

        private static IEnumerable<InvoiceIndexes> GetLastMonthIndexes(int invoiceId)
        {
            var result = new List<InvoiceIndexes>();

            var invoice = InvoicesManager.GetPreviousMonthById(invoiceId);
            if (invoice != null)
            {
                result = invoice.InvoiceIndexes.ToList();
            }

            return result;
        }

        #endregion

        #region Add

        private static void AddDefault(int invoiceId, List<AssociationCounters> counters, List<InvoiceIndexes> invoicesIndexes)
        {
            var lastMonthIndexes = GetLastMonthIndexes(invoiceId);

            foreach (var counter in counters)
            {
                var exists = invoicesIndexes.Any(ii => ii.Id_Counter == counter.Id);
                var lastMonthIndex = lastMonthIndexes.FirstOrDefault(lm => lm.Id_Counter == counter.Id);

                if (!exists)
                {
                    var result = new InvoiceIndexes
                    {
                        Id_Invoice = invoiceId,
                        Id_Counter = counter.Id,
                        IndexOld = lastMonthIndex != null ? lastMonthIndex.IndexNew : null
                    };
                    GetContext().InvoiceIndexes.Add(result);
                }
            }

            GetContext().SaveChanges();
        }

        #endregion

        #region Update

        public static void Update(int invoiceIndexId, decimal? indexOld, decimal? indexNew)
        {
            InvoiceIndexes result;
            result = GetContext(true).InvoiceIndexes.FirstOrDefault(c => c.Id == invoiceIndexId);

            if (result != null)
            {
                result.IndexNew = indexNew;
                result.IndexOld = indexOld;
                GetContext().Entry(result).CurrentValues.SetValues(result);
                GetContext().SaveChanges();
            }
        }

        #endregion

        public static List<InvoiceIndexes> ConfigureWatherCold(List<Invoices> invoices, List<InvoiceIndexes> invoicesIndexes, List<AssociationCounters> counters)
        {
            if (invoicesIndexes.Count < counters.Count)
            {
                AddDefault(invoices[0].Id, counters, invoicesIndexes);
                invoicesIndexes = Get(invoices[0].Id).ToList();
            }
            return invoicesIndexes;
        }
    }
}
