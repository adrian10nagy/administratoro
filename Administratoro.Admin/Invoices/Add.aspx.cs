using Administratoro.BL.Constants;
using Administratoro.BL.Managers;
using Administratoro.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Administratoro.BL.Extensions;

namespace Admin.Invoices
{
    public partial class Add : BasePage
    {
        public int? _year
        {
            get
            {
                return Request.QueryString["year"].GetYear();
            }
        }

        public int? _month
        {
            get
            {
                return Request.QueryString["month"].GetMonth();
            }
        }

        private int? _expense
        {
            get
            {
                return Request.QueryString["expense"].GetInt();

            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                InitializeYearMonth();
                InitializeExpenses();
            }
            InitializeValueField();
        }

        private void InitializeYearMonth()
        {

            drpInvoiceYearMonth.Items.Clear();
            var yearMonths = EstateExpensesManager.GetAllMonthsAndYearsAvailableByEstateId(Association.Id);
            bool hasValue = _year.HasValue && _month.HasValue;
            for (int i = 0; i < yearMonths.Count; i++)
            {
                var yearMonth = yearMonths[i];
                drpInvoiceYearMonth.Items.Add(new ListItem
                {
                    Value = yearMonth.Year + "-" + yearMonth.Month,
                    Text = "Anul:" + yearMonth.Year + " Luna:" + yearMonth.Month,
                    Selected = hasValue ? (yearMonth.Month == _month.Value && yearMonth.Year == _year.Value) : i + 1 == yearMonths.Count
                });
            }

            if (hasValue)
            {
                drpInvoiceYearMonth.Enabled = false;
            }
        }

        private void InitializeExpenses()
        {
            var estateExpenses = GetEstateExpenses();
            drpInvoiceExpenses.Items.Clear();
            foreach (var estateExpense in estateExpenses)
            {
                drpInvoiceExpenses.Items.Add(new ListItem
                {
                    Value = estateExpense.Expenses.Id.ToString(),
                    Text = estateExpense.Expenses.Name.ToString(),
                    Selected = _expense.HasValue && _expense.Value == estateExpense.Id_Expense
                });
            }

            drpInvoiceExpenses.Items.Add(new ListItem
            {
                Value = "24",
                Text = "Diverse"
            });

            if (_expense.HasValue)
            {
                drpInvoiceExpenses.Enabled = false;
            }
        }

        private List<EstateExpenses> GetEstateExpenses()
        {
            var result = new List<EstateExpenses>();

            if (!string.IsNullOrEmpty(drpInvoiceYearMonth.SelectedValue))
            {
                var yearMonth = drpInvoiceYearMonth.SelectedValue.Split('-');
                if (yearMonth.Length == 2)
                {
                    int year;
                    int month;

                    if (int.TryParse(yearMonth[0], out year) && int.TryParse(yearMonth[1], out month))
                    {
                        result = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearNotDisabled(Association.Id, year, month);
                    }
                }
            }

            return result;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/");
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            //InitializeValueField();
            var yearMonth = drpInvoiceYearMonth.SelectedValue.Split('-');

            if (yearMonth.Length == 2)
            {

                int year;
                int month;
                int expenseId;
                if (int.TryParse(drpInvoiceExpenses.SelectedValue, out expenseId) &&
                    int.TryParse(yearMonth[0], out year) &&
                    int.TryParse(yearMonth[1], out month))
                {
                    if (expenseId == (int)Expense.Diverse)
                    {
                        SaveDiverse(year, month, Association.Id, expenseId);
                    }
                    else
                    {
                        SaveDefault(year, month, Association.Id, expenseId);
                    }
                }
            }

            var estate = AssociationsManager.GetById(Association.Id);
            Session[SessionConstants.SelectedAssociation] = estate;
            if(_year.HasValue && _month.HasValue)
            {
                Response.Redirect("~/Reports/Monthly.aspx?year=" + _year.Value + "&month=" + _month.Value);
            }
            else
            {
                Response.Redirect("~/");
            }
        }

