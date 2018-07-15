
namespace Administratoro.BL.Models
{
    using DAL;
    using System.Collections.Generic;

    public class FormatedAssociationCounter : AssociationCounters
    {
        public List<int> AssociationCounterStairCaseIds { get; set; }
        public string AssociationCounterStairCaseIdsString { get; set; }

        public FormatedAssociationCounter(AssociationCounters item)
        {
            List<int> result = new List<int>();

            foreach (var assCounterStairCase in item.AssociationCountersStairCase)
            {
                int toAppend = -1;

                if (assCounterStairCase.Id_StairCase.HasValue)
                {
                    toAppend = assCounterStairCase.Id_StairCase.Value;
                }

                result.Add(toAppend);
            }

            AssociationCounterStairCaseIds = result;
            AssociationCounterStairCaseIdsString = string.Join(",", result);

            this.AssociationCountersApartment = item.AssociationCountersApartment;
            this.AssociationCountersStairCase = item.AssociationCountersStairCase;
            this.Associations = item.Associations;
            this.Expenses = item.Expenses;
            this.Id = item.Id;
            this.Id_Estate = item.Id_Estate;
            this.Id_Expense = item.Id_Expense;
            this.InvoiceIndexes = item.InvoiceIndexes;
            this.Value = item.Value;
        }
    }
}
