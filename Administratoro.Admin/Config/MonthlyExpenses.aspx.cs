
namespace Admin.Config
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Managers;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public partial class MonthlyExpenses : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                InitializeExpenses();
                InitializeMonths();
            }
            else if (Page.IsPostBack && step22.Visible)
            {
                InitializeExpenses();
            }

            expenseListHref1.Attributes["class"] = "selected";
            expenseListHref1.Attributes["isdone"] = "0";
            expenseListHref2.Attributes["class"] = "disabled";
            expenseListHref2.Attributes["isdone"] = "0";
            expenseListHref3.Attributes["class"] = "disabled";
            expenseListHref3.Attributes["isdone"] = "0";
        }

        private void InitializeExpenses()
        {
            tblMonthlyExpenses.Rows.Clear();
            int month = 0;
            var i = 1;
            if (int.TryParse(drpExpenseMonth.SelectedValue, out month))
            {
                var estate = (Estates)Session[SessionConstants.SelectedEstate];
                if (estate != null)
                {
                    var ee = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearNotDisabled(estate.Id, 2017, month);
                    var eeAlsoDisabled = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearIncludingDisabled(estate.Id, 2017, month);

                    var expenses = ExpensesManager.GetAllExpensesAsList();

                    foreach (var item in expenses)
                    {
                        TableRow row = new TableRow();

                        // add expense exists
                        TableCell expenseExists = new TableCell();
                        CheckBox esexExists = new CheckBox();
                        esexExists.AutoPostBack = false;
                        esexExists.ID = String.Format("esateExpense{0}", item.Id);
                        esexExists.Checked = isExpenseSelected(item, ee, month);
                        expenseExists.Controls.Add(esexExists);
                        row.Cells.Add(expenseExists);

                        // add expense name
                        TableCell expenseName = new TableCell
                        {
                            Text = item.Name
                        };
                        row.Cells.Add(expenseName);

                        // add expense type
                        TableCell expenseType = new TableCell();
                        DropDownList dp = new DropDownList();
                        EstateExpenses esex = eeAlsoDisabled.FirstOrDefault(s => s.Id_Expense == item.Id && s.Month == month && s.Year == 2017 && s.Id_Estate == 1);

                        var selected1 = isDplExpenseTypesSelected(esex, ExpenseType.PerIndex);
                        dp.Items.Add(new ListItem
                        {
                            Value = "1",
                            Text = "Individuală prin indecși",
                            Selected = selected1
                        });

                        var selected2 = isDplExpenseTypesSelected(esex, ExpenseType.PerCotaIndiviza);
                        dp.Items.Add(new ListItem
                        {
                            Value = "2",
                            Text = "Cotă indiviză de proprietate",
                            Selected = selected2
                        });

                        var selected3 = isDplExpenseTypesSelected(esex, ExpenseType.PerTenants);
                        dp.Items.Add(new ListItem
                        {
                            Value = "3",
                            Text = "Per număr locatari imobil",
                            Selected = selected3
                        });

                        expenseType.Controls.Add(dp);
                        row.Cells.Add(expenseType);
                        i++;

                        tblMonthlyExpenses.Rows.Add(row);
                    }
                }
            }
        }

        private static bool isExpenseSelected(Administratoro.DAL.Expenses expense, List<EstateExpenses> ee, int month)
        {
            bool result = false;
            if (ee.Where(e => e.Id_Expense == expense.Id && e.Month == month && e.Year == 2017 && !e.WasDisabled).Any())
            {
                result = true;
            }

            return result;
        }

        private static bool isDplExpenseTypesSelected(Administratoro.DAL.EstateExpenses expense, ExpenseType expenseType)
        {
            bool result = false;

            if (expense != null)
            {
                result = expense.Id_ExpenseType == (int)expenseType;
            }

            return result;
        }

        private void InitializeMonths()
        {
            drpExpenseMonth.Items.Clear();
            drpExpenseMonth.Items.Add(new ListItem
            {
                Text = "Iulie",
                Value = "7"

            });

            drpExpenseMonth.Items.Add(new ListItem
            {
                Text = "August",
                Value = "8"

            });

            drpExpenseMonth.Items.Add(new ListItem
            {
                Text = "Septembrie",
                Value = "9"

            });


            drpExpenseMonth.Items.Add(new ListItem
            {
                Text = "Octombrie",
                Value = "10"

            });
        }

        protected void btnStep1_Click(object sender, EventArgs e)
        {
            step22Message.Text = "Bifează cheltuielile de afișat pentru luna " + drpExpenseMonth.SelectedItem.Text;
            step11.Visible = false;
            step22.Visible = true;
            step33.Visible = false;
            expenseListHref1.Attributes["class"] = "done";
            expenseListHref1.Attributes["isdone"] = "1";
            expenseListHref2.Attributes["class"] = "selected";
            InitializeExpenses();
        }

        protected void btnStep2_Click(object sender, EventArgs e)
        {
            step11.Visible = false;
            step22.Visible = false;
            step33.Visible = true;
            expenseListHref1.Attributes["class"] = "done";
            expenseListHref1.Attributes["isdone"] = "1";
            expenseListHref2.Attributes["class"] = "done";
            expenseListHref3.Attributes["class"] = "selected";

            int month = 0;
            if (int.TryParse(drpExpenseMonth.SelectedValue, out month))
            {
                var estate = (Estates)Session[SessionConstants.SelectedEstate];
                if (estate != null)
                {
                    var existingEstateExpenses = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearNotDisabled(estate.Id, 2017, month);
                    var existingEstateExpensesIncludingDisabled = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearIncludingDisabled(estate.Id, 2017, month);

                    foreach (TableRow row in tblMonthlyExpenses.Rows)
                    {
                        if (row != null && row.Cells.Count > 2 &&
                            row.Cells[0] != null && row.Cells[0].Controls.Count > 0
                            && row.Cells[0].Controls[0] != null)
                        {
                            if (row.Cells[0].Controls[0] is CheckBox && row.Cells[2].Controls[0] is DropDownList)
                            {
                                CheckBox cb = (CheckBox)row.Cells[0].Controls[0];
                                DropDownList dpExpenseType = (DropDownList)row.Cells[2].Controls[0];
                                int expenseId = 0;
                                if (int.TryParse(cb.ID.Replace("esateExpense", ""), out expenseId))
                                {
                                    bool existingEstateExpensesContainsItem = existingEstateExpenses.Select(esex => esex.Id_Expense).Contains(expenseId);
                                    bool existingEstateExpensesContainsItemAsDisabled = existingEstateExpensesIncludingDisabled.Where(es => es.WasDisabled).Select(esex => esex.Id_Expense).Contains(expenseId);
                                    EstateExpenses ee = null;

                                    // if selected and non existing in the prev. config
                                    if (cb.Checked && existingEstateExpensesContainsItemAsDisabled)
                                    {
                                        // enables it
                                        ee = EstateExpensesManager.GetEstateExpensesByMonthAndYearAndDisabled(estate.Id, expenseId, 2017, month);
                                        EstateExpensesManager.MarkEstateExpensesDisableProperty(ee, false);
                                    }
                                    else if (!cb.Checked && existingEstateExpensesContainsItem)
                                    {
                                        // disables it
                                        ee = EstateExpensesManager.GetEstateExpensesByMonthAndYearAndDisabled(estate.Id, expenseId, 2017, month, false);
                                        EstateExpensesManager.MarkEstateExpensesDisableProperty(ee, true);
                                    }
                                    else if (cb.Checked && !existingEstateExpensesContainsItem)
                                    {
                                        ee = EstateExpensesManager.GetEstateExpensesByMonthAndYearAndDisabled(expenseId, estate.Id, 2017, month);
                                        if (ee != null)
                                        {
                                            // disables it
                                            EstateExpensesManager.MarkEstateExpensesDisableProperty(ee, true);
                                        }
                                        else
                                        {
                                            // adds it
                                            EstateExpensesManager.AddEstateExpensesByTenantAndMonth(estate.Id, expenseId, month, 2017, dpExpenseType.SelectedValue);
                                        }
                                    }

                                    ExpenseType selectedExpenseType;
                                    if (Enum.TryParse<ExpenseType>(dpExpenseType.SelectedValue, out  selectedExpenseType))
                                    {
                                        if (ee == null)
                                        {
                                            ee = EstateExpensesManager.GetEstateExpenses(estate.Id, expenseId, 2017, month);
                                        }

                                        if (ee != null && (ExpenseType)ee.ExpenseTypes.Id != selectedExpenseType)
                                        {
                                            EstateExpensesManager.UpdateEstateExpenseType(ee, selectedExpenseType);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void btnBackStep2_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Config/MonthlyExpenses.aspx");
        }

        protected void btnStep3Next1_Click(object sender, EventArgs e)
        {
            Response.Redirect("..\\Expenses\\Dashboard.aspx", true);
        }

        protected void btnStep3Next2_Click(object sender, EventArgs e)
        {
            step11.Visible = true;
            step22.Visible = false;
            step33.Visible = false;
            expenseListHref1.Attributes["class"] = "selected";
            expenseListHref1.Attributes["isdone"] = "0";
            expenseListHref2.Attributes["class"] = "disabled";
            expenseListHref2.Attributes["isdone"] = "0";
            expenseListHref3.Attributes["class"] = "disabled";
            expenseListHref3.Attributes["isdone"] = "0";
        }
    }
}