
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
    using Administrataro.BL.Models;

    public partial class NewMonthOpening : BasePage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
             
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var defaultEE = AssociationExpensesManager.GetFromLastesOpenedMonth(Association.Id);
            if (!Page.IsPostBack)
            {
                InitializeYearsAndMonths(defaultEE);
            }
            InitializeExpenses();
        }

        private void InitializeMonths(int year, int month)
        {
            var availableYearMonths = AssociationExpensesManager.GetAllMonthsAndYearsAvailableByAssociationId(Association.Id);
            drpOpeningMonth.Items.Clear();

            drpOpeningMonth.Items.Add(new ListItem { Value = "1", Text = "Ianuarie", Selected = IsMonthSelected(1, month), Enabled = isMonthEnabled(1, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "2", Text = "Februarie", Selected = IsMonthSelected(2, month), Enabled = isMonthEnabled(2, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "3", Text = "Martie", Selected = IsMonthSelected(3, month), Enabled = isMonthEnabled(3,  availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "4", Text = "Aprilie", Selected = IsMonthSelected(4, month), Enabled = isMonthEnabled(4, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "5", Text = "Mai", Selected = IsMonthSelected(5, month), Enabled = isMonthEnabled(5, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "6", Text = "Iunie", Selected = IsMonthSelected(6, month), Enabled = isMonthEnabled(6, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "7", Text = "Iulie", Selected = IsMonthSelected(7, month), Enabled = isMonthEnabled(7,  availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "8", Text = "August", Selected = IsMonthSelected(8, month), Enabled = isMonthEnabled(8, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "9", Text = "Septembrie", Selected = IsMonthSelected(9, month), Enabled = isMonthEnabled(9,  availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "10", Text = "Octombrie", Selected = IsMonthSelected(10, month), Enabled = isMonthEnabled(10, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "11", Text = "Noiembrie", Selected = IsMonthSelected(11, month), Enabled = isMonthEnabled(11, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "12", Text = "Decembrie", Selected = IsMonthSelected(12, month), Enabled = isMonthEnabled(12, availableYearMonths) });
        }

        private bool isMonthEnabled(int month, List<YearMonth> availableYearMonths)
        {
            bool result = false;
            int selectedYear = drpOpeningYear.SelectedValue.ToNullableInt().Value;
            if (!availableYearMonths.Any(ee => ee.Month == month && ee.Year == selectedYear))
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

        private void InitializeYearsAndMonths(List<AssociationExpenses> defaultEE)
        {
            var ee = defaultEE.LastOrDefault();
            int year = 2010;
            int month = 0;
            if (ee != null)
            {
                year = ee.Year;
                month = ee.Month;
            }

            if (drpOpeningYear.Items.Count == 0)
            {
                drpOpeningYear.Items.Add(new ListItem { Value = "2017", Text = "2017", Selected = year == 2017  });
                drpOpeningYear.Items.Add(new ListItem { Value = "2018", Text = "2018", Selected = year == 2018 });
                drpOpeningYear.Items.Add(new ListItem { Value = "2019", Text = "2019", Selected = year == 2019 });
                drpOpeningYear.AutoPostBack = true;
            }

            InitializeMonths(year, month);
            drpOpeningYear.SelectedIndexChanged += drpOpeningYear_SelectedIndexChanged;

        }

        private void drpOpeningYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            var defaultEEs = AssociationExpensesManager.GetFromLastesOpenedMonth(Association.Id);
            InitializeYearsAndMonths(defaultEEs);
        }

        private void InitializeExpenses(int? selectedYear = null, int? selectedMonth = null)
        {
            tblMonthlyExpenses.Rows.Clear();

            int month = 0;
            int year = 0;
            if (int.TryParse(drpOpeningMonth.SelectedValue, out month) && int.TryParse(drpOpeningYear.SelectedValue, out year))
            {
                var estate = (Associations)Session[SessionConstants.SelectedAssociation];
                if (estate != null)
                {
                    int defaultYear = 2017;
                    int defaultMonth = 1;

                    var defaultEE = AssociationExpensesManager.GetFromLastesOpenedMonth(estate.Id);
                    if (defaultEE.Count > 0)
                    {
                        defaultYear = defaultEE.FirstOrDefault().Year;
                        defaultMonth = defaultEE.FirstOrDefault().Month;
                    }
                    var eeAlsoDisabled = AssociationExpensesManager.GetAllAssociationExpensesByMonthAndYearIncludingDisabled(estate.Id, defaultYear, defaultMonth);

                    var expenses = ExpensesManager.GetAllExpenses();
                    var ee = AssociationExpensesManager.GetAllAssociationsByMonthAndYearNotDisabled(estate.Id, defaultYear, defaultMonth);

                    TableRow defaultRow = new TableRow();

                    // add expense exists
                    TableCell defaultExpenseSelected = new TableCell
                    {
                        Text = "Activează pentru noua lună"
                    };
                    defaultRow.Cells.Add(defaultExpenseSelected);

                    // add expense name
                    TableCell defaultExpenseName = new TableCell
                    {
                        Text = "Cheltuială"
                    };
                    defaultRow.Cells.Add(defaultExpenseName);

                    // add expense type
                    TableCell defaultExpenseType = new TableCell()
                    {
                        Text = "Tip calcul cheltuială"
                    };
                    defaultRow.Cells.Add(defaultExpenseType);

                    if (estate.HasStaircase)
                    {
                        TableCell tcStairCaseDefaule = new TableCell()
                            {
                                Text = "Contor individual per scară"
                            };

                        defaultRow.Cells.Add(tcStairCaseDefaule);
                    }

                    tblMonthlyExpenses.Rows.Add(defaultRow);

                    foreach (var expense in expenses)
                    {
                        TableRow row = new TableRow();

                        // add expense exists
                        TableCell expenseExists = new TableCell();
                        CheckBox esexExists = new CheckBox();
                        esexExists.AutoPostBack = false;
                        esexExists.ID = String.Format("expense{0}", expense.Id);
                        esexExists.Checked = isExpenseSelected(expense, defaultEE);
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
                        DropDownList dp = new DropDownList();
                        if (expense.Id != (int)Expense.AjutorÎncălzire)
                        {
                            AssociationExpenses esex = eeAlsoDisabled.FirstOrDefault(s => s.Id_Expense == expense.Id);

                            var selected1 = isDplExpenseTypesSelected(esex, ExpenseType.PerIndex, expense.LegalType);
                            dp.Items.Add(new ListItem
                            {
                                Value = ((int)ExpenseType.PerIndex).ToString(),
                                Text = "Individuală prin indecși",
                                Selected = selected1
                            });

                            var selected2 = isDplExpenseTypesSelected(esex, ExpenseType.PerCotaIndiviza, expense.LegalType);
                            dp.Items.Add(new ListItem
                            {
                                Value = ((int)ExpenseType.PerCotaIndiviza).ToString(),
                                Text = "Cotă indiviză de proprietate",
                                Selected = selected2
                            });

                            var selected3 = isDplExpenseTypesSelected(esex, ExpenseType.PerApartments, expense.LegalType);
                            dp.Items.Add(new ListItem
                            {
                                Value = ((int)ExpenseType.PerApartments).ToString(),
                                Text = "Per număr locatari imobil",
                                Selected = selected3
                            });
                        }
                        else
                        {
                            dp.Items.Add(new ListItem
                            {
                                Value = ((int)ExpenseType.Individual).ToString(),
                                Text = "Individual",
                                Selected = expense.LegalType == (int)ExpenseType.Individual
                            });
                        }

                        expenseType.Controls.Add(dp);
                        row.Cells.Add(expenseType);

                        if (estate.HasStaircase)
                        {
                            TableCell tcStairCase = new TableCell();
                            CheckBox stairCaseSplit = new CheckBox();
                            stairCaseSplit.AutoPostBack = false;
                            stairCaseSplit.Checked = isStairCaseSplitSelected(expense, ee, defaultYear, defaultMonth);
                            tcStairCase.Controls.Add(stairCaseSplit);
                            row.Cells.Add(tcStairCase);
                        }

                        tblMonthlyExpenses.Rows.Add(row);
                    }
                }
            }
        }

        private bool isStairCaseSplitSelected(Expenses expense, List<AssociationExpenses> ee, int year, int month)
        {
            bool result = false;
            if (ee.Where(e => e.Id_Expense == expense.Id && e.Month == month && e.Year == year &&
                !e.WasDisabled && e.SplitPerStairCase.HasValue && e.SplitPerStairCase.Value).Any())
            {
                result = true;
            }

            return result;
        }

        private static bool isDplExpenseTypesSelected(Administratoro.DAL.AssociationExpenses associationExpense, ExpenseType expenseType, int? expenseLegalType)
        {
            bool result = false;

            if (associationExpense != null)
            {
                result = associationExpense.Id_ExpenseType == (int)expenseType;
            }
            else
            {
                if (expenseLegalType.HasValue)
                {
                    result = (int)expenseType == expenseLegalType.Value;
                }
            }

            return result;
        }

        private static bool isExpenseSelected(Administratoro.DAL.Expenses expense, IEnumerable<AssociationExpenses> ee)
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

            var ee = AssociationExpensesManager.GetAllAssociationExpensesByMonthAndYearIncludingDisabled(Association.Id, year, month);

            if (ee.Count != 0)
            {
                lblMessage.Text = "Luna deschisa deja, selecteaza alta luna-an";
                lblMessage.Attributes.Add("style", "color: red");
                return;
            }
            List<AssociationExpenses> oldEE = AssociationExpensesManager.GetFromLastesOpenedMonth(Association.Id);

            foreach (TableRow row in tblMonthlyExpenses.Rows)
            {
                if (row.Cells.Count > 3)
                {
                    TableCell cellIsSelected = row.Cells[0];
                    TableCell cellExpenseType = row.Cells[2];
                    TableCell cellIsStairCaseSplit = row.Cells[3];

                    if (cellIsSelected.Controls.Count == 1 && cellIsSelected.Controls[0] is CheckBox
                        && cellExpenseType.Controls.Count == 1 && cellExpenseType.Controls[0] is DropDownList)
                    {
                        CheckBox cbIsSelected = (CheckBox)cellIsSelected.Controls[0];
                        DropDownList drpExpenseType = (DropDownList)cellExpenseType.Controls[0];
                        CheckBox cbIsStairCaseSplitSelected = (CheckBox)cellIsStairCaseSplit.Controls[0];

                        if (cbIsSelected.Checked)
                        {
                            string cbId = cbIsSelected.ID.Replace("expense", "");
                            int expenseId;
                            if (int.TryParse(cbId, out expenseId))
                            {
                                AssociationExpenses newEe = AssociationExpensesManager.Add(Association.Id, expenseId, month, year, drpExpenseType.SelectedValue, cbIsStairCaseSplitSelected.Checked);
                                AssociationExpensesManager.UpdatePricePerUnitDefaultPrevieousMonth(newEe, oldEE);
                            }
                        }
                    }
                }
            }

            Response.Redirect("~/Expenses/Invoices.aspx?year=" + year + "&month=" + month);
        }
    }
}