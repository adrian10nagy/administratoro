
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
            AssociationExpensesManager.ConfigurePerIndex(Association, year(), month());

            mainHeader.InnerText = "Facturi pentru luna " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month()) + " " + year();
            InitializeInvoice();
        }

        private void InitializeInvoice()
        {
            if (month() != 0 && year() != 0 && Association.Id != 0)
            {
                InitializeInvoice(Association.Id, year(), month());
            }
            else
            {
                Response.Redirect("~/");
            }
        }

        private void InitializeInvoice(int associationId, int yearNr, int monthNr)
        {
            bool isMonthClosed = AssociationExpensesManager.IsMonthClosed(associationId, yearNr, monthNr);
            if (!isMonthClosed)
            {
                AddHeaderPanels();
                AddBodyPanels(associationId, yearNr, monthNr);
                btnCloseMonth.CommandArgument = "1";
            }
            else
            {
                Label lbMessage = new Label();
                lbMessage.Attributes.CssStyle.Add("color", "red");
                lbMessage.Text = "Luna este închisă, nu se mai pot face modificări. Pentru a redeschide luna contactează administratorul asociației";
                pnlMessage.Controls.Add(lbMessage);
                pnlMessage.Visible = true;
                btnCloseMonth.Text = "Deschide Luna";
                btnCloseMonth.CommandArgument = "0";
                lblExpenseMeessageConfigure.Visible = false;
            }
        }

        private void AddBodyPanels(int associationId, int yearNr, int monthNr)
        {
            string mainRowCssFormat = "col-md-12 col-sm-12 xs-12 cashBokItemsRow {0}";
            bool even = false;

            IEnumerable<Administratoro.DAL.Expenses> allExpenses = ExpensesManager.GetAllExpenses();
            var associationExpenses = AssociationExpensesManager.GetByMonthAndYearNotDisabled(associationId, yearNr, monthNr).OrderBy(ee => ee.Id_ExpenseType);

            // add expense panels
            foreach (Expenses expense in allExpenses)
            {
                var associationExpense = associationExpenses.FirstOrDefault(ea => ea.Id_Expense == expense.Id);
                if (associationExpense != null)
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

                    bool isExpensePerIndex = associationExpense.ExpenseTypes.Id == (int)ExpenseType.PerIndex;

                    var col0 = AddBodyPanelCol0(associationExpense, isExpensePerIndex);
                    var col1 = AddBodyPanelCol1(expense, associationExpense);

                    TextBox tb2;
                    var col2 = AddBodyPanelCol2(associationExpense, out tb2);

                    TextBox tb3;
                    var col3 = AddBodyPanelCol3(associationExpense, tb2, out tb3);

                    var col6 = AddBodyPanelCol6(associationExpense);
                    var col4 = AddBodyPanelCol4(associationExpense, tb2);
                    var col5 = AddBodyPanelCol5(associationExpense, isExpensePerIndex);

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

        private static Panel AddBodyPanelCol5(AssociationExpenses associationExpense, bool isExpensePerIndex)
        {
            // column 5
            var col5 = new Panel
            {
                CssClass = "col-md-3 col-sm-2 col-xs-6"
            };

            if (associationExpense.Id_Expense != (int)Expense.AjutorÎncălzire)
            {
                if (isExpensePerIndex && associationExpense.Associations.Apartments.Count() > 0)
                {
                    var message = AssociationExpensesManager.ExpensePercentageFilledInMessage(associationExpense);
                    var col5Literal = new Literal { Text = message };
                    col5.Controls.Add(col5Literal);
                    if (!message.Contains("<b>0</b> cheltuieli adăugate din <b>0</b"))
                    {
                        Button btnAddExpense = new Button
                        {
                            PostBackUrl = "AddEditExpense.aspx?id_exes=" + associationExpense.Id,
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
                    PostBackUrl = "AddEditHeatHelp.aspx?id_exes=" + associationExpense.Id,
                    Text = "Adaugă/Modifică"
                };

                col5.Controls.Add(btnAddHeatHelp);
            }

            return col5;
        }

        private Panel AddBodyPanelCol4(AssociationExpenses associationExpense, TextBox tb1)
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
            btn4.CommandArgument = associationExpense.Id.ToString();
            btn4.Visible = associationExpense.Id_Expense != (int)Expense.AjutorÎncălzire;
            col4.Controls.Add(btn4);

            return col4;
        }

        private Panel AddBodyPanelCol6(AssociationExpenses associationExpense)
        {
            // column 6
            var col6 = new Panel
            {
                CssClass = "col-md-2 col-sm-2 col-xs-6"
            };

            if (associationExpense.ExpenseTypes.Id == (int)ExpenseType.PerIndex)
            {
                string percentage = AssociationExpensesManager.GetPercentageAsString(associationExpense);

                Button btnRedistibuteRemainingExpense = new Button
                {
                    CssClass = "btnRedistibuteRemainingExpense",
                    Visible = (percentage == "100" || percentage == string.Empty) ? true : false,
                    Text = (!associationExpense.RedistributeType.HasValue) ? "Redistribuie" : "Redistibuit " + associationExpense.AssociationExpensesRedistributionTypes.Value + ", MODIFICĂ",
                    CommandArgument = associationExpense.Id.ToString()
                };
                btnRedistibuteRemainingExpense.Click += btnRedistibuteRemainingExpense_Click;
                col6.Controls.Add(btnRedistibuteRemainingExpense);
            }
            return col6;
        }

        private static Panel AddBodyPanelCol3(AssociationExpenses associationExpense, TextBox tb1, out TextBox tb3)
        {
            // column 3
            var col3 = new Panel
            {
                CssClass = "col-md-1 col-sm-4 col-xs-6"
            };
            tb3 = new TextBox { Enabled = false, CssClass = "invoiceItemTextbox" };

            if (associationExpense.Id_ExpenseType == (int)ExpenseType.PerIndex)
            {
                if (!associationExpense.RedistributeType.HasValue)
                {
                    var percentage = AssociationExpensesManager.GetPercentageAsString(associationExpense);
                    if (percentage == "100")
                    {
                        tb3.Text = RedistributionManager.RedistributeValuePerIndexAsString(associationExpense);
                    }

                    if (tb3.Text == "0,0000000")
                    {
                        tb3.Text = string.Empty;
                    }

                    if (!string.IsNullOrEmpty(tb3.Text))
                    {
                        var col3Literal = new Literal { Text = "<b>" + tb1.Text + " - " + associationExpense.ApartmentExpenses.Sum(ee => ee.Value) + "</b>" };
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
            //else if (apartmentExpense.Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
            //{
            //    //if (!string.IsNullOrEmpty(tb1.Text))
            //    //{
            //    //    var col3Literal = new Literal { Text = "0" };
            //    //    col3.Controls.Add(col3Literal);
            //    //}
            //}
            //else if (apartmentExpense.Id_ExpenseType == (int)ExpenseType.PerTenants && estateapartmentExpenseExpense.TenantExpenses.Count > 0)
            //{
            //    //var col3Literal = new Literal { Text = "0" };

            //    //var col3Literal = new Literal { Text = Estate.Tenants.Sum(s => s.Dependents) + " persoane, <b>" + ApartmentExpensesManager.CalculatePertenantPrice(apartmentExpense) + "</b> alocat fiecăruia " };
            //    //col3.Controls.Add(col3Literal);
            //}

            return col3;
        }

        private static Panel AddBodyPanelCol2(AssociationExpenses associationExpense, out TextBox tb2)
        {
            // column 2
            var col2 = new Panel
            {
                CssClass = "col-md-2 col-sm-4 col-xs-6"
            };

            tb2 = new TextBox { Enabled = false, CssClass = "invoiceItemTextbox" };

            if (associationExpense.Id_Expense == (int)Expense.AjutorÎncălzire)
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
            tb2.Attributes.Add("eeId", associationExpense.Id.ToString());
            var invoice = associationExpense.Invoices.FirstOrDefault(i => i.Id_EstateExpense == associationExpense.Id && i.Id_StairCase == null);

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

        private static Panel AddBodyPanelCol1(Expenses expense, AssociationExpenses associationExpense)
        {
            // column 1
            var col1 = new Panel
            {
                CssClass = "col-md-1 col-sm-2 col-xs-6",
            };
            Literal literal1 = new Literal
            {
                Text = "<b>" + expense.Name + "</b> (" + associationExpense.ExpenseTypes.Name + ")"
            };
            col1.Controls.Add(literal1);
            return col1;
        }

        private static Panel AddBodyPanelCol0(AssociationExpenses associationExpense, bool isExpensePerIndex)
        {
            // column 0
            var col0 = new Panel();
            Literal literal0 = new Literal
            {
                Text = AssociationExpensesManager.StatusOfInvoices(associationExpense, isExpensePerIndex)
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
            int associationExpenseId;

            if (int.TryParse(btn.CommandArgument, out associationExpenseId))
            {
                var associationExpense = AssociationExpensesManager.GetById(associationExpenseId);

                if (associationExpense == null)
                {
                    return;
                }

                Response.Redirect("~/Invoices/Add.aspx?year=" + year() + "&month=" + month() + "&expense=" + associationExpense.Id_Expense);
            }
        }

        protected void lblExpenseMeessageConfigure_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Config/MonthlyExpenses.aspx?year=" + year() + "&month=" + month());
        }

        private void btnRedistibuteRemainingExpense_Click(object sender, EventArgs e)
        {
            Button associationExpense = (Button)sender;
            int associationExpenseId;
            if (int.TryParse(associationExpense.CommandArgument, out associationExpenseId))
            {
                ConfigureRedistributeMessages(associationExpenseId);
            }

            invoiceRedistribute.Visible = true;
            invoiceMain.Visible = false;
            lblExpenseMeessageConfigure.Visible = false;
        }

        private void ConfigureRedistributeMessages(int associationExpenseId)
        {
            string redistributeValue = RedistributionManager.CalculateRedistributeValueAsString(associationExpenseId);
            invoiceRedistributeMessage.Text = "<br>Cheltuială de redistribuit:" + redistributeValue + "<br>";

            decimal redistributeVal;
            if (decimal.TryParse(redistributeValue, out redistributeVal))
            {
                IEnumerable<Apartments> apartmentsForRedistribute = AssociationExpensesManager.GetApartmentsNrThatShouldRedistributeTo(associationExpenseId);

                txtInvoiceRedistributeEqualApartment.Text = apartmentsForRedistribute.Count() + " apartamente, alocă <b>" +
                    Math.Round(redistributeVal / apartmentsForRedistribute.Count(), 2) + "</b> la fiecare apartament";

                var allApartmentDependents = apartmentsForRedistribute.Sum(t => t.Dependents);
                var valuePerApartment = "0";
                if (allApartmentDependents != 0)
                {
                    valuePerApartment = Math.Round(redistributeVal / allApartmentDependents, 2).ToString();
                }

                txtInvoiceRedistributeEqualTenants.Text = allApartmentDependents + " persoane în bloc, alocă <b>" + valuePerApartment + "</b> per fiecare locatar";

                invoiceRedistributeEqualApartment.CommandArgument = associationExpenseId.ToString();
                invoiceRedistributeEqualTenants.CommandArgument = associationExpenseId.ToString();
                invoiceRedistributeConsumption.CommandArgument = associationExpenseId.ToString();
            }
        }

        protected void invoiceRedistributeConsumption_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int associationExpenseId;
            if (int.TryParse(btn.CommandArgument, out associationExpenseId))
            {
                AssociationExpensesManager.UpdateRedistributeMethod(associationExpenseId, 3);
                Response.Redirect(Request.RawUrl);
            }
        }

        protected void invoiceRedistributeEqualTenants_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int associationExpenseId;
            if (int.TryParse(btn.CommandArgument, out associationExpenseId))
            {
                AssociationExpensesManager.UpdateRedistributeMethod(associationExpenseId, 2);
                Response.Redirect(Request.RawUrl);
            }
        }

        protected void invoiceRedistributeEqualApartment_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int associationExpenseId;
            if (int.TryParse(btn.CommandArgument, out associationExpenseId))
            {
                AssociationExpensesManager.UpdateRedistributeMethod(associationExpenseId, 1);
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
            var expenses = AssociationExpensesManager.CheckCloseMonth(Association.Id, year(), month());

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
                bool shouldMonthClose = btnCloseMonth.CommandArgument == "0" ? false : true;
                AssociationExpensesManager.OpenCloseMonth(Association.Id, year(), month(), shouldMonthClose);
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