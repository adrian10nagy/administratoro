
namespace Admin.Invoices
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Managers;
    using Administratoro.DAL;
    using System;
    using System.Web.UI.WebControls;

    public partial class List : BasePage
    {
        private int month()
        {
            var monthId = Request.QueryString["month"];
            int month;
            if (!int.TryParse(monthId, out month) || month > 13 || month < 0)
            {
                Response.Redirect("..\\Expenses\\Dashboard.aspx", true);
            }

            return month;
        }

        private int year()
        {
            var yearId = Request.QueryString["year"];
            int year;
            if (!int.TryParse(yearId, out year) || year < 0)
            {
                Response.Redirect("..\\Expenses\\Dashboard.aspx", true);
            }

            return year;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitializeInvoices();
        }

        private void InitializeInvoices()
        {
            var ee = AssociationExpensesManager.GetAllAssociationExpensesByMonthAndYearwithDiverse(Association.Id, year(), month());

            Panel pnHeader = new Panel();
            pnHeader.CssClass = "col-md-12 invoicesListHeader";

            Label lbHeaderName = new Label
            {
                Text = "Cheltuială",
                CssClass = "col-md-6"
            };
            pnHeader.Controls.Add(lbHeaderName);

            Label lbHeaderValue = new Label
            {
                Text = "Valoare",
                CssClass = "col-md-6"
            };
            pnHeader.Controls.Add(lbHeaderValue);
            invoiceMain.Controls.Add(pnHeader);

            foreach (var associationExpense in ee)
            {
                if (associationExpense.Invoices.Count != 0)
                {
                    foreach (var invoice in associationExpense.Invoices)
                    {
                        Panel pn = new Panel();
                        pn.CssClass = "col-md-12";

                        Label lbName = new Label();
                        lbName.Text = invoice.Description;
                        lbName.CssClass = "col-md-6";
                        pn.Controls.Add(lbName);

                        Label lbValue = new Label();
                        lbValue.Text = invoice.Value.HasValue ? invoice.Value.Value.ToString() : string.Empty;
                        lbValue.CssClass = "col-md-6";
                        pn.Controls.Add(lbValue);

                        invoiceMain.Controls.Add(pn);
                    }
                }
                else
                {
                    Panel pn = new Panel();
                    pn.CssClass = "col-md-12";

                    Label lbName = new Label();
                    lbName.Text = associationExpense.Expenses.Name;
                    lbName.CssClass = "col-md-6";
                    pn.Controls.Add(lbName);

                    Label lbValue = new Label();
                    lbValue.Text = string.Empty;
                    lbValue.CssClass = "col-md-6";
                    pn.Controls.Add(lbValue);

                    invoiceMain.Controls.Add(pn);
                }
            }

        }
    }
}