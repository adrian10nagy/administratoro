using Administratoro.BL.Managers;
using Administratoro.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Administratoro.BL.Constants;
using Administratoro.BL.Extensions;

namespace Admin.Associations
{
    public partial class New : BasePage
    {
        #region variables

        private int _step
        {
            get
            {
                if (ViewState["step"] == null)
                    return 1;
                else
                    return (int)ViewState["step"];
            }
            set { ViewState["step"] = value; }
        }

        public List<DymanicCounter> DynamicCounters
        {
            get
            {
                return (List<DymanicCounter>)Session["DynamicCounters"];
            }
            set
            {
                Session["DynamicCounters"] = value;
            }
        }

        public Dictionary<string, string> DynamicStairs
        {
            get
            {
                return (Dictionary<string, string>)Session["DynamicControls"];
            }
            set
            {
                Session["DynamicControls"] = value;
            }
        }

        public int NextControl
        {
            get
            {
                return ViewState["NextControl"] == null ? 0 : (int)ViewState["NextControl"];
            }
            set
            {
                ViewState["NextControl"] = value;
            }
        }


        public int NextCounterControl
        {
            get
            {
                return ViewState["NextCounterControl"] == null ? 0 : (int)ViewState["NextCounterControl"];
            }
            set
            {
                ViewState["NextCounterControl"] = value;
            }
        }
        #endregion

        protected void Page_Init(object sender, EventArgs e)
        {
            //_step = 3;

            PlaceholderControls.Controls.Clear();
            PlaceholderControls.Controls.Add(new LiteralControl("<br />"));

            if (!IsPostBack)
            {
                AddDymanicStairs();
            }
            else
            {
                //add textbox
                Label txtValue = new Label()
                {
                    Text = "Denumire scară",
                    CssClass = "col-md-3 col-md-offset-3 col-xs-3"
                };
                PlaceholderControls.Controls.Add(txtValue);

                Label txtIndiviza = new Label()
                {
                    Text = "Indiviză scară",
                    CssClass = "col-md-3 col-md-offset-3 col-xs-3"
                };
                PlaceholderControls.Controls.Add(txtIndiviza);

                PlaceholderControls.Controls.Add(new LiteralControl("<br />"));

                for (int i = 0; i < DynamicStairs.Count; i++)
                {
                    string key;
                    string value;
                    AddStairsControl(i, out key, out value);
                }

                if (Request.Form.AllKeys.Any(key => DynamicStairs.Any(d => d.Key == key)))
                {
                    NextControl++;
                    string key;
                    string value;
                    AddStairsControl(DynamicStairs.Count, out key, out value);
                    DynamicStairs.Add(key, value);
                }

                //add button
                Button btnAddNext = new Button();
                btnAddNext.Text = "Adaugă scară nouă";
                btnAddNext.ID = "btnAddNext";
                btnAddNext.CausesValidation = false;
                btnAddNext.Visible = true;

                btnAddNext.Command += new CommandEventHandler(btnAddNext_Command);
                PlaceholderControls.Controls.Add(btnAddNext);
            }
        }

        private void AddDymanicCounters()
        {
            DynamicCounters = new List<DymanicCounter>();
            AddCounterHeader();
            for (int i = 0; i <= NextCounterControl; i++)
            {
                var control = AddCounterControl(i);
                DynamicCounters.Add(control);
            }
        }

        private void AddCounterHeader()
        {
            var association = Session[SessionConstants.SelectedAssociation] as Administratoro.DAL.Associations;
            var headerCssClass = association.HasStaircase ? "col-md-4 col-xs-4 countersHeadersNew" : "col-md-6 col-xs-6 countersHeadersNew";

            //var cssClass = Association.HasStaircase ? "col-md-4 col-xs-4" : "col-md-6 col-xs-6";

            Panel headerPanel = new Panel { CssClass = "col-md-12 col-xs-12" };
            // add expense name header
            Label expenseNameHeader = new Label
            {
                Text = "Cheltuiala",
                CssClass = headerCssClass
            };
            headerPanel.Controls.Add(expenseNameHeader);

            // add coounter header
            Label expenseCounterHeader = new Label
            {
                Text = "Serie contor",
                CssClass = headerCssClass
            };
            headerPanel.Controls.Add(expenseCounterHeader);

            if (association.HasStaircase && association.StairCases.Count != 00)
            {
                // add coounter header
                Label expenseStairHeader = new Label
                {
                    Text = "Scară",
                    CssClass = headerCssClass
                };
                headerPanel.Controls.Add(expenseStairHeader);
            }

            countersConfiguration.Controls.Add(headerPanel);
        }

