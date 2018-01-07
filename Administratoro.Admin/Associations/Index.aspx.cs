
namespace Admin.Associations
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
    using System.Data;
    using System.Data.SqlClient;

    public partial class Index : BasePage
    {

        protected void Page_Init(object sender, EventArgs e)
        {
            estateName.InnerText = Estate.Name;
            estateAddress.InnerText = Estate.Address;
            estateIndiviza.Text = Estate.Indiviza.ToString();
            estateFiscalCode.InnerText = Estate.FiscalCode;
            InitializeCounters2();
            InitializeStairs2();
        }

        private void InitializeCounters2()
        {
            gvCounters.DataSource = Estate.Counters;
            gvCounters.DataBind();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitializeStairs2();
            InitializeCounters2();
            if (!Page.IsPostBack)
            {
                estateStairs.SelectedIndex = Estate.HasStaircase ? 1 : 0;
            }

        }

        private void InitializeStairs2()
        {
            bool estateHasStairCase = Estate.HasStaircase;

            gvStaircasesMessage.Visible = estateHasStairCase;
            gvStaircases.Visible = estateHasStairCase;
            btneStatestairCasesNew.Visible = estateHasStairCase;
            if (estateHasStairCase)
            {
                gvStaircases.DataSource = Estate.StairCases;
                gvStaircases.DataBind();
            }
        }

        /*private void InitializeCounters(Estates es)
        {
            // add expense name header
            Label expenseNameHeader = new Label
            {
                Text = "Cheltuială",
                CssClass = "col-md-6 col-xs-6 countersHeadersNew"
            };
            associationcounters.Controls.Add(expenseNameHeader);

            // add coounter header
            Label expenseCounterHeader = new Label
            {
                Text = "Serie contor",
                CssClass = "col-md-6 col-xs-6 countersHeadersNew"
            };
            associationcounters.Controls.Add(expenseCounterHeader);

            foreach (Counters counter in es.Counters)
            {
                // add expense name header
                Label expenseName = new Label
                {
                    Text = "<b>" + counter.Expenses.Name + "</b>",
                    CssClass = "col-md-6 col-xs-6"
                };
                associationcounters.Controls.Add(expenseName);

                // add coounter header
                Label expenseCounter = new Label
                {
                    Text = counter.Value,
                    CssClass = "col-md-6 col-xs-6"
                };
                associationcounters.Controls.Add(expenseCounter);
                associationcounters.Controls.Add(new LiteralControl("<br />"));
            }
        }*/

        private void stairsRemove_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;

            int stairCaseId;
            if (int.TryParse(btn.CommandName, out stairCaseId))
            {
                StairCasesManager.Remove(stairCaseId, Estate.Id);
                var es = EstatesManager.GetById(Estate.Id);
                Session[SessionConstants.SelectedEstate] = es;
                InitializeStairs2();
            }
        }

        protected void estateStairs_SelectedIndexChanged(object sender, EventArgs e)
        {
            EstatesManager.UpdateStairs(Estate, estateStairs.SelectedIndex == 1);

            Estate.HasStaircase = estateStairs.SelectedIndex == 1;
            Session[SessionConstants.SelectedEstate] = Estate;
            Response.Redirect(Request.RawUrl);
        }

        //private bool StairCaseAddNew(Estates estate)
        //{
        //    txtEstateStairsNew.Attributes.CssStyle.Add("border-color", "");
        //    txtEstateStairsIndiviza.Attributes.CssStyle.Add("border-color", "");

        //    if (string.IsNullOrEmpty(txtEstateStairsNew.Text) || estate == null)
        //    {
        //        txtEstateStairsNew.Attributes.CssStyle.Add("border-color", "red");
        //        return false;
        //    }

        //    decimal? indivizaResult = null;
        //    decimal indiviza;
        //    if (decimal.TryParse(txtEstateStairsIndiviza.Text, out indiviza) || string.IsNullOrEmpty(txtEstateStairsIndiviza.Text))
        //    {
        //        indivizaResult = indiviza;
        //    }
        //    else
        //    {
        //        txtEstateStairsIndiviza.Attributes.CssStyle.Add("border-color", "red");
        //        return false;
        //    }

        //    StairCasesManager.AddNew(estate, txtEstateStairsNew.Text, indivizaResult);
        //    var newEstate = EstatesManager.GetById(estate.Id);
        //    Session[SessionConstants.SelectedEstate] = newEstate;
        //    InitializeStairs(newEstate);

        //    return true;
        //}

        protected void btnEstateCountersNew_Click(object sender, EventArgs e)
        {
            if (!drpEstateCounterTypeNew.Visible)
            {
                newCounter.Visible = true;

                List<Expenses> expenses = ExpensesManager.GetAllExpensesAsList();

                foreach (Expenses expense in expenses)
                {
                    drpEstateCounterTypeNew.Items.Add(new ListItem
                    {
                        Text = expense.Name,
                        Value = expense.Id.ToString()
                    });

                }
            }
            else
            {
                if (!string.IsNullOrEmpty(txtEstateCounterValueNew.Text))
                {
                    Counters counter = new Counters
                    {
                        Id_Estate = Estate.Id,
                        Value = txtEstateCounterValueNew.Text,
                        Id_Expense = drpEstateCounterTypeNew.SelectedValue.ToNullableInt().Value
                    };

                    CountersManager.Addcounter(counter);
                    var newEstate = EstatesManager.GetById(Estate.Id);
                    Session[SessionConstants.SelectedEstate] = newEstate;
                    Response.Redirect(Request.RawUrl);
                }
                else
                {
                    txtEstateCounterValueNew.Attributes.Add("style", "border-color:red");
                }
            }
        }

        protected void gvStaircases_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvStaircases.EditIndex = e.NewEditIndex;
            gvStaircases.DataBind();
            btneStatestairCasesNew.Visible = false;
        }

        protected void gvStaircases_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            var row = gvStaircases.Rows[e.RowIndex];
            var stair = new StairCases();
            if (row.Cells.Count > 4 &&
                row.Cells[2].Controls.Count > 0 && row.Cells[2].Controls[0] is TextBox &&
                row.Cells[4].Controls.Count > 0 && row.Cells[4].Controls[0] is TextBox)
            {
                var stairName = (TextBox)row.Cells[2].Controls[0];
                var stairCotaIndiviza = (TextBox)row.Cells[4].Controls[0];
                var stairIdValue = (TextBox)row.Cells[1].Controls[0];

                decimal newIndivizaValue;
                int stairId;
                if (string.IsNullOrEmpty(stairName.Text) || !decimal.TryParse(stairCotaIndiviza.Text, out newIndivizaValue)
                    || !int.TryParse(stairIdValue.Text, out stairId))
                {
                    stairName.Attributes.Add("style", "background-color:red");
                    stairCotaIndiviza.Attributes.Add("style", "background-color:red");
                }
                else
                {
                    var stairCase = StairCasesManager.GetById(stairId);
                    if (stairCase != null && (stairCase.Nume != stairName.Text || stairCase.Indiviza != newIndivizaValue))
                    {
                        var newStairCase = new StairCases
                        {
                            Nume = stairName.Text,
                            Indiviza = newIndivizaValue
                        };
                        StairCasesManager.Update(newStairCase, stairId);
                    }

                    gvStaircases.EditIndex = -1;
                    gvStaircases.DataBind();

                    var addedEstate = EstatesManager.GetById(Estate.Id);

                    Session[SessionConstants.SelectedEstate] = addedEstate;
                    var estates = EstatesManager.GetAllEstatesByPartner(Estate.Id_Partner);
                    Session[SessionConstants.AllEsates] = estates;
                    Response.Redirect(Request.RawUrl);
                }
            }
        }

        protected void gvStaircases_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            btnEstateCountersNew.Visible = true;
            gvStaircases.EditIndex = -1;
            gvStaircases.DataBind();
        }

        protected void gvStaircases_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            decimal dummy;

            if(decimal.TryParse(e.Row.Cells[4].Text, out dummy))
            {
                e.Row.Cells[4].Text= e.Row.Cells[4].Text + "%";
            }
            else if (e.Row.Cells[4].Text == "Indiviza")
            {
                e.Row.Cells[4].Text = e.Row.Cells[4].Text + " [totalul trebuie sa fie 100%]";
            }
            e.Row.Cells[1].Visible = false;
            e.Row.Cells[3].Visible = false;
        }

        protected void gvCounters_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            e.Row.Cells[3].Visible = false;
            e.Row.Cells[4].Visible = false;
            e.Row.Cells[5].Visible = false;
            e.Row.Cells[6].Visible = false;
            e.Row.Cells[7].Visible = false;
           // e.Row.Cells[9].Visible = false;
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Find the DropDownList in the Row
                DropDownList ddlCounters = (e.Row.FindControl("ddlExpense") as DropDownList);
                ddlCounters.DataSource = Expenses;
                ddlCounters.DataTextField = "Name";
                ddlCounters.DataValueField = "Id";
                ddlCounters.DataBind();

                //Label txtStairCase = (e.Row.FindControl("lblStairCase") as Label);
                //txtStairCase.Text = "adian e om fain";
                //string stairCase = (e.Row.FindControl("lblStairCaseId") as Label).Text;

                //Select the Country of Customer in DropDownList
                string expense = (e.Row.FindControl("lblExpense") as Label).Text;
                ddlCounters.Items.FindByValue(expense).Selected = true;
            }
        }

        protected void gvCounters_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            btnEstateCountersNew.Visible = true;
            gvCounters.EditIndex = -1;
            gvCounters.DataBind();
        }

        protected void gvCounters_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            var row = gvCounters.Rows[e.RowIndex];
            if (row.Cells.Count > 4 && row.Cells[2].Controls[0] is TextBox)
            {
                var counterValue = (TextBox)row.Cells[2].Controls[0];
                var counterIdValue = row.Cells[3];

                int counterId;
                if (string.IsNullOrEmpty(counterValue.Text) || !int.TryParse(counterIdValue.Text, out counterId))
                {
                    counterValue.Attributes.Add("style", "background-color:red");
                }
                else
                {
                    Counters counter = CountersManager.GetById(counterId);
                    if (counter != null && counterValue.Text != counter.Value)
                    {
                        var newCounter = new Counters
                        {
                            Value = counterValue.Text
                        };
                        CountersManager.Update(newCounter, counterId);
                    }

                    gvStaircases.EditIndex = -1;
                    gvStaircases.DataBind();

                    var addedEstate = EstatesManager.GetById(Estate.Id);

                    Session[SessionConstants.SelectedEstate] = addedEstate;
                    var estates = EstatesManager.GetAllEstatesByPartner(Estate.Id_Partner);
                    Session[SessionConstants.AllEsates] = estates;
                    Response.Redirect(Request.RawUrl);
                }
            }
        }

        protected void gvCounters_RowEditing(object sender, GridViewEditEventArgs e)
        {
            btnEstateCountersNew.Visible = false;
            gvCounters.EditIndex = e.NewEditIndex;
            gvCounters.DataBind();
        }

        protected void btneStatestairCasesNew_Click(object sender, EventArgs e)
        {
            if (newStairCasePanel.Visible)
            {
                decimal indiviza;
                if (!string.IsNullOrEmpty(txtEstateStairCaseName.Text) && decimal.TryParse(txtEstateStairCaseIndiviza.Text
                    , out indiviza))
                {
                    StairCasesManager.AddNew(Estate, txtEstateStairCaseName.Text, indiviza);
                    var newEstate = EstatesManager.GetById(Estate.Id);
                    Session[SessionConstants.SelectedEstate] = newEstate;
                    Response.Redirect(Request.RawUrl);
                }
                else
                {
                    txtEstateStairCaseIndiviza.Attributes.CssStyle.Add("border-color", "red");
                    txtEstateStairCaseName.Attributes.Add("style", "border-color:red");
                }
            }
            else
            {
                newStairCasePanel.Visible = true;
            }
        }
    }
}