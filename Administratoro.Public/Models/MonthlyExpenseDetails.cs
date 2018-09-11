
namespace Administratoro.Public.Models
{
    public class MonthlyExpenseDetails
    {
        public int Month { get; set; }
        public int Year { get; set; }
        public string FilePath { get; set; }
        public decimal? ExpensesTotal { get; set; }

    }
}