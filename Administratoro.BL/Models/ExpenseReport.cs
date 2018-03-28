

namespace Administratoro.BL.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ExpenseReport
    {
        public string CotaIndiviza { get; set; }

        public int Id { get; set; }
        public int NrPers { get; set; }
        public string Ap { get; set; }
        public string Name { get; set; }
        public decimal? Administrator { get; set; }
        public decimal? WatherCold { get; set; }
        public decimal? WatherWarm { get; set; }
        public decimal? Cleaning { get; set; }
        public decimal? Gas { get; set; }
        public decimal? HeatRAT { get; set; }
        public decimal? Trash { get; set; }
        public decimal? Electricity { get; set; }
        public decimal? Elevator { get; set; }
        public decimal? Utilities { get; set; }
        public decimal? President { get; set; }
        public decimal? Censor { get; set; }
        public decimal? Fireman { get; set; }
        public decimal? ElevatorUtility { get; set; }
        public decimal? Diverse { get; set; }
        public decimal? HeatHelp { get; set; }
    }
}
