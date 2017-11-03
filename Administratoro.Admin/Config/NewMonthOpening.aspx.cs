
namespace Admin.Config
{
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

    public partial class NewMonthOpening : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var estate = (Estates)Session[SessionConstants.SelectedEstate];
            var defaultEE = EstateExpensesManager.GetFromLastesOpenedMonth(estate.Id);
            InitializeYearsAndMonths(defaultEE);
            InitializeExpenses();
        }

        private void InitializeMonths(int year, int month)
        {
            drpOpeningMonth.Items.Clear();

            drpOpeningMonth.Items.Add(new ListItem { Value = "1", Text = "Ianuarie", Selected = IsMonthSelected(1, month), Enabled = isMonthEnabled(1, year, month) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "2", Text = "Februarie", Selected = IsMonthSelected(2, month), Enabled = isMonthEnabled(2, year, month) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "3", Text = "Martie", Selected = IsMonthSelected(3, month), Enabled = isMonthEnabled(3, year, month) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "4", Text = "Aprilie", Selected = IsMonthSelected(4, month), Enabled = isMonthEnabled(4, year, month) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "5", Text = "Mai", Selected = IsMonthSelected(5, month), Enabled = isMonthEnabled(5, year, month) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "6", Text = "Iunie", Selected = IsMonthSelected(6, month), Enabled = isMonthEnabled(6, year, month) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "7", Text = "Iulie", Selected = IsMonthSelected(7, month), Enabled = isMonthEnabled(7, year, month) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "8", Text = "August", Selected = IsMonthSelected(8, month), Enabled = isMonthEnabled(8, year, month) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "9", Text = "Septembrie", Selected = IsMonthSelected(9, month), Enabled = isMonthEnabled(9, year, month) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "10", Text = "Octombrie", Selected = IsMonthSelected(10, month), Enabled = isMonthEnabled(10, year, month) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "11", Text = "Noiembrie", Selected = IsMonthSelected(11, month), Enabled = isMonthEnabled(11, year, month) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "12", Text = "Decembrie", Selected = IsMonthSelected(12, month), Enabled = isMonthEnabled(12, year, month) });
        }

        private bool isMonthEnabled(int month, int lastYear, int lastMonth)
        {
            bool result = false;
            int selectedYear = drpOpeningYear.SelectedValue.ToNullableInt().Value;

            if (selectedYear > lastYear)
            {
                result = true;
            }
            else if (selectedYear == lastYear && month > lastMonth)
            {
                result = true;
            }

            return result;
        }

        private bool IsMonthSelected(int monthNR, int lastMonth)
        {
            bool result = false;

            if (lastMonth == 12 && monthNR == 1)
            {
                result = true;
            }
            else if (monthNR == lastMonth + 1)
            {
                result = true;
            }

            return result;
        }

        private void InitializeYearsAndMonths(List<EstateExpenses> defaultEE)
        {
            var ee = defaultEE.FirstOrDefault();
            int year = 2010;
            int month = 0;
            if (ee != null)
            {
                year = ee.Year;
                month = ee.Month;
            }

            if (drpOpeningYear.Items.Count == 0)
            {
                drpOpeningYear.Items.Add(new ListItem { Value = "2017", Text = "2017", Enabled = (year <= 2017) });
                drpOpeningYear.Items.Add(new ListItem { Value = "2018", Text = "2018", Enabled = (year <= 2018) });
                drpOpeningYear.Items.Add(new ListItem { Value = "2019", Text = "2019", Enabled = (year <= 2019) });
                drpOpeningYear.AutoPostBack = true;
                InitializeMonths(year, month);
            }
            drpOpeningYear.SelectedIndexChanged += drpOpeningYear_SelectedIndexChanged;

        }

        private void drpOpeningYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedYear = drpOpeningYear.SelectedValue.ToNullableInt().Value;
            int selectedMonth = drpOpeningYear.SelectedValue.ToNullableInt().Value;
            var estate = (Estates)Session[SessionConstants.SelectedEstate];
            if (estate != null)
            {
                var defaultEE = EstateExpensesManager.GetFromLastesOpenedMonth(estate.Id);
                InitializeYearsAndMonths(defaultEE);
            }
        }

        private void InitializeExpenses(int? selectedYear = null, int? selectedMonth = null)
        {
            tblMonthlyExpenses.Rows.Clear();

            int month = 0;
            int year = 0;
            var i = 1;
            if (int.TryParse(drpOpeningMonth.SelectedValue, out month) && int.TryParse(drpOpeningYear.SelectedValue, out year))
            {
                var estate = (Estates)Session[SessionConstants.SelectedEstate];
                if (estate != null)
                {
                    var defaultEE = EstateExpensesManager.GetFromLastesOpenedMonth(estate.Id);
                    var eeAlsoDisabled = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearIncludingDisabled(estate.Id, defaultEE.FirstOrDefault().Year, defaultEE.FirstOrDefault().Month);

                    var expenses = ExpensesManager.GetAllExpensesAsList();

                    foreach (var item in expenses)
                    {
                        TableRow row = new TableRow();

                        // add expense exists
                        TableCell expenseExists = new TableCell();
                        CheckBox esexExists = new CheckBox();
                        esexExists.AutoPostBack = false;
                        esexExists.ID = String.Format("expense{0}", item.Id);
                        esexExists.Checked = isExpenseSelected(item, defaultEE);
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
                        EstateExpenses esex = eeAlsoDisabled.FirstOrDefault(s => s.Id_Expense == item.Id);

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

        private static bool isDplExpenseTypesSelected(Administratoro.DAL.EstateExpenses expense, ExpenseType expenseType)
        {
            bool result = false;

            if (expense != null)
            {
                result = expense.Id_ExpenseType == (int)expenseType;
            }

            return result;
        }

        private static bool isExpenseSelected(Administratoro.DAL.Expenses expense, IEnumerable<EstateExpenses> ee)
        {
            bool result = false;
            if (ee.Where(e => e.Id_Expense == expense.Id).Any())
            {
                result = true;
            }

            return result;
        }

        protected void btnOpening_Click(object sender, EventArgs e)
        {
            lblMessage.Attributes.Add("style", "");
            int year = drpOpeningYear.SelectedValue.ToNullableInt().Value;
            int month = drpOpeningMonth.SelectedValue.ToNullableInt().Value;

            var estate = (Estates)Session[SessionConstants.SelectedEstate];

            var ee = EstateExpensesManager.GetAllEstateExpensesByMonthAndYearIncludingDisabled(estate.Id, year, month);

            if (ee.Count != 0)
            {
                lblMessage.Text = "Luna deschisa deja, selecteaza alta luna-an";
                lblMessage.Attributes.Add("style", "color: red");
                return;
            }

            foreach (TableRow row in tblMonthlyExpenses.Rows)
            {
                TableCell cell0 = row.Cells[0];
                TableCell cell2 = row.Cells[2];
                if (cell0.Controls.Count == 1 && cell0.Controls[0] is CheckBox
                    && cell2.Controls.Count == 1 && cell2.Controls[0] is DropDownList)
                {
                    CheckBox cb = (CheckBox)cell0.Controls[0];
                    DropDownList dropDown = (DropDownList)cell2.Controls[0];

                    if (cb.Checked)
                    {
                        string cbId = cb.ID.Replace("expense", "");
                        int expenseId;
                        if (int.TryParse(cbId, out expenseId))
                        {
                            //dpExpenseType.SelectedValue
                            EstateExpenses newEe = EstateExpensesManager.AddEstateExpensesByTenantAndMonth(estate.Id, expenseId, month, year, dropDown.SelectedValue);
                            CashBookManager.AddDefault(newEe.Id);
                        }
                    }
                }
            }
        }

    }
}