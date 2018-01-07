

namespace Administratoro.BL.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class ExpenseReport
    {
        public int NrPers { get; set; }

        public string Ap { get; set; }

        public decimal? Administrare { get; set; }

        public decimal? ColdWather { get; set; }

        public decimal? Cleaning { get; set; }

        public decimal? Gas { get; set; }

        public decimal? Heat { get; set; }

        public decimal? Trash { get; set; }
    }
}
