
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
            mainHeader.InnerText = "Facturi pentru luna " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month());
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
            AddHeaderPanels();
            AddBodyPanels(estateId, yearNr, monthNr);
        }

        private void AddBodyPanels(int estateId, int yearNr, int monthNr)
        {
            string mainRowCssFormat = "col-md-12 col-sm-12 xs-12 cashBokItemsRow {0}";
            bool even = false;

            var allExpenses = ExpensesManager.GetAllExpensesAsList();
            var estateExpenses = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearNotDisabled(estateId, yearNr, monthNr);


            // add expense panels
            foreach (Expenses expense in allExpenses)
            {
                var estateExpense = estateExpenses.FirstOrDefault(ea => ea.Id_Expense == expense.Id);
                if (estateExpense != null)
                {
                    var mainCol = new Panel();

                    if (even)
                    {
                        even = false;
                        mainCol.CssClass = string.Format(mainRowCssFormat, "evenRow");
                    }
                    else
                    {
                        even = true;
                        mainCol.CssClass = string.Format(mainRowCssFormat, "oddRow");
                    }

                    bool isExpensePerIndex = estateExpense.ExpenseTypes.Id == (int)ExpenseType.PerIndex;
                    var cashBook = CashBookManager.GetByEstateExpenseId(estateExpense.Id);
                    if (cashBook == null)
                    {
                        CashBookManager.AddDefault(estateExpense.Id);
                        cashBook = CashBookManager.GetByEstateExpenseId(estateExpense.Id);
                    }

                    var col0 = AddBodyPanelCol0(estateExpense, isExpensePerIndex, cashBook);
                    var col1 = AddBodyPanelCol1(expense, estateExpense);

                    TextBox tb1;
                    var col2 = AddBodyPanelCol2(estateExpense, cashBook, out tb1);

                    TextBox tb3;
                    var col3 = AddBodyPanelCol3(estateExpense, cashBook, tb1, out tb3);

                    var col6 = AddBodyPanelCol6(estateExpense, cashBook, tb3);
                    var col4 = AddBodyPanelCol4(estateExpense, tb1);
                    var col5 = AddBodyPanelCol5(estateExpense, isExpensePerIndex);

                    mainCol.Controls.Add(col0);
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

        private Panel AddBodyPanelCol5(EstateExpenses estateExpense, bool isExpensePerIndex)
        {
            // column 5
            var col5 = new Panel
            {
                CssClass = "col-md-3 col-sm-2 col-xs-6"
            };
            Button btnAddExpense = new Button
            {
                PostBackUrl = "AddEditExpense.aspx?id_exes=" + estateExpense.Id,
                Text = "Adaugă/Modifică",
                Visible = isExpensePerIndex
            };

            if (isExpensePerIndex && (decimal)estateExpense.TenantExpenses.Count() > 0)
            {
                var message = ExpensePercentageFilledInMessage(estateExpense);
                var col5Literal = new Literal { Text = message };
                col5.Controls.Add(col5Literal);
            }
            col5.Controls.Add(btnAddExpense);
            return col5;
        }

        private Panel AddBodyPanelCol4(EstateExpenses estateExpense, TextBox tb1)
        {
            // column 4
            var col4 = new Panel
            {
                CssClass = "col-md-2 col-sm-2 col-xs-6"
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

            return col4;
        }

        private Panel AddBodyPanelCol6(EstateExpenses estateExpense, CashBooks cashBook, TextBox tb3)
        {
            // column 6
            var col6 = new Panel
            {
                CssClass = "col-md-2 col-sm-2 col-xs-6"
            };
            Button btnRedistibuteRemainingExpense = new Button
            {
                Visible = (tb3.Text == string.Empty) ? false : true,
                Text = (!cashBook.RedistributeType.HasValue) ? "Redistribuie" : "Redistibuit " + cashBook.EstateExpensesRedistributionTypes.Value + ", MODIFICĂ",
                CommandArgument = estateExpense.Id.ToString()
            };
            btnRedistibuteRemainingExpense.Click += btnRedistibuteRemainingExpense_Click;
            col6.Controls.Add(btnRedistibuteRemainingExpense);
            return col6;
        }

        private Panel AddBodyPanelCol3(EstateExpenses estateExpense, CashBooks cashBook, TextBox tb1, out TextBox tb3)
        {
            // column 3
            var col3 = new Panel
            {
                CssClass = "col-md-1 col-sm-4 col-xs-6"
            };
            tb3 = new TextBox { Enabled = false, CssClass = "cashbookItemTextbox" };
            // tb3.Attributes.Add("eeId", estateExpense.Id.ToString());

            if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerIndex)
            {
                tb3.Text = RedistributeValuePerIndex(estateExpense, cashBook);
                if (!string.IsNullOrEmpty(tb3.Text))
                {
                    var col3Literal = new Literal { Text = "<b>" + tb1.Text + " - " + estateExpense.TenantExpenses.Sum(ee => ee.Value) + "</b>" };
                    col3.Controls.Add(col3Literal);
                }
                col3.Controls.Add(tb3);
            }
            else if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
            {
                if (!string.IsNullOrEmpty(tb1.Text))
                {
                    var col3Literal = new Literal { Text = "0" };
                    col3.Controls.Add(col3Literal);
                }
            }
            else if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerTenants && estateExpense.TenantExpenses.Count > 0)
            {
                var col3Literal = new Literal { Text = estate().Tenants.Sum(s => s.Dependents) + " locatari, <b>" + EstateExpensesManager.CalculatePertenantPrice(estateExpense) + "</b> alocat fiecăruia " };
                col3.Controls.Add(col3Literal);
            }

            return col3;
        }

        private Panel AddBodyPanelCol2(EstateExpenses estateExpense, CashBooks cashBook, out TextBox tb1)
        {
            // column 2
            var col2 = new Panel
            {
                CssClass = "col-md-2 col-sm-4 col-xs-6"
            };

            tb1 = new TextBox { Enabled = false, CssClass = "cashbookItemTextbox" };

            if (estate().HasStaircase && estateExpense.SplitPerStairCase.HasValue && estateExpense.SplitPerStairCase.Value)
            {
                foreach (StairCases stairCase in estate().StairCases)
                {
                    var invoice = stairCase.Invoices.FirstOrDefault(i => i.Id_StairCase == stairCase.Id && i.Id_CashBook == cashBook.Id);
                    var lit = new Literal
                    {
                        Text = "Scara: " + stairCase.Nume + "  "
                    };

                    var tb = new TextBox
                    {
                        Enabled = false,
                        CssClass = "cashbookItemTextbox",
                        Text = ((invoice != null) ? invoice.Value.ToString() : string.Empty)
                    };
                    tb.Attributes.Add("eeId", estateExpense.Id.ToString());
                    tb.Attributes.Add("sc", stairCase.Id.ToString());
                    col2.Controls.Add(lit);
                    col2.Controls.Add(tb);
                    col2.Controls.Add(new Literal { Text = "<br>" });
                }
            }
            else
            {
                tb1.Attributes.Add("eeId", estateExpense.Id.ToString());
                tb1.Text = (cashBook != null) ? cashBook.Value.ToString() : string.Empty;
                col2.Controls.Add(tb1);
            }

            return col2;
        }

        private Panel AddBodyPanelCol1(Expenses expense, EstateExpenses estateExpense)
        {
            // column 1
            var col1 = new Panel
            {
                CssClass = "col-md-1 col-sm-2 col-xs-6",
            };
            Literal literal1 = new Literal
            {
                Text = "<b>" + expense.Name + "</b> (" + estateExpense.ExpenseTypes.Name + ")"
            };
            col1.Controls.Add(literal1);
            return col1;
        }

        private Panel AddBodyPanelCol0(EstateExpenses estateExpense, bool isExpensePerIndex, CashBooks cashBook)
        {
            // column 0
            var col0 = new Panel();
            Literal literal0 = new Literal
            {
                Text = statusOfCashBook(cashBook, estateExpense, isExpensePerIndex)               
            };
            col0.Controls.Add(literal0);
            col0.CssClass = "col-md-1 col-sm-2 col-xs-6";
            return col0;
        }

        private void AddHeaderPanels()
        {
            // add header panels
            var headerCol0 = new Panel { CssClass = "col-md-1 col-sm-1 col-xs-6" };
            var headerCol0Literal0 = new Literal { Text = "Status" };
            headerCol0.Controls.Add(headerCol0Literal0);

            var headerCol1 = new Panel { CssClass = "col-md-1 col-sm-1 col-xs-6" };
            var headerCol1Literal1 = new Literal { Text = "Cheltuială" };
            headerCol1.Controls.Add(headerCol1Literal1);

            var headerCol2 = new Panel { CssClass = "col-md-2 col-sm-1 col-xs-6" };
            var headerCol2Literal2 = new Literal { Text = "Valoare factură" };
            headerCol2.Controls.Add(headerCol2Literal2);

            var headerCol3 = new Panel { CssClass = "col-md-1 col-sm-1 col-xs-6" };
            var headerCol3Literal3 = new Literal { Text = "Valoare de redistribuit" };
            headerCol3.Controls.Add(headerCol3Literal3);

            var headerCol4 = new Panel { CssClass = "col-md-2 col-sm-3 col-xs-6" };

            var headerCol5 = new Panel { CssClass = "col-md-2 col-sm-2 col-xs-6" };
            var headerCol5Literal = new Literal { Text = "Cheltuieli" };
            headerCol5.Controls.Add(headerCol5Literal);
            var headerCol6 = new Panel { CssClass = "col-md-3 col-sm-3 col-xs-6" };

            var headerPanel = new Panel() { CssClass = "headerRow row" };
            headerPanel.Controls.Add(headerCol0);
            headerPanel.Controls.Add(headerCol1);
            headerPanel.Controls.Add(headerCol2);
            headerPanel.Controls.Add(headerCol3);
            headerPanel.Controls.Add(headerCol5);
            headerPanel.Controls.Add(headerCol6);
            cashBookMain.Controls.Add(headerPanel);
        }

        private static string ExpensePercentageFilledIn(EstateExpenses estateExpense)
        {
            var addedExpenses = estateExpense.TenantExpenses.Count(te => te.IndexNew.HasValue);
            var percentage = (((decimal)addedExpenses / (decimal)estateExpense.TenantExpenses.Count()) * 100).ToString("0.##");

            return percentage;
        }

        private static string ExpensePercentageFilledInMessage(EstateExpenses estateExpense)
        {
            var addedExpenses = estateExpense.TenantExpenses.Count(te => te.IndexNew.HasValue);
            var allExpenses = estateExpense.TenantExpenses.Count();

            return "<b>"+addedExpenses + "</b> cheltuieli adăugate din <b>" + allExpenses + "</b> ";
        }

        private string statusOfCashBook(CashBooks cashBook, EstateExpenses estateExpense, bool isExpensePerIndex)
        {
            string result = string.Empty;
            var redistributeValue = CalculateRedistributeValue(estateExpense.Id, cashBook);
            var percentage = string.Empty;

            if (isExpensePerIndex && (decimal)estateExpense.TenantExpenses.Count() > 0)
            {
                percentage = ExpensePercentageFilledIn(estateExpense);
            }

            if (cashBook.Value != null && string.IsNullOrEmpty(percentage) && string.IsNullOrEmpty(redistributeValue))
            {
                result = "<i class='fa fa-check'></i> 100%";
            }
            else if (string.IsNullOrEmpty(redistributeValue) && cashBook.Value == null && percentage == "0")
            {
                result = "Adaugă factura, cheltuielile! 0%";
            }
            else if (!string.IsNullOrEmpty(redistributeValue))
            {
                result = "Redistribuie cheltuiala! 80%";
            }
            else if (cashBook.Value == null)
            {
                result = "Lipsă factură! 60%";
            }
            else if (!string.IsNullOrEmpty(percentage))
            {
                result = "Cheltuieli neadăugate! 20%";
            }

            return result;
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
                var estateExpense = EstateExpensesManager.GetById(estateExpenseId);

                if (estateExpense == null)
                {
                    return;
                }

                foreach (var control in cashBookMain.Controls)
                {
                    if (found)
                    {
                        // break;
                    }

                    if (control != null && control is Panel)
                    {
                        Panel panel = (Panel)control;
                        foreach (var innerControl in panel.Controls)
                        {
                            if (found)
                            {
                                //break;
                            }

                            if (control != null && innerControl is Panel)
                            {
                                Panel innerPanel = (Panel)innerControl;
                                foreach (var controls3 in innerPanel.Controls)
                                {

                                    if (controls3 != null && controls3 is TextBox)
                                    {
                                        TextBox tb2 = (TextBox)controls3;
                                        int tbxEstateExpenseId;
                                        if (int.TryParse(tb2.Attributes["eeId"], out tbxEstateExpenseId)
                                            && tbxEstateExpenseId == estateExpenseId)
                                        {

                                            if (tb2.Enabled)
                                            {
                                                decimal? updatedValue = null;
                                                decimal eeValue;
                                                if (decimal.TryParse(tb2.Text, out eeValue) || tb2.Text == string.Empty)
                                                {
                                                    if (tb2.Text != string.Empty)
                                                    {
                                                        updatedValue = eeValue;
                                                    }

                                                    tb2.Attributes.Add("border-color", "");
                                                    // if no staircase update only default value
                                                    if (!(estateExpense.Estates.HasStaircase && estateExpense.SplitPerStairCase.HasValue && estateExpense.SplitPerStairCase.Value))
                                                    {
                                                        CashBookManager.Update(estateExpense, updatedValue);
                                                        Response.Redirect(Request.RawUrl);
                                                    }
                                                    else
                                                    {
                                                        int stairCaseId;
                                                        if (int.TryParse(tb2.Attributes["sc"], out stairCaseId)
                                                         && tbxEstateExpenseId == estateExpenseId)
                                                        {
                                                            int cashBookId;
                                                            if (int.TryParse(tb2.Attributes["cb"], out cashBookId)
                                                             && tbxEstateExpenseId == estateExpenseId)
                                                            {

                                                            }

                                                            CashBookManager.Update(estateExpense, updatedValue,stairCaseId);

                                                        }
                                                    }

                                                    tb2.Enabled = false;
                                                }
                                                else
                                                {
                                                    tb2.Attributes.Add("style", "border-color: red");
                                                }

                                            }
                                            else
                                            {
                                                tb2.Enabled = true;
                                            }

                                            found = true;
                                            // break;
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

                var tenants = ApartmentsManager.GetAllByEstateId(estate.Id);
                var allTenantDependents = tenants.Sum(t => t.Dependents);
                var valuePerTenant = "0";
                if (allTenantDependents != 0)
                {
                    valuePerTenant = Math.Round(redistributeVal / allTenantDependents, 2).ToString();
                }

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
            if (int.TryParse(btn.CommandArgument, out estateExpenseId))
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