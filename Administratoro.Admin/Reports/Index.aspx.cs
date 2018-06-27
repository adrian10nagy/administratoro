
namespace Admin.Reports
{
    using Administratoro.BL.Models;
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
            var estate = (Associations)Session[SessionConstants.SelectedAssociation];
            var defaultCssClass = "col-md-2 col-sm-3 col-xs-12";
            List<YearMonth> yearMonths = AssociationExpensesManager.GetAllMonthsAndYearsAvailableByAssociationId(estate.Id);
            if (yearMonths.Count != 0)
            {
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

                monthsMain.Controls.Add(month0);
            }
            else
            {
                var lblMessage = new Label
                {
                    Text = @"<h4>Nici o lună deschisă.</h4> <br> Pentru a vedea rapoarte, mai întâi trebuie să ai cel puțin o lună deschisă. 
                            <a href='/Expenses/Dashboard.aspx'>Click aici pentru a începe<a/>"
                };
                monthsMain.Controls.Add(lblMessage);
            }
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