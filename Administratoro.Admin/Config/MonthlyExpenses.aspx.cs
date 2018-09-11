
namespace Admin.Config
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Extensions;
    using Administratoro.BL.Managers;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI.WebControls;

    public partial class MonthlyExpenses : System.Web.UI.Page
    {
        private int? Year()
        {
            var yearId = Request.QueryString["year"];
            int year;
            if (!int.TryParse(yearId, out year))
            {
                return null;
            }

            return year;
        }

        private int? Month()
        {
            var monthId = Request.QueryString["month"];
            int month;
            if (!int.TryParse(monthId, out month) || month > 13 || month < 0)
            {
                return null;
            }

            return month;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                InitializeExpenses();
                InitializeMonths(Year(), Month());
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

            if (Year() != null && Month() != null && step11.Visible)
            {
                ConfigureStep1();
            }
        }

        private void InitializeExpenses()
        {
            //tblMonthlyExpenses.Rows.Clear();
            int month;
            int _year = Year().HasValue ? Year().Value : 2017;
            if (int.TryParse(drpExpenseMonth.SelectedValue, out month))
            {
                var estate = (Associations)Session[SessionConstants.SelectedAssociation];
                if (estate != null)
                {
                    var ee = AssociationExpensesManager.GetByMonthAndYearNotDisabled(estate.Id, _year, month);
                    var eeAlsoDisabled = AssociationExpensesManager.GetAllAssociationExpensesByMonthAndYearIncludingDisabled(estate.Id, _year, month);

                    IEnumerable<Expenses> expenses = ExpensesManager.GetAllExpenses();

                    foreach (var expense in expenses)
                    {
                        TableRow row = new TableRow();

                        // add expense that were added
                        TableCell expenseExists = new TableCell();
                        CheckBox esexExists = new CheckBox();
                        esexExists.AutoPostBack = false;
                        esexExists.ID = String.Format("esateExpense{0}", expense.Id);
                        esexExists.Checked = IsExpenseSelected(expense, ee, _year, month);
                        expenseExists.Controls.Add(esexExists);
                        row.Cells.Add(expenseExists);

                        // add expense name
                        TableCell expenseName = new TableCell
                        {
                            Text = expense.Name
                        };
                        row.Cells.Add(expenseName);

                        // add expense type
                        TableCell expenseType = new TableCell();
                        AssociationExpenses esex = eeAlsoDisabled.FirstOrDefault(s => s.Id_Expense == expense.Id && s.Month == month && s.Year == _year && s.Id_Estate == estate.Id);

                        DropDownList dp = new DropDownList();
                        if (expense.Id != (int)Expense.AjutorÎncălzire)
                        {
                            var selected1 = IsDplExpenseTypesSelected(esex, ExpenseType.PerIndex);
                            dp.Items.Add(new ListItem
                            {
                                Value = "1",
                                Text = "Individuală prin indecși",
                                Selected = selected1
                            });

                            var selected2 = IsDplExpenseTypesSelected(esex, ExpenseType.PerCotaIndiviza);
                            dp.Items.Add(new ListItem
                            {
                                Value = "2",
                                Text = "Cotă indiviză de proprietate",
                                Selected = selected2
                            });

                            var selected3 = IsDplExpenseTypesSelected(esex, ExpenseType.PerNrTenants);
                            dp.Items.Add(new ListItem
                            {
                                Value = "3",
                                Text = "Per număr persoane imobil",
                                Selected = selected3
                            });

                            var selected4 = IsDplExpenseTypesSelected(esex, ExpenseType.PerApartament);
                            dp.Items.Add(new ListItem
                            {
                                Value = ((int)ExpenseType.PerApartament).ToString(),
                                Text = ExpenseType.PerApartament.ToDescription(),
                                Selected = selected4
                            });
                        }

                        dp.Items.Add(new ListItem
                        {
                            Value = "6",
                            Text = "Individual",
                        });

                        expenseType.Controls.Add(dp);
                        row.Cells.Add(expenseType);

                        if (estate.HasStaircase)
                        {
                            TableCell tcStairCase = new TableCell();
                            CheckBox stairCaseSplit = new CheckBox();
                            stairCaseSplit.AutoPostBack = false;
                            stairCaseSplit.Checked = IsStairCaseSplitSelected(expense, ee, _year, month);
                            tcStairCase.Controls.Add(stairCaseSplit);
                            row.Cells.Add(tcStairCase);
                        }
                        else
                        {
                            tblMonthlyExpensesStairCaseSplit.Visible = false;
                        }

                        tblMonthlyExpenses.Rows.Add(row);
                    }
                }
            }
        }

        private static bool IsStairCaseSplitSelected(Expenses expense, IEnumerable<AssociationExpenses> ee, int year, int month)
        {

            return ee.Any(e => e.Id_Expense == expense.Id && e.Month == month && e.Year == year && !e.WasDisabled && e.SplitPerStairCase.HasValue && e.SplitPerStairCase.Value);
        }

        private static bool IsExpenseSelected(Expenses expense, IEnumerable<AssociationExpenses> ee, int year, int month)
        {
            return ee.Any(e => e.Id_Expense == expense.Id && e.Month == month && e.Year == year && !e.WasDisabled);
        }

        private static bool IsDplExpenseTypesSelected(AssociationExpenses expense, ExpenseType expenseType)
        {
            bool result = false;

            if (expense != null)
            {
                result = expense.Id_ExpenseType == (int)expenseType;
            }

            return result;
        }

        private void InitializeMonths(int? year, int? month)
        {
            drpExpenseMonth.Items.Clear();
            var estate = (Associations)Session[SessionConstants.SelectedAssociation];
            if (estate != null)
            {
                var yearMonths = AssociationExpensesManager.GetAllMonthsAndYearsAvailableByAssociationId(estate.Id);
                foreach (var ym in yearMonths)
                {
                    drpExpenseMonth.Items.Add(new ListItem
                    {
                        Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(ym.Month) + " " + ym.Year,
                        Value = ym.Month.ToString(),
                        Selected = (year != null && ym.Month == month.Value)
                    });
                }
            }
        }

        protected void btnStep1_Click(object sender, EventArgs e)
        {
            ConfigureStep1();
        }

        private void ConfigureStep1()
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

            int _month;
            int _year = Year().HasValue ? Year().Value : 2017;
            if (int.TryParse(drpExpenseMonth.SelectedValue, out _month))
            {
                var estate = (Associations)Session[SessionConstants.SelectedAssociation];
                if (estate != null)
                {
                    var existingAssociationExpenses = AssociationExpensesManager.GetByMonthAndYearNotDisabled(estate.Id, _year, _month);
                    var existingAssociationExpensesIncludingDisabled = AssociationExpensesManager.GetAllAssociationExpensesByMonthAndYearIncludingDisabled(estate.Id, _year, _month);

                    foreach (TableRow row in tblMonthlyExpenses.Rows)
                    {
                        if (row != null && row.Cells.Count > 2 &&
                            row.Cells[0] != null && row.Cells[0].Controls.Count > 0
                            && row.Cells[0].Controls[0] != null)
                        {
                            if (row.Cells[0].Controls[0] is CheckBox && row.Cells[2].Controls[0] is DropDownList)
                            {
                                bool? cbExpensePerStaircase = null;

                                if (row.Cells.Count > 3 && row.Cells[3].Controls[0] is CheckBox)
                                {
                                    CheckBox cbStairCase = (CheckBox)row.Cells[3].Controls[0];
                                    cbExpensePerStaircase = cbStairCase.Checked;
                                }

                                CheckBox cbExpenseSelect = (CheckBox)row.Cells[0].Controls[0];
                                DropDownList dpExpenseType = (DropDownList)row.Cells[2].Controls[0];
                                int expenseId = 0;
                                if (int.TryParse(cbExpenseSelect.ID.Replace("esateExpense", ""), out expenseId))
                                {
                                    bool existingAssociationExpensesContainsItem = existingAssociationExpenses.Select(esex => esex.Id_Expense).Contains(expenseId);
                                    bool existingAssociationExpensesContainsItemAsDisabled = existingAssociationExpensesIncludingDisabled
                                        .Where(es => es.WasDisabled).Select(esex => esex.Id_Expense).Contains(expenseId);
                                    AssociationExpenses ee = null;

                                    // if selected and non existing in the prev. config
                                    if (cbExpenseSelect.Checked && existingAssociationExpensesContainsItemAsDisabled)
                                    {
                                        // enables it
                                        ee = AssociationExpensesManager.GetAssociationExpensesByMonthAndYearAndDisabled(estate.Id, expenseId, _year, _month);
                                        AssociationExpensesManager.MarkAssociationExpensesDisableProperty(ee, false, cbExpensePerStaircase);
                                    }
                                    else if (!cbExpenseSelect.Checked && existingAssociationExpensesContainsItem)
                                    {
                                        // disables it
                                        ee = AssociationExpensesManager.GetAssociationExpensesByMonthAndYearAndDisabled(estate.Id, expenseId, _year, _month, false);
                                        AssociationExpensesManager.MarkAssociationExpensesDisableProperty(ee, true, cbExpensePerStaircase);
                                    }
                                    else if (cbExpenseSelect.Checked && !existingAssociationExpensesContainsItem)
                                    {
                                        ee = AssociationExpensesManager.GetAssociationExpensesByMonthAndYearAndDisabled(expenseId, estate.Id, _year, _month);
                                        if (ee != null)
                                        {
                                            // disables it
                                            AssociationExpensesManager.MarkAssociationExpensesDisableProperty(ee, true, cbExpensePerStaircase);
                                        }
                                        else
                                        {
                                            // adds it
                                            AssociationExpensesManager.Add(estate.Id, expenseId, _month, _year, dpExpenseType.SelectedValue, false);
                                        }
                                    }

                                    ExpenseType selectedExpenseType;
                                    if (Enum.TryParse<ExpenseType>(dpExpenseType.SelectedValue, out  selectedExpenseType))
                                    {
                                        if (ee == null)
                                        {
                                            ee = AssociationExpensesManager.GetAssociationExpense(estate.Id, expenseId, _year, _month);
                                        }
                                        if (ee == null) { continue; }

                                        if ((ExpenseType)ee.ExpenseTypes.Id != selectedExpenseType)
                                        {
                                            AssociationExpensesManager.UpdateAssociationExpenseType(ee, selectedExpenseType);
                                        }

                                        if (!ee.SplitPerStairCase.HasValue || ee.SplitPerStairCase.Value != cbExpensePerStaircase)
                                        {
                                            AssociationExpensesManager.MarkAssociationExpensesDisableProperty(ee, !cbExpenseSelect.Checked, cbExpensePerStaircase);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (Year() != null && Month() != null && step33.Visible)
            {
                Response.Redirect("~/Expenses/Invoices.aspx?year=" + Year() + "&month=" + Month());
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
            Response.Redirect("~/Config/MonthlyExpenses.aspx");

        }
    }
}