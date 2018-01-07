
namespace Admin.Reports
{
    using Administrataro.BL.Models;
    using Administratoro.BL.Constants;
    using Administratoro.BL.Managers;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI.WebControls;

    public partial class Index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var estate = (Estates)Session[SessionConstants.SelectedEstate];
            var defaultCssClass = "col-md-2 col-sm-3 col-xs-12 cashbookMonth";
            List<YearMonth> yearMonths = EstateExpensesManager.GetAllMonthsAndYeardAvailableByEstateId(estate.Id);
            foreach (var item in yearMonths)
            {
                var month = new Panel
                {
                    CssClass = defaultCssClass
                };
                var link = new LinkButton { Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(item.Month) + " " + item.Year.ToString() };
                link.Click += link_Click;
                link.CommandArgument = item.Year.ToString() + item.Month.ToString();
                link.CssClass = "monthsMainItem";
                month.Controls.Add(link);
                monthsMain.Controls.Add(month);
            }

            var month0 = new Panel
            {
                CssClass = defaultCssClass
            };

            monthsMain.Controls.Add(month0);
        }

        private void link_Click(object sender, EventArgs e)
        {
            LinkButton lb = (LinkButton)sender;
            string year = lb.CommandArgument.Substring(0, 4);
            string month = (lb.CommandArgument.Count() == 5) ? lb.CommandArgument.Substring(4, 1) : lb.CommandArgument.Substring(4, 2);

            Response.Redirect("~/Reports/Monthly.aspx?year=" + year + "&month=" + month);
        }
    }
}