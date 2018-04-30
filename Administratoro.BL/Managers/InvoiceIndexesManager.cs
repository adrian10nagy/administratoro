

namespace Administratoro.BL.Managers
{
    using Administratoro.BL.Constants;
using Administratoro.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static IEnumerable<InvoiceIndexes> Get(int invoiceId, int? stairCase)
        {
            return GetContext().InvoiceIndexes.Where(t => t.Id_Invoice == invoiceId && t.AssociationCounters.Id_StairCase == stairCase);
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

        public static void Add(int invoiceId, decimal? indexOld, decimal? indexNew)
        {
            var result = new InvoiceIndexes
            {
                Id_Invoice = invoiceId,
                IndexOld = indexOld,
                IndexNew = indexNew
            };

            GetContext().InvoiceIndexes.Add(result);
            GetContext().SaveChanges();
        }

        public static void AddDefault(int invoiceId, List<AssociationCounters> counters, List<InvoiceIndexes> invoicesIndexes)
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

        public static void Update(int invoiceIndexId, int invoiceId, decimal? indexOld, decimal? indexNew)
        {
            InvoiceIndexes result = new InvoiceIndexes();
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
    }
}