        private void SaveDiverse(int year, int month, int associationId, int expenseId)
        {
            foreach (var control in pnInvoiceValues.Controls)
            {
                if (control is Panel)
                {
                    var innerControls = control as Panel;
                    if (innerControls != null && innerControls.Controls.Count == 2)
                    {
                        var theDescriptionControl = innerControls.Controls[1] as Panel;
                        var theInvoiceValuecontrol = innerControls.Controls[0] as Panel;
                        if (theDescriptionControl != null && theDescriptionControl.Controls.Count == 1 && theDescriptionControl.Controls[0] is TextBox &&
                            theInvoiceValuecontrol != null && theInvoiceValuecontrol.Controls.Count == 1 && theInvoiceValuecontrol.Controls[0] is TextBox)
                        {
                            var theDescription = theDescriptionControl.Controls[0] as TextBox;
                            var theInvoiceValue = theInvoiceValuecontrol.Controls[0] as TextBox;

                            int? invoiceId = null;
                            int invoiceIdValue;
                            if (int.TryParse(theInvoiceValue.ID.Replace("tbInvoiceId", string.Empty), out invoiceIdValue))
                            {
                                invoiceId = invoiceIdValue;
                            }
                            var ee = EstateExpensesManager.GetAllMonthYearAssoiationExpense(associationId, expenseId, year, month);
                            if (ee == null)
                            {
                                EstateExpensesManager.AddEstateExpensesByTenantAndMonth(associationId, expenseId, month, year, ((int)ExpenseType.PerTenants).ToString(), false);
                                ee = EstateExpensesManager.GetAllMonthYearAssoiationExpense(associationId, expenseId, year, month);
                            }

                            decimal? theValue = null;
                            decimal tempValue;
                            if (decimal.TryParse(theInvoiceValue.Text, out tempValue))
                            {
                                theValue = tempValue;
                            }

                            int? stairCaseId = null;
                            int stairCaseIdValue;
                            var stairCaseIDUnprocces = theDescription.ID.Substring(theDescription.ID.IndexOf("tbStairCase") + 11);
                            if (int.TryParse(stairCaseIDUnprocces, out stairCaseIdValue))
                            {
                                stairCaseId = stairCaseIdValue;
                            }

                            if (invoiceId.HasValue)
                            {
                                if (theValue.HasValue)
                                {
                                    var invoice = InvoicesManager.GetDiverseById(invoiceId.Value);
                                    if (invoice != null)
                                    {
                                        InvoicesManager.Update(invoice, theValue, stairCaseId, theDescription.Text);
                                    }
                                }
                                else
                                {
                                    InvoicesManager.Remove(invoiceId.Value);
                                }
                            }
                            else if (theValue.HasValue)
                            {
                                InvoicesManager.AddDiverse(ee, theValue, theDescription.Text);
                            }
                        }
                    }
                }
            }
        }

