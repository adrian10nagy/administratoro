
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
    using System.Collections.Generic;
    using System.Text;

    public partial class Invoice : BasePage
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

        protected void Page_Init(object sender, EventArgs e)
        {
            mainHeader.InnerText = "Facturi pentru luna " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month()) + " " + year();
            InitializeInvoice();
        }

        private void InitializeInvoice(int? exesId = null)
        {
            if (month() != 0 && year() != 0 && Association.Id != 0)
            {
                InitializeInvoice(Association.Id, year(), month(), exesId);
            }
            else
            {
                Response.Redirect("~/");
            }
        }

        private void InitializeInvoice(int associationId, int yearNr, int monthNr, int? exesId)
        {
            bool isMonthClosed = EstateExpensesManager.IsMonthClosed(Association.Id, yearNr, monthNr);
            if (!isMonthClosed)
            {
                AddHeaderPanels();
                AddBodyPanels(associationId, yearNr, monthNr);
            }
            else
            {
                Label lbMessage = new Label();
                lbMessage.Attributes.CssStyle.Add("color", "red");
                lbMessage.Text = "Luna este închisă, nu se mai pot face modificări. Pentru a redeschide luna contactează administratorul asociației";
                pnlMessage.Controls.Add(lbMessage);
                pnlMessage.Visible = true;
                btnCloseMonth.Visible = false;
                lblExpenseMeessageConfigure.Visible = false;
            }
        }

        private void AddBodyPanels(int associationId, int yearNr, int monthNr)
        {
            string mainRowCssFormat = "col-md-12 col-sm-12 xs-12 cashBokItemsRow {0}";
            bool even = false;

            var allExpenses = ExpensesManager.GetAllExpenses();
            var estateExpenses = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearNotDisabled(associationId, yearNr, monthNr).OrderBy(ee => ee.Id_ExpenseType);

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

                    var col0 = AddBodyPanelCol0(estateExpense, isExpensePerIndex);
                    var col1 = AddBodyPanelCol1(expense, estateExpense);

                    TextBox tb2;
                    var col2 = AddBodyPanelCol2(estateExpense, out tb2);

                    TextBox tb3;
                    var col3 = AddBodyPanelCol3(estateExpense, tb2, out tb3);

                    var col6 = AddBodyPanelCol6(estateExpense, tb3);
                    var col4 = AddBodyPanelCol4(estateExpense, tb2);
                    var col5 = AddBodyPanelCol5(estateExpense, isExpensePerIndex);

                    mainCol.Controls.Add(col0);
                    mainCol.Controls.Add(col1);
                    mainCol.Controls.Add(col2);
                    mainCol.Controls.Add(col3);
                    mainCol.Controls.Add(col6);
                    mainCol.Controls.Add(col4);
                    mainCol.Controls.Add(col5);

                    invoiceMain.Controls.Add(mainCol);
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

            if (estateExpense.Id_Expense != (int)Expense.AjutorÎncălzire)
            {
                if (isExpensePerIndex && estateExpense.Estates.Tenants.Count() > 0)
                {
                    var message = EstateExpensesManager.ExpensePercentageFilledInMessage(estateExpense);
                    var col5Literal = new Literal { Text = message };
                    col5.Controls.Add(col5Literal);
                    if (!message.Contains("<b>0</b> cheltuieli adăugate din <b>0</b"))
                    {
                        Button btnAddExpense = new Button
                        {
                            PostBackUrl = "AddEditExpense.aspx?id_exes=" + estateExpense.Id,
                            Text = "Adaugă/Modifică",
                            Visible = isExpensePerIndex
                        };
                        col5.Controls.Add(btnAddExpense);
                    }
                }
            }
            else
            {
                Button btnAddHeatHelp = new Button
                {
                    PostBackUrl = "AddEditHeatHelp.aspx?id_exes=" + estateExpense.Id,
                    Text = "Adaugă/Modifică"
                };

                col5.Controls.Add(btnAddHeatHelp);
            }

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
            btn4.Visible = estateExpense.Id_Expense != (int)Expense.AjutorÎncălzire;
            col4.Controls.Add(btn4);

            return col4;
        }

        private Panel AddBodyPanelCol6(EstateExpenses estateExpense, TextBox tb3)
        {
            // column 6
            var col6 = new Panel
            {
                CssClass = "col-md-2 col-sm-2 col-xs-6"
            };

            if (estateExpense.ExpenseTypes.Id == (int)ExpenseType.PerIndex)
            {
                string percentage = EstateExpensesManager.GetPercentageAsString(estateExpense);

                Button btnRedistibuteRemainingExpense = new Button
                {
                    CssClass = "btnRedistibuteRemainingExpense",
                    Visible = (percentage == "100" || percentage == string.Empty) && !string.IsNullOrEmpty(tb3.Text) ? true : false,
                    Text = (!estateExpense.RedistributeType.HasValue) ? "Redistribuie" : "Redistibuit " + estateExpense.EstateExpensesRedistributionTypes.Value + ", MODIFICĂ",
                    CommandArgument = estateExpense.Id.ToString()
                };
                btnRedistibuteRemainingExpense.Click += btnRedistibuteRemainingExpense_Click;
                col6.Controls.Add(btnRedistibuteRemainingExpense);
            }
            return col6;
        }

        private Panel AddBodyPanelCol3(EstateExpenses estateExpense, TextBox tb1, out TextBox tb3)
        {
            // column 3
            var col3 = new Panel
            {
                CssClass = "col-md-1 col-sm-4 col-xs-6"
            };
            tb3 = new TextBox { Enabled = false, CssClass = "invoiceItemTextbox" };

            if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerIndex)
            {
                if (!estateExpense.RedistributeType.HasValue)
                {
                    var percentage = EstateExpensesManager.GetPercentageAsString(estateExpense);
                    if (percentage == "100")
                    {
                        tb3.Text = RedistributionManager.RedistributeValuePerIndexAsString(estateExpense);
                    }

                    if (tb3.Text == "0,0000000")
                    {
                        tb3.Text = string.Empty;
                    }

                    if (!string.IsNullOrEmpty(tb3.Text))
                    {
                        var col3Literal = new Literal { Text = "<b>" + tb1.Text + " - " + estateExpense.TenantExpenses.Sum(ee => ee.Value) + "</b>" };
                        col3.Controls.Add(col3Literal);
                    }
                    col3.Controls.Add(tb3);
                }
                else
                {
                    tb3.Text = "";
                    col3.Controls.Add(tb3);
                }
            }
            //else if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
            //{
            //    //if (!string.IsNullOrEmpty(tb1.Text))
            //    //{
            //    //    var col3Literal = new Literal { Text = "0" };
            //    //    col3.Controls.Add(col3Literal);
            //    //}
            //}
            //else if (estateExpense.Id_ExpenseType == (int)ExpenseType.PerTenants && estateExpense.TenantExpenses.Count > 0)
            //{
            //    //var col3Literal = new Literal { Text = "0" };

            //    //var col3Literal = new Literal { Text = Estate.Tenants.Sum(s => s.Dependents) + " locatari, <b>" + EstateExpensesManager.CalculatePertenantPrice(estateExpense) + "</b> alocat fiecăruia " };
            //    //col3.Controls.Add(col3Literal);
            //}

            return col3;
        }

        private Panel AddBodyPanelCol2(EstateExpenses estateExpense, out TextBox tb2)
        {
            // column 2
            var col2 = new Panel
            {
                CssClass = "col-md-2 col-sm-4 col-xs-6"
            };

            tb2 = new TextBox { Enabled = false, CssClass = "invoiceItemTextbox" };

            if (estateExpense.Id_Expense == (int)Expense.AjutorÎncălzire)
            {
                return col2;
            }

            //if (Association.HasStaircase && estateExpense.SplitPerStairCase.HasValue && estateExpense.SplitPerStairCase.Value)
            //{
            //    decimal? sumOfInvoices = null;

            //    foreach (StairCases stairCase in Association.StairCases)
            //    {
            //        var invoice = stairCase.Invoices.FirstOrDefault(i => i.Id_StairCase == stairCase.Id && i.Id_EstateExpense == estateExpense.Id);
            //        var lit = new Literal
            //        {
            //            Text = "Scara: " + stairCase.Nume + "  "
            //        };

            //        var tb = new TextBox
            //        {
            //            Enabled = false,
            //            CssClass = "invoiceItemTextbox",
            //            Text = ((invoice != null) ? invoice.Value.ToString() : string.Empty),
            //            Visible = estateExpense.Id_Expense != (int)Expense.AjutorÎncălzire
            //        };
            //        tb.Attributes.Add("eeId", estateExpense.Id.ToString());
            //        tb.Attributes.Add("sc", stairCase.Id.ToString());
            //        col2.Controls.Add(lit);
            //        col2.Controls.Add(tb);
            //        col2.Controls.Add(new Literal { Text = "<br>" });

            //        if (invoice != null && invoice.Value.HasValue)
            //        {
            //            if (!sumOfInvoices.HasValue)
            //            {
            //                sumOfInvoices = 0m;
            //            }
            //            sumOfInvoices = sumOfInvoices + invoice.Value.Value;
            //        }
            //    }

            //    tb2.Text = (sumOfInvoices.HasValue) ? sumOfInvoices.Value.ToString() : string.Empty;
            //}
            //else
            //{
            tb2.Attributes.Add("eeId", estateExpense.Id.ToString());
            var invoice = estateExpense.Invoices.FirstOrDefault(i => i.Id_EstateExpense == estateExpense.Id && i.Id_StairCase == null);

            string message = string.Empty;
            if (invoice != null && invoice.Value.HasValue)
            {
                message = invoice.Value.Value.ToString();
            }
            tb2.Text = message;
            col2.Controls.Add(tb2);
            //}

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

        private Panel AddBodyPanelCol0(EstateExpenses estateExpense, bool isExpensePerIndex)
        {
            // column 0
            var col0 = new Panel();
            Literal literal0 = new Literal
            {
                Text = EstateExpensesManager.StatusOfInvoices(estateExpense, isExpensePerIndex)
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

            var headerCol2 = new Panel { CssClass = "col-md-2 col-sm-2 col-xs-6" };
            var headerCol2Literal2 = new Literal { Text = "Valoare factură" };
            headerCol2.Controls.Add(headerCol2Literal2);

            var headerCol3 = new Panel { CssClass = "col-md-1 col-sm-1 col-xs-6" };
            var headerCol3Literal3 = new Literal { Text = "Valoare de redistribuit" };
            headerCol3.Controls.Add(headerCol3Literal3);

            var headerCol4 = new Panel { CssClass = "col-md-2 col-sm-3 col-xs-6" };

            var headerCol5 = new Panel { CssClass = "col-md-2 col-sm-2 col-xs-6" };
            var headerCol6 = new Panel { CssClass = "col-md-2 col-sm-2 col-xs-6" };
            var headerCol7 = new Panel { CssClass = "col-md-3 col-sm-3 col-xs-6" };
            var headerCol7Literal = new Literal { Text = "Cheltuielile apartamentelor" };
            headerCol7.Controls.Add(headerCol7Literal);

            var headerPanel = new Panel() { CssClass = "headerRow row" };
            headerPanel.Controls.Add(headerCol0);
            headerPanel.Controls.Add(headerCol1);
            headerPanel.Controls.Add(headerCol2);
            headerPanel.Controls.Add(headerCol3);
            headerPanel.Controls.Add(headerCol5);
            headerPanel.Controls.Add(headerCol6);
            headerPanel.Controls.Add(headerCol7);
            invoiceMain.Controls.Add(headerPanel);
        }

        #region click events

        public void ClickablePanel1_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int estateExpenseId;

            if (int.TryParse(btn.CommandArgument, out estateExpenseId))
            {
                var estateExpense = EstateExpensesManager.GetById(estateExpenseId);

                if (estateExpense == null)
                {
                    return;
                }
                Response.Redirect("~/Invoices/Add.aspx?year=" + year() + "&month=" + month() + "&expense=" + estateExpense.Id_Expense);
            }
        }

        protected void lblExpenseMeessageConfigure_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Config/MonthlyExpenses.aspx?year=" + year() + "&month=" + month());
        }

        private void btnRedistibuteRemainingExpense_Click(object sender, EventArgs e)
        {
            Button estateExpense = (Button)sender;
            int estateExpenseId;
            if (int.TryParse(estateExpense.CommandArgument, out estateExpenseId))
            {
                ConfigureRedistributeMessages(estateExpenseId);
            }

            invoiceRedistribute.Visible = true;
            invoiceMain.Visible = false;
            lblExpenseMeessageConfigure.Visible = false;
        }

        private void ConfigureRedistributeMessages(int estateExpenseId)
        {
            string redistributeValue = RedistributionManager.CalculateRedistributeValueAsString(estateExpenseId);
            invoiceRedistributeMessage.Text = "<br>Cheltuială de redistribuit:" + redistributeValue + "<br>";

            decimal redistributeVal;
            if (decimal.TryParse(redistributeValue, out redistributeVal))
            {
                Estates estate = AssociationsManager.GetByEstateExpenseId(estateExpenseId);
                List<Tenants> apartments = EstateExpensesManager.GetApartmentsNrThatShouldRedistributeTo(estateExpenseId);

                txtInvoiceRedistributeEqualApartment.Text = apartments.Count() + " apartamente, alocă <b>" +
                    Math.Round(redistributeVal / apartments.Count(), 2) + "</b> la fiecare apartament";

                var tenants = ApartmentsManager.GetAllByAssociationId(estate.Id);
                var allTenantDependents = apartments.Sum(t => t.Dependents);
                var valuePerTenant = "0";
                if (allTenantDependents != 0)
                {
                    valuePerTenant = Math.Round(redistributeVal / allTenantDependents, 2).ToString();
                }

                txtInvoiceRedistributeEqualTenants.Text = allTenantDependents + " locatari în bloc, alocă <b>" + valuePerTenant + "</b> per fiecare locatar";

                invoiceRedistributeEqualApartment.CommandArgument = estateExpenseId.ToString();
                invoiceRedistributeEqualTenants.CommandArgument = estateExpenseId.ToString();
                invoiceRedistributeConsumption.CommandArgument = estateExpenseId.ToString();
            }
        }

        protected void invoiceRedistributeConsumption_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int estateExpenseId;
            if (int.TryParse(btn.CommandArgument, out estateExpenseId))
            {
                EstateExpensesManager.UpdateRedistributeMethod(estateExpenseId, 3);
                Response.Redirect(Request.RawUrl);
            }
        }

        protected void invoiceRedistributeEqualTenants_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int estateExpenseId;
            if (int.TryParse(btn.CommandArgument, out estateExpenseId))
            {
                EstateExpensesManager.UpdateRedistributeMethod(estateExpenseId, 2);
                Response.Redirect(Request.RawUrl);
            }
        }

        protected void invoiceRedistributeEqualApartment_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int estateExpenseId;
            if (int.TryParse(btn.CommandArgument, out estateExpenseId))
            {
                EstateExpensesManager.UpdateRedistributeMethod(estateExpenseId, 1);
                Response.Redirect(Request.RawUrl);
            }
        }

        protected void invoiceRedistributeCancel_Click(object sender, EventArgs e)
        {
            invoiceRedistribute.Visible = false;
            invoiceMain.Visible = true;
            lblExpenseMeessageConfigure.Visible = true;
        }

        protected void lblExpenseMonthlyList_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Reports/Monthly.aspx?year=" + year() + "&month=" + month());
        }

        protected void btnCloseMonth_Click(object sender, EventArgs e)
        {
            var expenses = EstateExpensesManager.CheckCloseMonth(Association.Id, year(), month());

            if (expenses.Any())
            {
                pnlMessage.Visible = true;

                StringBuilder sb = new StringBuilder();
                sb.Append("Luna nu poate fi închisă. <br> Cheltuielile care nu sunt complete: ");

                foreach (var expense in expenses)
                {
                    sb.Append(expense.ToString() + ", ");
                }

                string result = sb.ToString().Substring(0, sb.ToString().Length - 2);

                Label lbMessage = new Label();
                lbMessage.Attributes.CssStyle.Add("color", "red");
                lbMessage.Text = result;
                pnlMessage.Controls.Add(lbMessage);

            }
            else
            {
                EstateExpensesManager.CloseMonth(Association.Id, year(), month());
                Response.Redirect(Request.RawUrl);
            }
        }

        protected void lblInvoiceList_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Invoices/List.aspx?year=" + year() + "&month=" + month());
        }

        #endregion
    }
}