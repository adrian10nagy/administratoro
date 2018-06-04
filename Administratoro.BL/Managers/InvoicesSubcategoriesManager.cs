

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

        public static void Update(InvoiceSubcategories invoiceSubcategory)
        {
            InvoiceSubcategories result = new InvoiceSubcategories();
            result = GetByInvoiceId(invoiceSubcategory.Id_Invoice, invoiceSubcategory.Id_subCategType);

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

                GetContext().Entry(result).CurrentValues.SetValues(result);
                GetContext().SaveChanges();
            }
        }

        public static void AddDefault(int invoiceId)
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
            }

            AddDefault(invoiceId, subcategoryTypes);
        }

        public static void AddDefault(int invoiceId, IEnumerable<int> subcategoryTypes)
        {
            foreach (var subcategoryType in subcategoryTypes)
            {
                AddDefault(invoiceId, subcategoryType);
            }
        }

        public static void AddDefault(int invoiceId, int subcategoryType)
        {
            InvoiceSubcategories result = new InvoiceSubcategories
            {
                Id_Invoice = invoiceId,
                Id_subCategType = subcategoryType
            };

            GetContext().InvoiceSubcategories.Add(result);
            GetContext().SaveChanges();
        }
    }
}
