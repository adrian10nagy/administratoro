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
    
    public partial class AssociationExpensesRedistributionTypes
    {
        public AssociationExpensesRedistributionTypes()
        {
            this.AssociationExpenses = new HashSet<AssociationExpenses>();
            this.Invoices = new HashSet<Invoices>();
        }
    
        public int Id { get; set; }
        public string Value { get; set; }
    
        public virtual ICollection<AssociationExpenses> AssociationExpenses { get; set; }
        public virtual ICollection<Invoices> Invoices { get; set; }
    }
}