        private void AddDymanicStairs()
        {
            DynamicStairs = new Dictionary<string, string>();
            string key;
            string value;
            AddStairsControl(NextControl, out key, out value);
            DynamicStairs.Add(key, value);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (_step == 2)
            {
                ConfigureStep2();
            }

            if (_step == 3)
            {
                Step2PopulateExpenses();
                ConfigureStep3();
            }

            if (associationStairs.SelectedIndex == 0)
            {
                PlaceholderControls.Visible = false;
            }
            else
            {
                PlaceholderControls.Visible = true;
            }
        }

        #region saveEvents

        protected void btnSave_Click(object sender, EventArgs e)
        {
            Partners partner = Session[SessionConstants.LoggedPartner] as Partners;
            _step = 2;
            decimal? indivizaAparmentsResult = null;

            if (associationStairs.SelectedIndex == 1)
            {
                decimal indivizaAparments;
                if (decimal.TryParse(associationCotaIndivizaApartments.Text, out indivizaAparments))
                {
                    indivizaAparmentsResult = indivizaAparments;
                }
            }

            var association = new Administratoro.DAL.Associations
            {
                Name = associationName.Value,
                Address = associationAddress.Value,
                HasStaircase = (associationStairs.SelectedIndex == 1),
                Id_Partner = partner.Id,
                FiscalCode = associationFiscalCode.Value,
                CotaIndivizaAparments = indivizaAparmentsResult
            };

            var addedAssociation = AssociationsManager.AddNew(association);
            for (int i = 0; i < DynamicStairs.Count; i++)
            {
                var stairName = FindControl(DynamicStairs.ElementAt(i).Key);
                var stairIndiviza = FindControl(DynamicStairs.ElementAt(i).Value);
                if (stairName is TextBox)
                {
                    TextBox sn = (TextBox)stairName;
                    TextBox si = (TextBox)stairIndiviza;

                    decimal indivizaValue;

                    if (!string.IsNullOrEmpty(sn.Text))
                    {
                        if (decimal.TryParse(si.Text, out indivizaValue))
                        {
                            StairCasesManager.AddNew(association, sn.Text, indivizaValue);
                        }
                        else
                        {
                            StairCasesManager.AddNew(association, sn.Text, null);
                        }

                    }
                }
            }

            addedAssociation = AssociationsManager.GetById(addedAssociation.Id);

            Session[SessionConstants.SelectedAssociation] = addedAssociation;
            var associations = AssociationsManager.GetAllAssociationsByPartner(partner.Id);
            Session[SessionConstants.AllAssociations] = associations;

            ConfigureStep2();
        }

        protected void btnSave2_Click(object sender, EventArgs e)
        {
            var association = Session[SessionConstants.SelectedAssociation] as Administratoro.DAL.Associations;
            Dictionary<int, int> dictionary = GetSelectdExpenses();

            AssociationExpensesManager.AddAssociationExpensesByApartmentAndMonth(association.Id, dictionary);
            association = AssociationsManager.GetById(association.Id);

            Session[SessionConstants.SelectedAssociation] = association;
            ConfigureStep3();

        }

        protected void btnSave3_Click(object sender, EventArgs e)
        {
            var association = Session[SessionConstants.SelectedAssociation] as Administratoro.DAL.Associations;
            List<AssociationCounters> cnts = new List<AssociationCounters>();

            foreach (var control in countersConfiguration.Controls)
            {
                if (control is Panel)
                {
                    var thePanel = (Panel)control;

                    if (thePanel.Controls.Count > 1 && thePanel.Controls[0] is DropDownList && thePanel.Controls[1] is TextBox)
                    {
                        var expenseControl = (DropDownList)thePanel.Controls[0];
                        var expenseCleaned = expenseControl.SelectedValue.Remove(expenseControl.SelectedValue.IndexOf("dummyExpense"));
                        var valueControl = (TextBox)thePanel.Controls[1];

                        int? stairIdResult = null;
                        if (thePanel.Controls.Count == 3 && thePanel.Controls[2] is DropDownList)
                        {
                            var stairCaseControl = (DropDownList)thePanel.Controls[2];
                            var stairCleaned = stairCaseControl.SelectedValue.Remove(stairCaseControl.SelectedValue.IndexOf("dummyStair"));
                            int stairId;
                            if (int.TryParse(stairCleaned, out stairId))
                            {
                                stairIdResult = stairId;
                            }
                        }

                        // to do, add checkbox and select multiple

                        int expenseId;
                        if (valueControl != null && !string.IsNullOrEmpty(expenseControl.Text) &&
                            int.TryParse(expenseCleaned, out expenseId))
                        {
                            var cnt = new AssociationCounters()
                            {
                                Id_Estate = association.Id,
                                Id_Expense = expenseId,
                                Value = valueControl.Text,
                                AssociationCountersStairCase = new List<AssociationCountersStairCase> { new AssociationCountersStairCase { Id_StairCase = stairIdResult }}
                            };
                            cnts.Add(cnt);
                        }
                    }
                }
            }

            CountersManager.Addcounter(cnts);
            association = AssociationsManager.GetById(association.Id);

            Session[SessionConstants.SelectedAssociation] = association;
            Response.Redirect("~/?message=newEstate");
        }

