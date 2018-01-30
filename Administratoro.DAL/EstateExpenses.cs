//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Administratoro.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class EstateExpenses
    {
        public EstateExpenses()
        {
            this.Invoices = new HashSet<Invoices>();
            this.TenantExpenses = new HashSet<TenantExpenses>();
        }
    
        public int Id { get; set; }
        public int Id_Estate { get; set; }
        public int Id_Expense { get; set; }
        public int Id_ExpenseType { get; set; }
        public bool isDefault { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public bool WasDisabled { get; set; }
        public Nullable<decimal> PricePerExpenseUnit { get; set; }
        public Nullable<bool> SplitPerStairCase { get; set; }
        public Nullable<int> RedistributeType { get; set; }
    
        public virtual EstateExpensesRedistributionTypes EstateExpensesRedistributionTypes { get; set; }
        public virtual Estates Estates { get; set; }
        public virtual Expenses Expenses { get; set; }
        public virtual ExpenseTypes ExpenseTypes { get; set; }
        public virtual ICollection<Invoices> Invoices { get; set; }
        public virtual ICollection<TenantExpenses> TenantExpenses { get; set; }
    }
}
