using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Administratoro.BL.Constants
{
    public enum ExpensesCompletedStatus
    {
        Completed = 100,
        Redistribute = 80,
        Invoices = 50,
        Expenses = 20,
        All = 0
    }
}