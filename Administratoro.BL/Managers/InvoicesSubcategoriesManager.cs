

namespace Administratoro.BL.Managers
{
    using Administratoro.BL.Constants;
    using Administratoro.DAL;
    using System.Collections.Generic;
    using System.Linq;

    public static class InvoicesSubcategoriesManager
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

        public static InvoiceSubcategories GetByInvoiceId(int invoiceId, int subCategTypeId)
        {
            return GetContext().InvoiceSubcategories.FirstOrDefault(t => t.Id_Invoice == invoiceId && t.Id_subCategType == subCategTypeId);
        }

        public static InvoiceSubcategories GetByInvoiceId(int invoiceId, int subCategTypeId, int? idAssCount)
        {
            return GetContext().InvoiceSubcategories.FirstOrDefault(t => t.Id_Invoice == invoiceId && t.Id_subCategType == subCategTypeId && t.id_assCounter == idAssCount);
        }

        public static void Update(InvoiceSubcategories invoiceSubcategory)
        {
            InvoiceSubcategories result = new InvoiceSubcategories();
            result = GetByInvoiceId(invoiceSubcategory.Id_Invoice, invoiceSubcategory.Id_subCategType, invoiceSubcategory.id_assCounter);

            if (result != null && result.Value != invoiceSubcategory.Value ||
                result.quantity != invoiceSubcategory.quantity || result.PricePerUnit != invoiceSubcategory.PricePerUnit ||
                result.VAT != invoiceSubcategory.VAT || result.service != invoiceSubcategory.service || result.penalties != invoiceSubcategory.penalties)
            {
                result.quantity = invoiceSubcategory.quantity;
                result.PricePerUnit = invoiceSubcategory.PricePerUnit;
                result.VAT = invoiceSubcategory.VAT;
                result.service = invoiceSubcategory.service;
                result.penalties = invoiceSubcategory.penalties;
                result.Value = invoiceSubcategory.Value;
                result.id_assCounter = invoiceSubcategory.id_assCounter;

                GetContext().Entry(result).CurrentValues.SetValues(result);
                GetContext().SaveChanges();
            }
        }
        public static void AddDefault(int invoiceId)
        {
            AddDefault(invoiceId, new List<AssociationCounters>());
        }

        public static void AddDefault(int invoiceId, List<AssociationCounters> assCounters)
        {
            List<int> subcategoryTypes = new List<int>();
            var invoice = InvoicesManager.GetById(invoiceId);

            if (invoice == null)
            {
                return;
            }

            if (invoice.AssociationExpenses.Id_Expense == (int)Expense.ApaRece)
            {
                subcategoryTypes = new List<int> { 1, 2, 3, 4 };
                AddDefault(invoiceId, subcategoryTypes);
            }
            else if (invoice.AssociationExpenses.Id_Expense == (int)Expense.ApaCalda)
            {
                subcategoryTypes = new List<int> { 5, 6 };
                foreach (AssociationCounters assCounter in assCounters)
                {
                    AddDefault(invoiceId, subcategoryTypes, assCounter.Id);
                }
            }

        }

        private static void AddDefault(int invoiceId, IEnumerable<int> subcategoryTypes, int? assCounterId = null)
        {
            foreach (var subcategoryType in subcategoryTypes)
            {
                AddDefault(invoiceId, subcategoryType, assCounterId);
            }
        }

        private static void AddDefault(int invoiceId, int subcategoryType, int? assCounterId)
        {
            InvoiceSubcategories result = new InvoiceSubcategories
            {
                Id_Invoice = invoiceId,
                Id_subCategType = subcategoryType,
                id_assCounter = assCounterId
            };

            GetContext().InvoiceSubcategories.Add(result);
            GetContext().SaveChanges();
        }

        public static IEnumerable<Administratoro.DAL.Invoices> ConfigureSubcategories(IEnumerable<Administratoro.DAL.Invoices> invoices, List<AssociationCounters> counters)
        {
            var result = invoices;

            if (invoices.Any())
            {
                foreach (var invoice in invoices)
                {
                    if (invoice.AssociationExpenses.Id_Expense == (int)Expense.ApaRece)
                    {
                        if (invoice.InvoiceSubcategories.Count() == 0)
                        {
                            AddDefault(invoice.Id);
                        }
                        
                    }
                    else if (invoice.AssociationExpenses.Id_Expense == (int)Expense.ApaCalda)
                    {
                        if (invoice.InvoiceSubcategories.Count < counters.Count * 2)
                        {
                            AddDefault(invoice.Id, counters);
                        }
                    }
                }

                result = InvoicesManager.GetByAssociationExpenseId(invoices.FirstOrDefault().Id_EstateExpense.Value);
            }

            return result;
        }

        //public static List<InvoiceIndexes> ConfigureWatherHot(List<Invoices> invoices, List<AssociationCounters> counters)
        //{
        //    foreach (var invoice in invoices)
        //    {
        //        if (invoice.InvoiceSubcategories.Count < counters.Count * 2)
        //        {
        //            AddDefault(invoice.Id, counters);
        //        }
        //    }

        //    return null;
        //}
    }
}
