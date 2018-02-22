
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
            var estate = Association;
            txtEstateName.Text = estate.Name;
            txtEstateAddress.Text = estate.Address;
            estateIndiviza.Text = estate.Indiviza.ToString();
            txtEstateFiscalCode.Text = estate.FiscalCode;
            txtEstateBanckAccount.Text = estate.BanckAccont;
            drpEstateEqualIndiviza.SelectedValue = (estate.CotaIndivizaAparments.HasValue) ? "1" : "0";
            if (estate.CotaIndivizaAparments.HasValue)
            {
                txtEstateCotaIndivizaApartments.Text = estate.CotaIndivizaAparments.Value.ToString();
                btnEstateEqualIndiviza.Visible = true;
                txtEstateCotaIndivizaApartments.Visible = true;
            }
            InitializeCounters2();
            InitializeStairs2();
        }

        private void InitializeCounters2()
        {
            gvCounters.DataSource = Association.Counters;
            gvCounters.DataBind();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitializeStairs2();
            InitializeCounters2();
            if (!Page.IsPostBack)
            {
                estateStairs.SelectedIndex = Association.HasStaircase ? 1 : 0;
                rbHasRoundup.SelectedIndex = Association.HasRoundUpColumn.HasValue && Association.HasRoundUpColumn.Value ? 1 : 0;
            }
        }

        private void InitializeStairs2()
        {
            bool estateHasStairCase = Association.HasStaircase;

            gvStaircasesMessage.Visible = estateHasStairCase;
            gvStaircases.Visible = estateHasStairCase;
            btneStatestairCasesNew.Visible = estateHasStairCase;
            if (estateHasStairCase)
            {
                gvStaircases.DataSource = Association.StairCases;
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
                StairCasesManager.Remove(stairCaseId, Association.Id);
                var es = AssociationsManager.GetById(Association.Id);
                Session[SessionConstants.SelectedAssociation] = es;
                InitializeStairs2();
            }
        }

        protected void estateStairs_SelectedIndexChanged(object sender, EventArgs e)
        {
            AssociationsManager.UpdateStairs(Association, estateStairs.SelectedIndex == 1);

            Association.HasStaircase = estateStairs.SelectedIndex == 1;
            Session[SessionConstants.SelectedAssociation] = Association;
            Response.Redirect(Request.RawUrl);
        }

        protected void btnEstateCountersNew_Click(object sender, EventArgs e)
        {
            if (!newCounter.Visible)
            {
                newCounter.Visible = true;

                List<Expenses> expenses = ExpensesManager.GetAllExpenses();

                foreach (Expenses expense in expenses)
                {
                    drpEstateCounterTypeNew.Items.Add(new ListItem
                    {
                        Text = expense.Name,
                        Value = expense.Id.ToString()
                    });

                }

                var defaultExpense = new ListItem
                {
                    Value = "",
                    Text = "Contor pe bloc"
                };
                drpEstateStairs.Items.Add(defaultExpense);
                if (Association.HasStaircase)
                {
                    foreach (var stairCase in Association.StairCases)
                    {
                        drpEstateStairs.Items.Add(new ListItem
                        {
                            Text = stairCase.Nume,
                            Value = stairCase.Id.ToString()
                        });
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(txtEstateCounterValueNew.Text))
                {
                    Counters counter = new Counters
                    {
                        Id_Estate = Association.Id,
                        Value = txtEstateCounterValueNew.Text,
                        Id_Expense = drpEstateCounterTypeNew.SelectedValue.ToNullableInt().Value,
                        Id_StairCase = drpEstateStairs.SelectedValue.ToNullableInt(),
                    };

                    CountersManager.Addcounter(counter);
                    var newEstate = AssociationsManager.GetById(Association.Id);
                    Session[SessionConstants.SelectedAssociation] = newEstate;
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

                    var addedEstate = AssociationsManager.GetById(Association.Id);

                    Session[SessionConstants.SelectedAssociation] = addedEstate;
                    var estates = AssociationsManager.GetAllAssociationsByPartner(Association.Id_Partner);
                    Session[SessionConstants.AllAssociations] = estates;
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

            if (decimal.TryParse(e.Row.Cells[4].Text, out dummy))
            {
                e.Row.Cells[4].Text = e.Row.Cells[4].Text + "%";
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
            e.Row.Cells[5].Visible = false;
            e.Row.Cells[6].Visible = false;
            e.Row.Cells[7].Visible = false;
            e.Row.Cells[8].Visible = false;
            e.Row.Cells[9].Visible = false;

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //Find the DropDownList in the Row
                DropDownList ddlCounters = (e.Row.FindControl("ddlExpense") as DropDownList);
                ddlCounters.DataSource = Expenses;
                ddlCounters.DataTextField = "Name";
                ddlCounters.DataValueField = "Id";
                ddlCounters.DataBind();

                //Select the Expense in DropDownList
                string expense = (e.Row.FindControl("lblExpense") as Label).Text;
                ddlCounters.Items.FindByValue(expense).Selected = true;

                DropDownList ddlStairCase = (e.Row.FindControl("ddlStairCase") as DropDownList);
                ddlStairCase.Items.Add(new ListItem
                {
                    Value = "",
                    Text = "Contor pe bloc"
                });
                foreach (var stairCase in Association.StairCases)
                {
                    ddlStairCase.Items.Add(new ListItem
                    {
                        Value = stairCase.Id.ToString(),
                        Text = stairCase.Nume
                    });
                }
                ddlStairCase.DataBind();

                //string stairCase = (e.Row.FindControl("lblStairCaseId") as Label).Text;
                //stairCase = "adian e om fain2";

                //Select the Expense in DropDownList
                string stair = (e.Row.FindControl("lblStairCaseId") as Label).Text;
                if (ddlStairCase.Items.FindByValue(stair) != null)
                {
                    ddlStairCase.Items.FindByValue(stair).Selected = true;
                }
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
                var counterIdValue = row.Cells[5];

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

                    var addedEstate = AssociationsManager.GetById(Association.Id);

                    Session[SessionConstants.SelectedAssociation] = addedEstate;
                    var estates = AssociationsManager.GetAllAssociationsByPartner(Association.Id_Partner);
                    Session[SessionConstants.AllAssociations] = estates;
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
                    StairCasesManager.AddNew(Association, txtEstateStairCaseName.Text, indiviza);
                    var newEstate = AssociationsManager.GetById(Association.Id);
                    Session[SessionConstants.SelectedAssociation] = newEstate;
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

        protected void bntEstateNameChange_Click(object sender, EventArgs e)
        {
            txtEstateName.Attributes.CssStyle.Add("border-color", "");
            if (txtEstateName.Enabled)
            {
                if (string.IsNullOrEmpty(txtEstateName.Text))
                {
                    txtEstateName.Attributes.CssStyle.Add("border-color", "red");
                }
                else
                {
                    txtEstateName.Enabled = false;
                    var es = AssociationsManager.GetById(Association.Id);
                    es.Name = txtEstateName.Text;
                    AssociationsManager.Update(es.Id, es);
                    Session[SessionConstants.SelectedAssociation] = es;
                    var estates = AssociationsManager.GetAllAssociationsByPartner(Association.Id_Partner);
                    Session[SessionConstants.AllAssociations] = estates;
                    Response.Redirect(Request.RawUrl);
                }
            }
            else
            {
                txtEstateName.Enabled = true;
            }
        }

        protected void btnEstateAddress_Click(object sender, EventArgs e)
        {
            txtEstateAddress.Attributes.CssStyle.Add("border-color", "");
            if (txtEstateAddress.Enabled)
            {
                if (string.IsNullOrEmpty(txtEstateAddress.Text))
                {
                    txtEstateAddress.Attributes.CssStyle.Add("border-color", "red");
                }
                else
                {
                    txtEstateAddress.Enabled = false;
                    var es = AssociationsManager.GetById(Association.Id);
                    es.Address = txtEstateAddress.Text;
                    AssociationsManager.Update(es.Id, es);
                    Session[SessionConstants.SelectedAssociation] = es;
                    var estates = AssociationsManager.GetAllAssociationsByPartner(Association.Id_Partner);
                    Session[SessionConstants.AllAssociations] = estates;
                    Response.Redirect(Request.RawUrl);
                }
            }
            else
            {
                txtEstateAddress.Enabled = true;
            }
        }

        protected void btnEstateFiscalCode_Click(object sender, EventArgs e)
        {
            txtEstateFiscalCode.Attributes.CssStyle.Add("border-color", "");
            if (txtEstateFiscalCode.Enabled)
            {
                if (string.IsNullOrEmpty(txtEstateFiscalCode.Text))
                {
                    txtEstateFiscalCode.Attributes.CssStyle.Add("border-color", "red");
                }
                else
                {
                    txtEstateFiscalCode.Enabled = false;
                    var es = AssociationsManager.GetById(Association.Id);
                    es.FiscalCode = txtEstateFiscalCode.Text;
                    AssociationsManager.Update(es.Id, es);
                    Session[SessionConstants.SelectedAssociation] = es;
                    var estates = AssociationsManager.GetAllAssociationsByPartner(Association.Id_Partner);
                    Session[SessionConstants.AllAssociations] = estates;
                    Response.Redirect(Request.RawUrl);
                }
            }
            else
            {
                txtEstateFiscalCode.Enabled = true;
            }
        }

        protected void btnEstateBanckAccount_Click(object sender, EventArgs e)
        {
            txtEstateBanckAccount.Attributes.CssStyle.Add("border-color", "");
            if (txtEstateBanckAccount.Enabled)
            {
                if (string.IsNullOrEmpty(txtEstateBanckAccount.Text))
                {
                    txtEstateBanckAccount.Attributes.CssStyle.Add("border-color", "red");
                }
                else
                {
                    txtEstateBanckAccount.Enabled = false;
                    var es = AssociationsManager.GetById(Association.Id);
                    es.BanckAccont = txtEstateBanckAccount.Text;
                    AssociationsManager.Update(es.Id, es);
                    Session[SessionConstants.SelectedAssociation] = es;
                    var estates = AssociationsManager.GetAllAssociationsByPartner(Association.Id_Partner);
                    Session[SessionConstants.AllAssociations] = estates;
                    Response.Redirect(Request.RawUrl);
                }
            }
            else
            {
                txtEstateBanckAccount.Enabled = true;
            }
        }

        protected void estateEqualIndiviza_SelectedIndexChanged(object sender, EventArgs e)
        {
            var es = AssociationsManager.GetById(Association.Id);
            if (es.CotaIndivizaAparments.HasValue && drpEstateEqualIndiviza.SelectedIndex == 0)
            {
                es.CotaIndivizaAparments = null;
                AssociationsManager.Update(es.Id, es);
                Session[SessionConstants.SelectedAssociation] = es;
            }

            if (drpEstateEqualIndiviza.SelectedIndex == 0)
            {
                txtEstateCotaIndivizaApartments.Visible = false;
                btnEstateEqualIndiviza.Visible = false;
            }
            else
            {
                txtEstateCotaIndivizaApartments.Visible = true;
                btnEstateEqualIndiviza.Visible = true;
            }
        }

        protected void btnEstateEqualIndiviza_Click(object sender, EventArgs e)
        {
            txtEstateCotaIndivizaApartments.Attributes.CssStyle.Add("border-color", "");
            if (txtEstateCotaIndivizaApartments.Enabled)
            {
                if (string.IsNullOrEmpty(txtEstateCotaIndivizaApartments.Text))
                {
                    txtEstateCotaIndivizaApartments.Attributes.CssStyle.Add("border-color", "red");
                }
                else
                {
                    txtEstateCotaIndivizaApartments.Enabled = false;
                    var es = AssociationsManager.GetById(Association.Id);

                    decimal? cotaIndivizaAparmentsResult;
                    decimal cotaIndivizaAparments;
                    if (decimal.TryParse(txtEstateCotaIndivizaApartments.Text, out cotaIndivizaAparments))
                    {
                        cotaIndivizaAparmentsResult = cotaIndivizaAparments;
                    }
                    else
                    {
                        cotaIndivizaAparmentsResult = null;
                    }
                    es.CotaIndivizaAparments = cotaIndivizaAparments;
                    AssociationsManager.Update(es.Id, es);
                    Session[SessionConstants.SelectedAssociation] = es;
                    var estates = AssociationsManager.GetAllAssociationsByPartner(Association.Id_Partner);
                    Session[SessionConstants.AllAssociations] = estates;
                    Response.Redirect(Request.RawUrl);
                }
            }
            else
            {
                txtEstateCotaIndivizaApartments.Enabled = true;
            }
        }

        protected void rbHasRoundup_SelectedIndexChanged(object sender, EventArgs e)
        {
            AssociationsManager.UpdateRoundUpColumn(Association, rbHasRoundup.SelectedIndex == 1);

            Association.HasRoundUpColumn = rbHasRoundup.SelectedIndex == 1;
            Session[SessionConstants.SelectedAssociation] = Association;
            Response.Redirect(Request.RawUrl);
        }
    }
}