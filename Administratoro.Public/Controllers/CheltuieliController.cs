using Administratoro.BL.Managers;
using Administratoro.DAL;
using Administratoro.Public.Models;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Administratoro.Public.Controllers
{
    public class CheltuieliController : BaseController
    {
        public ActionResult Index()
        {
            var months = AssociationExpensesManager.GetAllMonthsAndYearsAvailableByAssociationId(AssociationId);

            return View(months);
        }

        public ActionResult Detalii(string year, string month)
        {
            int yearValue;
            int monthValue;
            var med = new MonthlyExpenseDetails();

            if (int.TryParse(year, out yearValue) && int.TryParse(month, out monthValue))
            {
                var filePath = FilesManager.GetMonthlyExpensefilePath(yearValue, monthValue, ApartmentId);
                med = new MonthlyExpenseDetails
                {
                    Year = yearValue,
                    Month = monthValue,
                    FilePath = filePath,
                    ExpensesTotal = 100,
                };
            }


            return View(med);
        }

        public ActionResult DownloadPDF(string path)
        {
            return File(path, "application/pdf", "p11.pdf");
        }

    }
}