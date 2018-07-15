
namespace Admin.Config
{
    using Administratoro.BL.Models;
    using Administratoro.BL.Constants;
    using Administratoro.BL.Extensions;
    using Administratoro.BL.Managers;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI.WebControls;

    public partial class NewMonthOpening : BasePage
    {
        protected void Page_Init(object sender, EventArgs e)
        {

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var defaultEe = AssociationExpensesManager.GetFromLastestOpenedMonth(Association.Id);
            if (!Page.IsPostBack)
            {
                InitializeYearsAndMonths(defaultEe);
            }
            InitializeExpenses();
        }

        private void InitializeMonths(int month)
        {
            var availableYearMonths = AssociationExpensesManager.GetAllMonthsAndYearsAvailableByAssociationId(Association.Id);
            drpOpeningMonth.Items.Clear();

            drpOpeningMonth.Items.Add(new ListItem { Value = "1", Text = "Ianuarie", Selected = IsMonthSelected(1, month), Enabled = IsMonthEnabled(1, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "2", Text = "Februarie", Selected = IsMonthSelected(2, month), Enabled = IsMonthEnabled(2, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "3", Text = "Martie", Selected = IsMonthSelected(3, month), Enabled = IsMonthEnabled(3, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "4", Text = "Aprilie", Selected = IsMonthSelected(4, month), Enabled = IsMonthEnabled(4, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "5", Text = "Mai", Selected = IsMonthSelected(5, month), Enabled = IsMonthEnabled(5, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "6", Text = "Iunie", Selected = IsMonthSelected(6, month), Enabled = IsMonthEnabled(6, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "7", Text = "Iulie", Selected = IsMonthSelected(7, month), Enabled = IsMonthEnabled(7, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "8", Text = "August", Selected = IsMonthSelected(8, month), Enabled = IsMonthEnabled(8, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "9", Text = "Septembrie", Selected = IsMonthSelected(9, month), Enabled = IsMonthEnabled(9, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "10", Text = "Octombrie", Selected = IsMonthSelected(10, month), Enabled = IsMonthEnabled(10, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "11", Text = "Noiembrie", Selected = IsMonthSelected(11, month), Enabled = IsMonthEnabled(11, availableYearMonths) });
            drpOpeningMonth.Items.Add(new ListItem { Value = "12", Text = "Decembrie", Selected = IsMonthSelected(12, month), Enabled = IsMonthEnabled(12, availableYearMonths) });
        }

        private bool IsMonthEnabled(int month, List<YearMonth> availableYearMonths)
        {
            bool result = false;
            int selectedYear = drpOpeningYear.SelectedValue.ToNullableInt().Value;
            if (!availableYearMonths.Any(ee => ee.Month == month && ee.Year == selectedYear))
            {
                result = true;
            }

            return result;
        }

        private static bool IsMonthSelected(int monthNr, int lastMonth)
        {
            bool result = false;

            if (lastMonth == 12 && monthNr == 1)
            {
                result = true;
            }
            else if (monthNr == lastMonth + 1)
            {
                result = true;
            }

            return result;
        }

        private void InitializeYearsAndMonths(IEnumerable<AssociationExpenses> defaultEe)
        {
            var ee = defaultEe.LastOrDefault();
            int year = 2010;
            int month = 0;
            if (ee != null)
            {
                year = ee.Year;
                month = ee.Month;
            }

            if (drpOpeningYear.Items.Count == 0)
            {
                drpOpeningYear.Items.Add(new ListItem { Value = "2017", Text = "2017", Selected = year == 2017 });
                drpOpeningYear.Items.Add(new ListItem { Value = "2018", Text = "2018", Selected = year == 2018 });
                drpOpeningYear.Items.Add(new ListItem { Value = "2019", Text = "2019", Selected = year == 2019 });
                drpOpeningYear.AutoPostBack = true;
            }

            InitializeMonths(month);
            drpOpeningYear.SelectedIndexChanged += drpOpeningYear_SelectedIndexChanged;

        }

        private void drpOpeningYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            var defaultEEs = AssociationExpensesManager.GetFromLastestOpenedMonth(Association.Id);
            InitializeYearsAndMonths(defaultEEs);
        }

        private void InitializeExpenses()
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

                    var defaultEe = AssociationExpensesManager.GetFromLastestOpenedMonth(estate.Id);
                    if (defaultEe.Any())
                    {
                        defaultYear = defaultEe.FirstOrDefault().Year;
                        defaultMonth = defaultEe.FirstOrDefault().Month;
                    }
                    var eeAlsoDisabled = AssociationExpensesManager.GetAllAssociationExpensesByMonthAndYearIncludingDisabled(estate.Id, defaultYear, defaultMonth);

                    IEnumerable<Expenses> expenses = ExpensesManager.GetAllExpenses();
                    var ee = AssociationExpensesManager.GetByMonthAndYearNotDisabled(estate.Id, defaultYear, defaultMonth);

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
                        esexExists.Checked = IsExpenseSelected(expense, defaultEe);
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

                            var selected1 = IsDplExpenseTypesSelected(esex, ExpenseType.PerIndex, expense.LegalType);
                            dp.Items.Add(new ListItem
                            {
                                Value = ((int)ExpenseType.PerIndex).ToString(),
                                Text = "Individuală prin indecși",
                                Selected = selected1
                            });

                            var selected2 = IsDplExpenseTypesSelected(esex, ExpenseType.PerCotaIndiviza, expense.LegalType);
                            dp.Items.Add(new ListItem
                            {
                                Value = ((int)ExpenseType.PerCotaIndiviza).ToString(),
                                Text = "Cotă indiviză de proprietate",
                                Selected = selected2
                            });

                            var selected3 = IsDplExpenseTypesSelected(esex, ExpenseType.PerNrTenants, expense.LegalType);
                            dp.Items.Add(new ListItem
                            {
                                Value = ((int)ExpenseType.PerNrTenants).ToString(),
                                Text = "Per număr persoane imobil",
                                Selected = selected3
                            });

                            var selected4 = IsDplExpenseTypesSelected(esex, ExpenseType.PerApartament, expense.LegalType);
                            dp.Items.Add(new ListItem
                            {
                                Value = ((int)ExpenseType.PerApartament).ToString(),
                                Text = ExpenseType.PerApartament.ToDescription(),
                                Selected = selected4
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
                            stairCaseSplit.Checked = IsStairCaseSplitSelected(expense, ee, defaultYear, defaultMonth);
                            tcStairCase.Controls.Add(stairCaseSplit);
                            row.Cells.Add(tcStairCase);
                        }

                        tblMonthlyExpenses.Rows.Add(row);
                    }
                }
            }
        }

        private static bool IsStairCaseSplitSelected(Expenses expense, IEnumerable<AssociationExpenses> ee, int year, int month)
        {
            return ee.Any(e => e.Id_Expense == expense.Id && e.Month == month && e.Year == year &&
                !e.WasDisabled && e.SplitPerStairCase.HasValue && e.SplitPerStairCase.Value);
        }

        private static bool IsDplExpenseTypesSelected(Administratoro.DAL.AssociationExpenses associationExpense, ExpenseType expenseType, int? expenseLegalType)
        {
            var result = false;

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

        private static bool IsExpenseSelected(Expenses expense, IEnumerable<AssociationExpenses> ee)
        {
            return ee.Any(e => e.Id_Expense == expense.Id);
        }

        protected void btnOpening_Click(object sender, EventArgs e)
        {
            lblMessage.Attributes.Add("style", "");
            var year = drpOpeningYear.SelectedValue.ToNullableInt().Value;
            var month = drpOpeningMonth.SelectedValue.ToNullableInt().Value;

            var ee = AssociationExpensesManager.GetAllAssociationExpensesByMonthAndYearIncludingDisabled(Association.Id, year, month);

            if (ee.Count() != 0)
            {
                lblMessage.Text = "Luna deschisa deja, selecteaza alta luna-an";
                lblMessage.Attributes.Add("style", "color: red");
                return;
            }
            IEnumerable<AssociationExpenses> oldEe = AssociationExpensesManager.GetFromLastestOpenedMonth(Association.Id);

            foreach (TableRow row in tblMonthlyExpenses.Rows)
            {
                if (row.Cells.Count > 2)
                {
                    TableCell cellIsSelected = row.Cells[0];
                    TableCell cellExpenseType = row.Cells[2];

                    if (cellIsSelected.Controls.Count == 1 && cellIsSelected.Controls[0] is CheckBox
                        && cellExpenseType.Controls.Count == 1 && cellExpenseType.Controls[0] is DropDownList)
                    {
                        CheckBox cbIsSelected = (CheckBox)cellIsSelected.Controls[0];
                        DropDownList drpExpenseType = (DropDownList)cellExpenseType.Controls[0];
                        bool cbIsStairCaseSplitSelected = false;

                        if (row.Cells.Count > 3)
                        {
                            TableCell cellIsStairCaseSplit = row.Cells[3];
                            cbIsStairCaseSplitSelected = ((CheckBox)cellIsStairCaseSplit.Controls[0]).Checked;
                        }

                        if (cbIsSelected.Checked)
                        {
                            string cbId = cbIsSelected.ID.Replace("expense", "");
                            int expenseId;
                            if (int.TryParse(cbId, out expenseId))
                            {
                                AssociationExpenses newEe = AssociationExpensesManager.Add(Association.Id, expenseId,
                                    month, year, drpExpenseType.SelectedValue, cbIsStairCaseSplitSelected);
                                AssociationExpensesManager.UpdatePricePerUnitDefaultPreviousMonth(newEe, oldEe);
                            }
                        }
                    }
                }
            }

            Response.Redirect("~/Expenses/Invoices.aspx?year=" + year + "&month=" + month);
        }
    }
}