using Administratoro.BL.Constants;
using Administratoro.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Administratoro.BL.Managers
{
    public static class InvoicesManager
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

        public static Invoices GetById(int invoiceId)
        {
            return GetContext(true).Invoices.FirstOrDefault(t => t.Id == invoiceId);
        }

        public static IEnumerable<Invoices> GetByAssociationExpenseId(int associationExpenseId)
        {
            return GetContext(true).Invoices.Where(t => t.Id_EstateExpense == associationExpenseId);
        }

        public static IEnumerable<Invoices> GetAllByAssotiationYearMonthExpenseId(int associationId, int expenseId, int year, int month)
        {
            return GetContext().Invoices.Where(i => i.AssociationExpenses.Id_Estate == associationId &&
                i.AssociationExpenses.Month == month && i.AssociationExpenses.Year == year && i.AssociationExpenses.Id_Expense == expenseId);
        }

        public static Invoices GetAllByAssotiationYearMonthExpenseIdstairCase(int associationId, int expenseId, int year, int month, int stairsCaseId)
        {
            return GetContext().Invoices.FirstOrDefault(i => i.AssociationExpenses.Id_Estate == associationId &&
                i.AssociationExpenses.Month == month && i.AssociationExpenses.Year == year && i.AssociationExpenses.Id_Expense == expenseId
                && i.Id_StairCase == stairsCaseId);
        }

        public static void Update(Invoices invoice, decimal? value, int? stairCaseId, string description, int? redistributionId,
            string issueNumber, DateTime? issueDate)
        {
            Invoices result = new Invoices();
            result = GetContext(true).Invoices.FirstOrDefault(c => c.Id == invoice.Id);

            if (result != null)
            {
                result.Value = value;
                result.Description = description;
                result.Id_StairCase = stairCaseId;
                result.id_Redistributiontype = redistributionId;
                result.issueDate = issueDate;
                result.issueNumber = issueNumber;

                GetContext().Entry(result).CurrentValues.SetValues(result);

                if (invoice.Id_EstateExpense.HasValue)
                {
                    var ee = AssociationExpensesManager.GetById(invoice.Id_EstateExpense.Value);
                    if (ee != null)
                    {
                        ApartmentExpensesManager.UpdateApartmentExpenses(ee, value, stairCaseId);
                    }
                }
                GetContext().SaveChanges();

            }
        }

        public static void AddOrUpdate(AssociationExpenses associationExpense, decimal? value, string description = null)
        {
            Invoices result = new Invoices();
            result = GetContext(true).Invoices.FirstOrDefault(c => c.Id_EstateExpense == associationExpense.Id && c.Id_StairCase == null);

            if (result != null)
            {
                result.Value = value;
                result.Description = description;
                GetContext().Entry(result).CurrentValues.SetValues(result);
            }
            else
            {
                result = new Invoices
                {
                    Id_EstateExpense = associationExpense.Id,
                    Value = value,
                    Id_StairCase = null,
                    Description = description
                };
                GetContext().Invoices.Add(result);
            }

            ApartmentExpensesManager.UpdateApartmentExpenses(associationExpense, value);

            GetContext().SaveChanges();
        }

        public static void Update(Invoices invoice, decimal? value, int stairCaseId)
        {
            Invoices theInvoice = new Invoices();

            theInvoice = GetContext(true).Invoices.FirstOrDefault(c => c.Id_StairCase == stairCaseId && c.Id == invoice.Id);

            if (theInvoice != null)
            {
                theInvoice.Value = value;
                GetContext().Entry(theInvoice).CurrentValues.SetValues(theInvoice);


                ApartmentExpensesManager.UpdateApartmentExpenses(invoice.AssociationExpenses, value, stairCaseId);

                GetContext().SaveChanges();
            }
        }

        public static void AddOrUpdate(AssociationExpenses associationExpense, decimal? value, int? stairCaseId, string description = null,
            string issueNumber = null, DateTime? issueDate = null)
        {
            Invoices invoice = new Invoices();

            invoice = GetContext(true).Invoices.FirstOrDefault(c => c.Id_StairCase == stairCaseId && c.Id_EstateExpense == associationExpense.Id);

            if (invoice != null)
            {
                invoice.Value = value;
                invoice.Description = description;
                invoice.issueDate = issueDate;
                invoice.issueNumber = issueNumber;
                GetContext().Entry(invoice).CurrentValues.SetValues(invoice);
            }
            else
            {
                invoice = new Invoices
                {
                    Id_StairCase = stairCaseId,
                    Value = value,
                    Id_EstateExpense = associationExpense.Id,
                    Description = description
                };
                GetContext().Invoices.Add(invoice);
            }

            ApartmentExpensesManager.UpdateApartmentExpenses(associationExpense, value, stairCaseId);

            GetContext().SaveChanges();
        }

        public static void Add(decimal? theValue, int year, int month, int expenseId, int? stairCaseId, int associationId)
        {
            var associationExpense = AssociationExpensesManager.GetMonthYearAssoiationExpense(associationId, expenseId, year, month);

            if (associationExpense != null)
            {
                AddOrUpdate(associationExpense, theValue, stairCaseId);
            }
        }

        public static void AddDefault(int associationId, int expenseId, int year, int month)
        {
            var associationExpense = AssociationExpensesManager.GetMonthYearAssoiationExpense(associationId, expenseId, year, month);

            if (associationExpense != null)
            {
                //if (estateExpense.SplitPerStairCase.HasValue && estateExpense.SplitPerStairCase.Value)
                //{
                //    foreach (var stairCase in estateExpense.Associations.StairCases)
                //    {
                //        var result = new Invoices
                //        {
                //            Id_EstateExpense = estateExpense.Id,
                //            Value = null,
                //            Id_StairCase = stairCase.Id
                //        };
                //        GetContext().Invoices.Add(result);
                //    }
                //}
                //else
                //{
                var result = new Invoices
                {
                    Id_EstateExpense = associationExpense.Id,
                    Value = null,
                    Id_StairCase = null
                };
                GetContext().Invoices.Add(result);
                //}

                GetContext().SaveChanges();
            }
        }

        public static IEnumerable<Invoices> GetDiverseByAssociationAssociationExpense(int associationExpenseId)
        {
            return GetContext().Invoices.Where(i => i.Id_EstateExpense == associationExpenseId);
        }

        public static Invoices GetDiverseById(int invoiceId)
        {
            return GetContext().Invoices.FirstOrDefault(i => i.Id == invoiceId);
        }

        public static IEnumerable<Invoices> GetDiverseByAssociationYearMonth(int association, int year, int month)
        {
            var result = new List<Invoices>();
            int divereId = 24;

            var esExpense = GetContext(true).AssociationExpenses.FirstOrDefault(ee => ee.Id_Expense == divereId
                && ee.Id_Estate == association && ee.Year == year && ee.Month == month);
            if (esExpense != null)
            {
                result = esExpense.Invoices.ToList();
            }

            return result;
        }

        public static void AddDiverse(AssociationExpenses ee, decimal? theValue, string description, int? stairCaseId, int? redistributionId = null,
            string issueNumber = null, DateTime? issueDate = null)
        {
            var invoice = new Invoices
            {
                Id_StairCase = stairCaseId,
                Value = theValue,
                Id_EstateExpense = ee.Id,
                Description = description,
                id_Redistributiontype = redistributionId,
                issueNumber = issueNumber,
                issueDate = issueDate
            };
            GetContext().Invoices.Add(invoice);
            GetContext().SaveChanges();
        }

        public static void Remove(int invoiceId)
        {
            var invoice = GetContext(true).Invoices.FirstOrDefault(i => i.Id == invoiceId);
            if (invoice != null)
            {
                GetContext().Invoices.Remove(invoice);
                GetContext().SaveChanges();
            }
        }

        public static Invoices GetByIndexInvoiceId(int indexInvoiceId)
        {
            Invoices result = null;
            var invoiceIndex = GetContext(true).InvoiceIndexes.FirstOrDefault(i => i.Id == indexInvoiceId);
            if (invoiceIndex != null)
            {
                result = invoiceIndex.Invoices;
            }

            return result;
        }

        public static Invoices GetPreviousMonthById(int invoiceId)
        {
            Invoices result = null;

            var invoice = GetById(invoiceId);

            if (invoice != null)
            {
                var ee = AssociationExpensesManager.GetById(invoice.Id_EstateExpense.Value);
                var month = ee.Month;
                var year = ee.Year;

                if (month == 1)
                {
                    month = 12;
                    year = year - 1;
                }
                else
                {
                    month = month - 1;
                }

                var associationExpense = AssociationExpensesManager.GetAssociationExpense(ee.Id_Estate, ee.Id_Expense, year, month);
                if(associationExpense != null)
                {
                    result = associationExpense.Invoices.FirstOrDefault();
                }
            }

            return result;
        }

        public static IEnumerable<Invoices> GetAllByAssotiationYearMonth(int associationId, int year, int month)
        {
            return GetContext().Invoices.Where(i => i.AssociationExpenses.Id_Estate == associationId &&
                            i.AssociationExpenses.Month == month && i.AssociationExpenses.Year == year);
        }

        public static Invoices GetByAssociationExpenseForExpense(AssociationExpenses associationExpense, Expense expense)
        {
            var ae = AssociationExpensesManager.GetForSameMonthByExpense(associationExpense.Id, expense);

            return ae.Invoices.FirstOrDefault();
        }
    }
}
