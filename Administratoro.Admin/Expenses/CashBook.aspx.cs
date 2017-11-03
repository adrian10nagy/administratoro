
namespace Admin.Expenses
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Extensions;
    using Administratoro.BL.Managers;
    using Administratoro.DAL;
    using System;
    using System.Web.UI.WebControls;
    using System.Linq;
    using System.Globalization;
    using System.Web.UI;

    public partial class CashBook : System.Web.UI.Page
    {
        public int Month { get { return month(); } }
        public int Estate { get { return estateId(); } }
        public int Year { get { return year(); } }

        private int estateId()
        {
            var estate = (Estates)Session[SessionConstants.SelectedEstate];

            if (estate == null)
            {
                Response.Redirect("..\\Expenses\\Dashboard.aspx", true);
            }

            return estate.Id;
        }

        private Estates estate()
        {
            var estate = (Estates)Session[SessionConstants.SelectedEstate];

            if (estate == null)
            {
                Response.Redirect("..\\Expenses\\Dashboard.aspx", true);
            }

            return estate;
        }

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

        protected void Page_Init(object sender, EventArgs e)
        {
            mainHeader.InnerText = "Registru de casă pentru luna " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month());
            InitializeCashBook();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        private void InitializeCashBook(int? exesId = null)
        {
            if (month() != 0 && year() != 0 && estateId() != 0)
            {
                InitializeCashBook(estateId(), year(), month(), exesId);
            }
            else
            {
                Response.Redirect("~/");
            }
        }

        private void InitializeCashBook(int estateId, int yearNr, int monthNr, int? exesId)
        {
            string mainRowCssFormat = "col-md-12 col-sm-12 xs-12 cashBokItemsRow {0}";
            bool even = false;

            var allExpenses = ExpensesManager.GetAllExpensesAsList();
            var estateExpenses = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearNotDisabled(estateId, yearNr, monthNr);

            // add header panels
            var headerCol1 = new Panel { CssClass = "col-md-1 col-sm-2 col-xs-6" };
            var headerCol1Literal1 = new Literal { Text = "Cheltuială" };
            headerCol1.Controls.Add(headerCol1Literal1);

            var headerCol2 = new Panel { CssClass = "col-md-1 col-sm-3 col-xs-6" };
            var headerCol2Literal2 = new Literal { Text = "Valoare factură" };
            headerCol2.Controls.Add(headerCol2Literal2);

            var headerCol3 = new Panel { CssClass = "col-md-1 col-sm-3 col-xs-6" };
            var headerCol3Literal3 = new Literal { Text = "Valoare de redistribuit" };
            headerCol3.Controls.Add(headerCol3Literal3);

            var headerCol4 = new Panel { CssClass = "col-md-2 col-sm-2 col-xs-6" };
            var headerCol5 = new Panel { CssClass = "col-md-2 col-sm-2 col-xs-6" };
            var headerCol6 = new Panel { CssClass = "col-md-2 col-sm-2 col-xs-6" };

            var headerPanel = new Panel() { CssClass="headerRow row"};
            headerPanel.Controls.Add(headerCol1);
            headerPanel.Controls.Add(headerCol2);
            headerPanel.Controls.Add(headerCol3);
            headerPanel.Controls.Add(headerCol4);
            headerPanel.Controls.Add(headerCol5);
            headerPanel.Controls.Add(headerCol6);
            cashBookMain.Controls.Add(headerPanel);

            // add expense panels
            foreach (Expenses expense in allExpenses)
            {
                var estateExpense = estateExpenses.FirstOrDefault(ea => ea.Id_Expense == expense.Id);
                if (estateExpense != null)
                {
                    var mainCol = new Panel();

                    if(even)
                    {
                        even = false;
                        mainCol.CssClass = string.Format(mainRowCssFormat,"evenRow");
                    }
                    else
                    {
                        even = true;
                        mainCol.CssClass = string.Format(mainRowCssFormat,"oddRow");
                    }

                    // column 1
                    var col1 = new Panel
                    {
                        CssClass = "col-md-1 col-sm-2 col-xs-6",
                    };
                    Literal literal1 = new Literal
                    {
                        Text = expense.Name
                    };
                    col1.Controls.Add(literal1);

                    // column 2
                    var col2 = new Panel
                    {
                        CssClass = "col-md-1 col-sm-4 col-xs-6"
                    };

                    TextBox tb1 = new TextBox { Enabled = false, CssClass = "cashbookItemTextbox" };
                    var cashBook = CashBookManager.GetByEstateExpenseId(estateExpense.Id);
                    if (cashBook == null)
                    {
                        CashBookManager.AddDefault(estateExpense.Id);
                        cashBook = CashBookManager.GetByEstateExpenseId(estateExpense.Id);
                    }

                    tb1.Attributes.Add("eeId", estateExpense.Id.ToString());
                    tb1.Text = (cashBook != null) ? cashBook.Value.ToString() : string.Empty;
                    col2.Controls.Add(tb1);

                    // column 3
                    var col3 = new Panel
                    {
                        CssClass = "col-md-1 col-sm-4 col-xs-6"
                    };
                    TextBox tb3 = new TextBox { Enabled = false, CssClass = "cashbookItemTextbox" };
                    tb3.Attributes.Add("eeId", estateExpense.Id.ToString());
                    if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerIndex)
                    {
                        tb3.Text = RedistributeValuePerIndex(estateExpense, cashBook);
                        col3.Controls.Add(tb3);
                    }
                    else if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
                    {
                        tb3.Text = RedistributeValueCotaIndiviza(estate(), estateExpense, cashBook);
                        col3.Controls.Add(tb3);
                    }
                    else if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerTenants)
                    {
                        // for Per Tenants do not add the textbox
                    }

                    // column 6
                    var col6 = new Panel
                    {
                        CssClass = "col-md-3 col-sm-2 col-xs-6"
                    };
                    Button btnRedistibuteRemainingExpense = new Button
                    {
                        Visible = (tb3.Text==string.Empty)?false:true,
                        Text = (cashBook.RedistributeType==null) ? "Redistribuie" : cashBook.EstateExpensesRedistributionTypes.Value + ", MODIFICĂ",
                        CommandArgument = estateExpense.Id.ToString()
                    };
                    btnRedistibuteRemainingExpense.Click += btnRedistibuteRemainingExpense_Click;
                    col6.Controls.Add(btnRedistibuteRemainingExpense);


                    // column 4
                    var col4 = new Panel
                    {
                        CssClass = "col-md-3 col-sm-2 col-xs-6"
                    };
                    Button btn4 = new Button();
                    if (tb1.Text == string.Empty)
                    {
                        btn4.Text = "Adaugă factura";
                        btn4.CssClass = "btn btn-warning";
                    }
                    else
                    {
                        btn4.Text = "Modifică factura";
                        btn4.CssClass = "btn btn-success";
                    }

                    btn4.Click += ClickablePanel1_Click;
                    btn4.CommandArgument = estateExpense.Id.ToString();
                    col4.Controls.Add(btn4);

                    // column 5
                    var col5 = new Panel
                    {
                        CssClass = "col-md-3 col-sm-2 col-xs-6"
                    };
                    Button btnAddExpense = new Button
                    {
                        PostBackUrl = "AddEditExpense.aspx?id_exes=" + estateExpense.Id,
                        Text = "Adaugă cheltuieli"
                    };
                    col5.Controls.Add(btnAddExpense);

                    mainCol.Controls.Add(col1);
                    mainCol.Controls.Add(col2);
                    mainCol.Controls.Add(col3);
                    mainCol.Controls.Add(col6);
                    mainCol.Controls.Add(col4);
                    mainCol.Controls.Add(col5);

                    cashBookMain.Controls.Add(mainCol);
                }
            }
        }

        private static string RedistributeValuePerIndex(EstateExpenses estateExpense, CashBooks cashBook1)
        {
            decimal? sumOfIndexes = TenantExpensesManager.GetSumOfIndexesForexpense(estateExpense.Id);
            return (sumOfIndexes != null & cashBook1 != null && estateExpense.PricePerExpenseUnit != null)
                ? (cashBook1.Value - (estateExpense.PricePerExpenseUnit * sumOfIndexes.Value)).ToString()
                : string.Empty;
        }

        private static string RedistributeValueCotaIndiviza(Estates estate, EstateExpenses estateExpense, CashBooks cashBook)
        {
            decimal? sumOfIndiviza = TenantExpensesManager.GetSumOfIndivizaForExpense(estateExpense);
            return (sumOfIndiviza != null & estate != null)
                ? ((sumOfIndiviza.Value / estate.Indiviza) * cashBook.Value).ToString()
                : string.Empty;
        }

        public void ClickablePanel1_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int estateExpenseId;
            bool found = false;

            if (int.TryParse(btn.CommandArgument, out estateExpenseId))
            {
                foreach (var controls1 in cashBookMain.Controls)
                {
                    if (found)
                    {
                        break;
                    }

                    if (controls1 != null && controls1 is Panel)
                    {
                        Panel panel1 = (Panel)controls1;
                        foreach (var controls2 in panel1.Controls)
                        {
                            if (found)
                            {
                                break;
                            }

                            if (controls2 != null && controls2 is Panel)
                            {
                                Panel panel2 = (Panel)controls2;
                                foreach (var controls3 in panel2.Controls)
                                {
                                    if (controls3 != null && controls3 is TextBox)
                                    {
                                        TextBox tb2 = (TextBox)controls3;
                                        int tbxEstateExpenseId;
                                        if (int.TryParse(tb2.Attributes["eeId"], out tbxEstateExpenseId)
                                            && tbxEstateExpenseId == estateExpenseId)
                                        {
                                            var estateExpense = EstateExpensesManager.GetById(estateExpenseId);

                                            if (estateExpense != null)
                                            {
                                                if (tb2.Enabled)
                                                {
                                                    decimal eeValue;
                                                    if (decimal.TryParse(tb2.Text, out eeValue))
                                                    {
                                                        tb2.Attributes.Add("color", "");
                                                        CashBookManager.Update(estateExpense, eeValue);
                                                        Response.Redirect(Request.RawUrl);
                                                    }
                                                    else
                                                    {
                                                        tb2.Attributes.Add("color", "red");
                                                    }

                                                    tb2.Enabled = false;
                                                }
                                                else
                                                {
                                                    tb2.Enabled = true;
                                                }

                                                found = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void lblExpenseMeessageConfigure_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Config/MonthlyExpenses.aspx");
        }

        private void btnRedistibuteRemainingExpense_Click(object sender, EventArgs e)
        {
            Button estateExpense = (Button)sender;
            int estateExpenseId;
            if (int.TryParse(estateExpense.CommandArgument, out estateExpenseId))
            {
                var cashBook = CashBookManager.GetByEstateExpenseId(estateExpenseId);
                if (cashBook != null)
                {
                    ConfigureRedistributeMessages(estateExpenseId, cashBook);
                }
            }

            cashBookRedistribute.Visible = true;
            cashBookMain.Visible = false;
            lblExpenseMeessageConfigure.Visible = false;
        }

        private void ConfigureRedistributeMessages(int estateExpenseId, CashBooks cashBook)
        {
            string redistributeValue = CalculateRedistributeValue(estateExpenseId, cashBook);
            cashBookRedistributeMessage.Text = "<br>Cheltuială de redistribuit:" + redistributeValue + "<br>";

            decimal redistributeVal;
            if (decimal.TryParse(redistributeValue, out redistributeVal))
            {
                Estates estate = EstatesManager.GetByEstateExpenseId(estateExpenseId);
                txtCashBookRedistributeEqualApartment.Text = estate.Tenants.Count + "apartamente, alocă <b>" +
                    Math.Round(redistributeVal / estate.Tenants.Count, 2) + "</b> la fiecare apartament";

                var tenants = TenantsManager.GetAllByEstateId(estate.Id);
                var allTenantDependents = tenants.Select(t => t.Dependents).Sum();
                var valuePerTenant = Math.Round(redistributeVal / allTenantDependents, 2).ToString();
                txtCashBookRedistributeEqualTenants.Text = allTenantDependents + " locatari în bloc, alocă <b>" + valuePerTenant + "</b> per fiecare locatar";

                txtCashBookRedistributeConsumption.Text = "Not implemented";

                cashBookRedistributeEqualApartment.CommandArgument = estateExpenseId.ToString();
                cashBookRedistributeEqualTenants.CommandArgument = estateExpenseId.ToString();
                cashBookRedistributeConsumption.CommandArgument = estateExpenseId.ToString();
            }
        }

        private string CalculateRedistributeValue(int estateExpenseId, CashBooks cashBook)
        {
            EstateExpenses estateExpense = EstateExpensesManager.GetById(estateExpenseId);

            if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerIndex)
            {
                return RedistributeValuePerIndex(estateExpense, cashBook);
            }
            else if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
            {
                var estate = EstatesManager.GetById(estateExpenseId);
                return RedistributeValueCotaIndiviza(estate, estateExpense, cashBook);
            }

            return string.Empty;
        }

        private void lnkCashBookModify_Click(object sender, EventArgs e)
        {
            LinkButton lnk = sender as LinkButton;
            int? estateId = lnk.ID.Replace("lnkBtn", "").ToNullableInt();
            if (estateId.HasValue)
            {
                CashBookManager.AddDefault(estateId.Value);
                InitializeCashBook(estateId);
            }
        }

        protected void cashBookRedistributeConsumption_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int estateExpenseId;
            if(int.TryParse(btn.CommandArgument, out estateExpenseId))
            {
                CashBookManager.UpdateRedistributeMethodAndValue(estateExpenseId, 3);
            }
        }

        protected void cashBookRedistributeEqualTenants_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int estateExpenseId;
            if (int.TryParse(btn.CommandArgument, out estateExpenseId))
            {
                CashBookManager.UpdateRedistributeMethodAndValue(estateExpenseId, 2);
                Response.Redirect(Request.RawUrl);
            }
        }

        protected void cashBookRedistributeEqualApartment_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int estateExpenseId;
            if (int.TryParse(btn.CommandArgument, out estateExpenseId))
            {
                CashBookManager.UpdateRedistributeMethodAndValue(estateExpenseId, 1);
                Response.Redirect(Request.RawUrl);
            }
        }

        protected void cashBookRedistributeCancel_Click(object sender, EventArgs e)
        {
            cashBookRedistribute.Visible = false;
            cashBookMain.Visible = true;
            lblExpenseMeessageConfigure.Visible = true;
        }
    }
}