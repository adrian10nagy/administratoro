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
    
    public partial class ApartmentPersons
    {
        public int Id { get; set; }
        public int Id_Person { get; set; }
        public int Id_Tenent { get; set; }
    
        public virtual Persons Persons { get; set; }
        public virtual Apartments Apartments { get; set; }
    }
}