        private void SaveDefault(int year, int month, int associationId, int expenseId)
        {
            EstateExpenses estateExpense = EstateExpensesManager.GetAllMonthYearAssoiationExpense(associationId, expenseId, year, month);
            if (estateExpense.SplitPerStairCase.HasValue && estateExpense.SplitPerStairCase.Value)
            {
                foreach (var control in pnInvoiceValues.Controls)
                {
                    if (control is Panel)
                    {
                        var thePanelControl = control as Panel;
                        if (thePanelControl != null && thePanelControl.Controls.Count == 2)
                        {
                            var theDescriptionControl = thePanelControl.Controls[1] as Panel;
                            var theInvoiceValuecontrol = thePanelControl.Controls[0] as Panel;
                            if (theDescriptionControl != null && theDescriptionControl.Controls.Count == 1 && theDescriptionControl.Controls[0] is TextBox &&
                                theInvoiceValuecontrol != null && theInvoiceValuecontrol.Controls.Count == 1 && theInvoiceValuecontrol.Controls[0] is TextBox)
                            {
                                var theDescription = theDescriptionControl.Controls[0] as TextBox;
                                var theInvoiceValue = theInvoiceValuecontrol.Controls[0] as TextBox;

                                int? invoiceId = null;
                                int invoiceIdValue;
                                if (int.TryParse(theInvoiceValue.ID.Replace("tbInvoiceId", string.Empty), out invoiceIdValue))
                                {
                                    invoiceId = invoiceIdValue;
                                }

                                int? stairCaseId = null;
                                int stairCaseIdValue;
                                var stairCaseIDUnprocces = theDescription.ID.Substring(theDescription.ID.IndexOf("tbStairCase") + 11);

                                if (int.TryParse(stairCaseIDUnprocces, out stairCaseIdValue))
                                {
                                    stairCaseId = stairCaseIdValue;
                                }

                                decimal? theValue = null;
                                decimal tempValue;
                                if (decimal.TryParse(theInvoiceValue.Text, out tempValue))
                                {
                                    theValue = tempValue;
                                }

                                if (invoiceId.HasValue)
                                {
                                    var invoice = InvoicesManager.GetDiverseById(invoiceId.Value);
                                    if (invoice != null)
                                    {
                                        InvoicesManager.Update(invoice, theValue, stairCaseId, theDescription.Text);
                                    }
                                }
                                else
                                {
                                    //InvoicesManager.Add(theValue, year, month, expenseId, invoiceId, associationId);
                                    InvoicesManager.AddOrUpdate(estateExpense, theValue, stairCaseId, theDescription.Text);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                bool shouldUpdate = true;
                foreach (var control in pnInvoiceValues.Controls)
                {
                    if (control is TextBox)
                    {
                        var theControl = (TextBox)control;

                        decimal? upladValue = null;
                        decimal value;
                        if (!string.IsNullOrEmpty(theControl.Text))
                        {
                            if (decimal.TryParse(theControl.Text, out value))
                            {
                                upladValue = value;
                            }
                            else
                            {
                                theControl.Attributes.Add("style", "background-color:red");
                                shouldUpdate = false;
                            }
                        }

                        if (shouldUpdate)
                        {
                            InvoicesManager.AddOrUpdate(estateExpense, upladValue);
                        }
                    }
                }
            }
        }

        protected void drpInvoiceAssociations_SelectedIndexChanged(object sender, EventArgs e)
        {
            InitializeYearMonth();
            InitializeExpenses();
            pnInvoiceValues.Controls.Clear();
            InitializeValueField();
        }

        protected void drpInvoiceYearMonth_SelectedIndexChanged(object sender, EventArgs e)
        {
            InitializeExpenses();
            pnInvoiceValues.Controls.Clear();
            InitializeValueField();
        }

        protected void drpInvoiceExpenses_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnInvoiceValues.Controls.Clear();
            InitializeValueField();
        }

        private void InitializeValueField()
        {
            var yearMonth = drpInvoiceYearMonth.SelectedValue.Split('-');

            if (yearMonth.Length == 2)
            {
                int year;
                int month;
                int expenseId;
                if (int.TryParse(drpInvoiceExpenses.SelectedValue, out expenseId) &&
                    int.TryParse(yearMonth[0], out year) &&
                    int.TryParse(yearMonth[1], out month))
                {
                    EstateExpenses estateExpenses = EstateExpensesManager.GetAllMonthYearAssoiationExpense(Association.Id, expenseId, year, month);

                    if (expenseId == (int)Expense.Diverse)
                    {
                        InitializeValueFieldAddExtraControlsForDiverse(year, month, Association.Id, estateExpenses);
                    }
                    else
                    {
                        InitializeValueFieldAddcontrols(year, month, Association.Id, expenseId, estateExpenses);
                    }
                }
            }
        }

        private void InitializeValueFieldAddExtraControlsForDiverse(int year, int month, int associationId, EstateExpenses estateExpense)
        {
            InitializeValueFieldAddColumnHeaders();

            if (estateExpense != null)
            {
                var invoices = InvoicesManager.GetDiverseByEstateExpense(estateExpense.Id);

                InitializeValueFieldAddInvoices(invoices);
            }

            Panel panelMain = new Panel
            {
                CssClass = "col-md-12 col-xs-12"
            };

            Panel panel3 = new Panel
            {
                CssClass = "col-md-6"
            };
            TextBox tbValue = new TextBox
            {
                CssClass = "form-control",
                Text = string.Empty,
                ID = "tb"
            };
            panel3.Controls.Add(tbValue);

            Panel panel4 = new Panel
            {
                CssClass = "col-md-6"
            };
            TextBox tbDescription = new TextBox
            {
                CssClass = "form-control",
                Text = string.Empty,
                ID = "-1tbStairCase"
            };
            panel4.Controls.Add(tbDescription);

            panelMain.Controls.Add(panel3);
            panelMain.Controls.Add(panel4);

            pnInvoiceValues.Controls.Add(panelMain);

        }

        private void InitializeValueFieldAddInvoices(List<Administratoro.DAL.Invoices> invoices)
        {
            foreach (var item in invoices)
            {
                Panel panelMain = new Panel
                {
                    CssClass = "col-md-12 col-xs-12"
                };

                Panel panel1 = new Panel
                {
                    CssClass = "col-md-6"
                };

                TextBox tbValue = new TextBox
                {
                    Text = item.Value.HasValue ? item.Value.Value.ToString() : string.Empty,
                    CssClass = "form-control",
                    ID = "tbInvoiceId" + item.Id
                };
                panel1.Controls.Add(tbValue);

                Panel panel2 = new Panel
                {
                    CssClass = "col-md-6"
                };

                TextBox tbDescription = new TextBox
                {
                    CssClass = "form-control",
                    Text = item.Description,
                    ID = item.Id + "tbStairCase" + item.Id_StairCase
                };
                panel2.Controls.Add(tbDescription);
                //var litValue = item.StairCases != null ? item.StairCases.Nume : string.Empty;
                //Literal literal = new Literal
                //{
                //    Text = "Scara " + litValue
                //};


                panelMain.Controls.Add(panel1);
                panelMain.Controls.Add(panel2);

                pnInvoiceValues.Controls.Add(panelMain);
            }
        }

        private void InitializeValueFieldAddColumnHeaders()
        {
            Panel panel1 = new Panel
            {
                CssClass = "col-md-6"
            };
            Label lb1 = new Label
            {
                Text = "Valoare factură"
            };
            panel1.Controls.Add(lb1);
            pnInvoiceValues.Controls.Add(panel1);

            Panel panel2 = new Panel
            {
                CssClass = "col-md-6"
            };
            Label lb2 = new Label
            {
                Text = "Denumire factură"
            };
            panel2.Controls.Add(lb2);
            pnInvoiceValues.Controls.Add(panel2);
        }

        private void InitializeValueFieldAddcontrols(int year, int month, int associationId, int expenseId, EstateExpenses estateExpense)
        {
            if (estateExpense != null && estateExpense.SplitPerStairCase.HasValue && estateExpense.SplitPerStairCase.Value)
            {
                InitializeValueFieldAddColumnHeaders();
                var invoices = InvoicesManager.GetAllByAssotiationYearMonthExpenseId(associationId, expenseId, year, month, true);

                if (invoices.Count != Association.StairCases.Count)
                {
                    InvoicesManager.AddDefault(associationId, expenseId, year, month, true);
                    invoices = InvoicesManager.GetAllByAssotiationYearMonthExpenseId(associationId, expenseId, year, month, true);
                }

                InitializeValueFieldAddInvoices(invoices);
            }
            else
            {
                var invoice = InvoicesManager.GetAllByAssotiationYearMonthExpenseId(associationId, expenseId, year, month, false);

                if (invoice.Count != 1)
                {
                    InvoicesManager.AddDefault(associationId, expenseId, year, month, false);
                    invoice = InvoicesManager.GetAllByAssotiationYearMonthExpenseId(associationId, expenseId, year, month, false);
                }

                var text = string.Empty;

                if (invoice.Count == 1 && invoice[0].Value.HasValue)
                {
                    text = invoice[0].Value.Value.ToString();
                }

                TextBox tb = new TextBox
                {
                    CssClass = "form-control",
                    Text = text,
                    ID = "tb001"
                };
                pnInvoiceValues.Controls.Add(tb);
            }
        }
    }
}