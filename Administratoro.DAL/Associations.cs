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
    
    public partial class Associations
    {
        public Associations()
        {
            this.Apartments = new HashSet<Apartments>();
            this.AssociationCounters = new HashSet<AssociationCounters>();
            this.AssociationExpenses = new HashSet<AssociationExpenses>();
            this.DocumentApartmentFlyers = new HashSet<DocumentApartmentFlyers>();
            this.DocumentAssociationMonthlyExpenses = new HashSet<DocumentAssociationMonthlyExpenses>();
            this.StairCases = new HashSet<StairCases>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public int Id_Partner { get; set; }
        public Nullable<decimal> Indiviza { get; set; }
        public bool HasStaircase { get; set; }
        public string BanckAccont { get; set; }
        public string FiscalCode { get; set; }
        public Nullable<decimal> CotaIndivizaAparments { get; set; }
        public Nullable<bool> HasRoundUpColumn { get; set; }
        public Nullable<decimal> penaltyRate { get; set; }
    
        public virtual ICollection<Apartments> Apartments { get; set; }
        public virtual ICollection<AssociationCounters> AssociationCounters { get; set; }
        public virtual ICollection<AssociationExpenses> AssociationExpenses { get; set; }
        public virtual ICollection<DocumentApartmentFlyers> DocumentApartmentFlyers { get; set; }
        public virtual ICollection<DocumentAssociationMonthlyExpenses> DocumentAssociationMonthlyExpenses { get; set; }
        public virtual Partners Partners { get; set; }
        public virtual ICollection<StairCases> StairCases { get; set; }
    }
}