        #endregion

        private void ConfigureStep2()
        {
            step1.Visible = false;
            step2.Visible = true;
            Step2PopulateExpenses();
        }

        private void Step2PopulateExpenses()
        {
            IEnumerable<Administratoro.DAL.Expenses> expenses = ExpensesManager.GetAllExpenses().OrderBy(e => e.LegalType);

            foreach (var expense in expenses)
            {
                var panel = new Panel
                {
                    CssClass = "associationNewExpense col-md-6 col-xs-6"
                };
                // add expense that were added
                CheckBox esexExists = new CheckBox();

                esexExists.AutoPostBack = false;
                esexExists.ID = String.Format("expense{0}", expense.Id);
                panel.Controls.Add(esexExists);

                // add expense name
                TableCell expenseName = new TableCell
                {
                    Text = "   " + expense.Name + "   "
                };
                panel.Controls.Add(expenseName);

                // add expense type
                DropDownList dp = new DropDownList();

                if (expense.Id != (int)Expense.AjutorÎncălzire)
                {
                    dp.Items.Add(new ListItem
                    {
                        Value = ((int)ExpenseType.PerIndex).ToString(),
                        Text = "Individuală prin indecși",
                        Selected = expense.LegalType == (int)ExpenseType.PerIndex
                    });

                    dp.Items.Add(new ListItem
                    {
                        Value = ((int)ExpenseType.PerCotaIndiviza).ToString(),
                        Text = "Cotă indiviză de proprietate",
                        Selected = expense.LegalType == (int)ExpenseType.PerCotaIndiviza
                    });

                    dp.Items.Add(new ListItem
                    {
                        Value = ((int)ExpenseType.PerNrTenants).ToString(),
                        Text = "Per număr persoane imobil",
                        Selected = expense.LegalType == (int)ExpenseType.PerNrTenants
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
                panel.Controls.Add(dp);
                expensesDefault.Controls.Add(panel);
            }
        }

        private void ConfigureStep3()
        {
            _step = 3;
            AddDymanicCounters();
            step1.Visible = false;
            step2.Visible = false;
            step3.Visible = true;
        }

        private static ListItem[] GetStairCasesAsListItemsWithExtradummyValue(Administratoro.DAL.Associations association, int controlId)
        {
            ListItem[] result = new ListItem[association.StairCases.Count + 1];
            int i = 0;

            var defaultExpense = new ListItem
            {
                Value = "dummyStair" + controlId,
                Text = "Contor pe bloc"
            };
            result[i] = defaultExpense;
            i++;

            foreach (var srairCase in association.StairCases)
            {
                var stair = new ListItem
                {
                    Value = srairCase.Id + "dummyStair" + controlId,
                    Text = "Scara " + srairCase.Nume
                };
                result[i] = stair;
                i++;
            }

            return result;
        }

        private ListItem[] GetExpensesAsListItemsWithExtradummyValue(int controlID)
        {
            Dictionary<int, int> expenses = GetSelectdExpenses();

            ListItem[] result = new ListItem[expenses.Count];
            int i = 0;

            foreach (var expense in expenses)
            {
                var ex = ExpensesManager.GetById(expense.Key);
                var theExpense = new ListItem
                {
                    Value = ex.Id + "dummyExpense" + controlID,
                    Text = ex.Name
                };

                result[i] = theExpense;
                i++;
            }

            return result;
        }

        private Dictionary<int, int> GetSelectdExpenses()
        {
            Dictionary<int, int> dictionary = new Dictionary<int, int>();

            foreach (var control in expensesDefault.Controls)
            {
                if (control is Panel)
                {
                    var thePanel = (Panel)control;
                    if (thePanel.Controls.Count >= 3 && thePanel.Controls[0] is CheckBox && thePanel.Controls[2] is DropDownList)
                    {
                        var chb = (CheckBox)thePanel.Controls[0];
                        var drp = (DropDownList)thePanel.Controls[2];
                        if (chb.Checked)
                        {
                            int expenseId;
                            int selectedTypeId;
                            if (int.TryParse(chb.ID.Replace("expense", ""), out expenseId) &&
                                int.TryParse(drp.SelectedValue, out selectedTypeId))
                            {
                                dictionary.Add(expenseId, selectedTypeId);
                            }
                        }
                    }
                }
            }
            return dictionary;
        }

        protected void btnAddNext_Command(object sender, CommandEventArgs e)
        {
        }

        private void AddStairsControl(int ControlNumber, out string key, out string value)
        {
            TextBox txtValue = new TextBox();
            txtValue.ID = "txtValue" + ControlNumber;
            txtValue.CssClass = "col-md-3 col-md-offset-3 col-xs-3";

            PlaceholderControls.Controls.Add(txtValue);

            TextBox txtIndiviza = new TextBox();
            txtIndiviza.ID = "txtIndiviza" + ControlNumber;
            txtIndiviza.CssClass = "col-md-3 col-md-offset-3 col-xs-3";

            PlaceholderControls.Controls.Add(txtIndiviza);

            PlaceholderControls.Controls.Add(new LiteralControl("<br />"));

            key = txtValue.UniqueID;
            value = txtIndiviza.UniqueID;
        }

        private DymanicCounter AddCounterControl(int ControlNumber)
        {
            var result = new DymanicCounter();
            var expenses = GetExpensesAsListItemsWithExtradummyValue(ControlNumber);
            var stairCases = GetStairCasesAsListItemsWithExtradummyValue(Association, ControlNumber);

            var cssClassRow = Association.HasStaircase ? "col-md-4 col-xs-4" : "col-md-6 col-xs-6";
            Panel rowPanel = new Panel { CssClass = "col-md-12 col-xs-12 associationsNewCounters" };

            var dpdCounterExpense = new DropDownList()
            {
                CssClass = cssClassRow,
                ID = "dpdCounterExpense" + ControlNumber
            };

            dpdCounterExpense.Items.AddRange(expenses);
            rowPanel.Controls.Add(dpdCounterExpense);

            TextBox txtContorValue = new TextBox()
            {
                ID = "txtContorValue" + ControlNumber,
                CssClass = cssClassRow
            };
            rowPanel.Controls.Add(txtContorValue);

            if (Association.HasStaircase && Association.StairCases.Count != 0)
            {
                var dpdStairCases = new DropDownList()
                {
                    CssClass = cssClassRow,
                    ID = "dpdStairCases" + ControlNumber,
                    EnableViewState = false
                };
                dpdStairCases.Items.AddRange(stairCases);
                rowPanel.Controls.Add(dpdStairCases);
                result.StaircaseId = dpdStairCases.UniqueID;
            }

            result.ExpenseId = dpdCounterExpense.UniqueID;
            result.NameId = txtContorValue.UniqueID;
            countersConfiguration.Controls.Add(rowPanel);

            return result;
        }

        protected void associationStairs_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (associationStairs.SelectedIndex == 0)
            {
                UpdatePanel1.Visible = false;
            }
            else
            {
                UpdatePanel1.Visible = true;
            }
        }

        protected void associationEqualIndiviza_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (associationEqualIndiviza.SelectedIndex == 0)
            {
                associationCotaIndivizaApartments.Visible = false;
            }
            else
            {
                associationCotaIndivizaApartments.Visible = true;
            }
        }

        protected void btnCounterAddNew_Click(object sender, EventArgs e)
        {
            NextCounterControl++;
            var control = AddCounterControl(NextCounterControl);
            DynamicCounters.Add(control);
        }

        #region Cancelevents

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/");
        }

        protected void btnCancel2_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/?message=newEstate");
        }

        protected void btnCancel3_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/?message=newEstate");
        }

        #endregion

    }
}