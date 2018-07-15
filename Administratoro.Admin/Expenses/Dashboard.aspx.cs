using Administratoro.BL.Models;
using Administratoro.BL.Constants;
using Administratoro.BL.Managers;
using System;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;

namespace Admin.Expenses
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var estate = (Administratoro.DAL.Associations)Session[SessionConstants.SelectedAssociation];
            var defaultCssClass = "col-md-2 col-sm-3 col-xs-12";
            var yearMonths = AssociationExpensesManager.GetAllMonthsAndYearsAvailableByAssociationId(estate.Id);
            foreach (var yearMonth in yearMonths)
            {
                var month = new Panel
                {
                    CssClass = defaultCssClass
                };
                var link = new LinkButton { Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(yearMonth.Month) + " " + yearMonth.Year.ToString() };
                link.Click += link_Click;
                link.CommandArgument = yearMonth.Year.ToString() + yearMonth.Month.ToString();
                link.CssClass = "monthsMainItem";
                month.Controls.Add(link);
                monthsMain.Controls.Add(month);
            }

            var month0 = new Panel
            {
                CssClass = defaultCssClass
            };

            var lnkNewMonthOprning = new LinkButton { Text = "Deschide o nouă lună" };
            lnkNewMonthOprning.Click += lnkNewMonthOprning_Click;
            lnkNewMonthOprning.CommandArgument = "-1";
            lnkNewMonthOprning.CssClass = "monthsMainItem fa fa-plus";

            month0.Controls.Add(lnkNewMonthOprning);
            monthsMain.Controls.Add(month0);
        }

        private void link_Click(object sender, EventArgs e)
        {
            var lb = (LinkButton)sender;
            var year = lb.CommandArgument.Substring(0, 4);
            var month = (lb.CommandArgument.Count() == 5) ? lb.CommandArgument.Substring(4, 1) : lb.CommandArgument.Substring(4, 2);

            Response.Redirect("~/Expenses/Invoices.aspx?year=" + year + "&month=" + month);
        }

        public void lnkNewMonthOprning_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Config/NewMonthOpening.aspx");
        }
    }
}