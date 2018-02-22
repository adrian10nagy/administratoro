
namespace Admin.Config
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Managers;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public partial class MonthlyExpenses : System.Web.UI.Page
    {
        private int? year()
        {
            var yearId = Request.QueryString["year"];
            int year;
            if (!int.TryParse(yearId, out year))
            {
                return null;
            }

            return year;
        }

        private int? month()
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
                InitializeMonths(year(), month());
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

            if (year() != null && month() != null && step11.Visible)
            {
                ConfigureStep1();
            }
        }

        private void InitializeExpenses()
        {
            //tblMonthlyExpenses.Rows.Clear();
            int month;
            int _year = year().HasValue ? year().Value : 2017;
            if (int.TryParse(drpExpenseMonth.SelectedValue, out month))
            {
                var estate = (Estates)Session[SessionConstants.SelectedAssociation];
                if (estate != null)
                {
                    var ee = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearNotDisabled(estate.Id, _year, month);
                    var eeAlsoDisabled = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearIncludingDisabled(estate.Id, _year, month);

                    var expenses = ExpensesManager.GetAllExpenses();

                    foreach (var item in expenses)
                    {
                        TableRow row = new TableRow();

                        // add expense that were added
                        TableCell expenseExists = new TableCell();
                        CheckBox esexExists = new CheckBox();
                        esexExists.AutoPostBack = false;
                        esexExists.ID = String.Format("esateExpense{0}", item.Id);
                        esexExists.Checked = isExpenseSelected(item, ee, _year, month);
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
                        EstateExpenses esex = eeAlsoDisabled.FirstOrDefault(s => s.Id_Expense == item.Id && s.Month == month && s.Year == _year && s.Id_Estate == estate.Id);

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

                        if (estate.HasStaircase)
                        {
                            TableCell tcStairCase = new TableCell();
                            CheckBox stairCaseSplit = new CheckBox();
                            stairCaseSplit.AutoPostBack = false;
                            stairCaseSplit.Checked = isStairCaseSplitSelected(item, ee, _year, month);
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

        private bool isStairCaseSplitSelected(Expenses expense, List<EstateExpenses> ee, int year, int month)
        {

            bool result = false;
            if (ee.Where(e => e.Id_Expense == expense.Id && e.Month == month && e.Year == year && !e.WasDisabled && e.SplitPerStairCase.HasValue && e.SplitPerStairCase.Value).Any())
            {
                result = true;
            }

            return result;
        }

        private static bool isExpenseSelected(Administratoro.DAL.Expenses expense, List<EstateExpenses> ee, int year, int month)
        {
            bool result = false;
            if (ee.Where(e => e.Id_Expense == expense.Id && e.Month == month && e.Year == year && !e.WasDisabled).Any())
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

        private void InitializeMonths(int? year, int? month)
        {
            drpExpenseMonth.Items.Clear();
            var estate = (Estates)Session[SessionConstants.SelectedAssociation];
            if (estate != null)
            {
                var yearMonths = EstateExpensesManager.GetAllMonthsAndYearsAvailableByEstateId(estate.Id);
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

            int _month = 0;
            int _year = year().HasValue?year().Value:2017;
            if (int.TryParse(drpExpenseMonth.SelectedValue, out _month))
            {
                var estate = (Estates)Session[SessionConstants.SelectedAssociation];
                if (estate != null)
                {
                    var existingEstateExpenses = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearNotDisabled(estate.Id, _year, _month);
                    var existingEstateExpensesIncludingDisabled = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearIncludingDisabled(estate.Id, _year, _month);

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
                                    bool existingEstateExpensesContainsItem = existingEstateExpenses.Select(esex => esex.Id_Expense).Contains(expenseId);
                                    bool existingEstateExpensesContainsItemAsDisabled = existingEstateExpensesIncludingDisabled
                                        .Where(es => es.WasDisabled).Select(esex => esex.Id_Expense).Contains(expenseId);
                                    EstateExpenses ee = null;

                                    // if selected and non existing in the prev. config
                                    if (cbExpenseSelect.Checked && existingEstateExpensesContainsItemAsDisabled)
                                    {
                                        // enables it
                                        ee = EstateExpensesManager.GetEstateExpensesByMonthAndYearAndDisabled(estate.Id, expenseId, _year, _month);
                                        EstateExpensesManager.MarkEstateExpensesDisableProperty(ee, false, cbExpensePerStaircase);
                                    }
                                    else if (!cbExpenseSelect.Checked && existingEstateExpensesContainsItem)
                                    {
                                        // disables it
                                        ee = EstateExpensesManager.GetEstateExpensesByMonthAndYearAndDisabled(estate.Id, expenseId, _year, _month, false);
                                        EstateExpensesManager.MarkEstateExpensesDisableProperty(ee, true, cbExpensePerStaircase);
                                    }
                                    else if (cbExpenseSelect.Checked && !existingEstateExpensesContainsItem)
                                    {
                                        ee = EstateExpensesManager.GetEstateExpensesByMonthAndYearAndDisabled(expenseId, estate.Id, _year, _month);
                                        if (ee != null)
                                        {
                                            // disables it
                                            EstateExpensesManager.MarkEstateExpensesDisableProperty(ee, true, cbExpensePerStaircase);
                                        }
                                        else
                                        {
                                            // adds it
                                            EstateExpensesManager.Add(estate.Id, expenseId, _month, _year, dpExpenseType.SelectedValue, false);
                                        }
                                    }

                                    ExpenseType selectedExpenseType;
                                    if (Enum.TryParse<ExpenseType>(dpExpenseType.SelectedValue, out  selectedExpenseType))
                                    {
                                        if (ee == null)
                                        {
                                            ee = EstateExpensesManager.GetEstateExpenses(estate.Id, expenseId, _year, _month);
                                        }

                                        if (ee != null && (ExpenseType)ee.ExpenseTypes.Id != selectedExpenseType)
                                        {
                                            EstateExpensesManager.UpdateEstateExpenseType(ee, selectedExpenseType);
                                        }

                                        if (ee != null && (!ee.SplitPerStairCase.HasValue || ee.SplitPerStairCase.Value != cbExpensePerStaircase))
                                        {
                                            EstateExpensesManager.MarkEstateExpensesDisableProperty(ee, false, cbExpensePerStaircase);
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
            Response.Redirect("~/Config/MonthlyExpenses.aspx");

        }
    }
}