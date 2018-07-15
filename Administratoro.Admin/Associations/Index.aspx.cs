
using System.Globalization;

namespace Admin.Associations
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Managers;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI.WebControls;
    using Administratoro.BL.Extensions;
    using Administratoro.BL.Models;

    public partial class Index : BasePage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            if(!Page.IsPostBack)
            {
                InitializeCounters();
            }
            else
            {
                gvCounters.DataBind();
            }

            var assoc = Association;
            txtAssociationName.Text = assoc.Name;
            txtAssociationAddress.Text = assoc.Address;
            txtAssociationFiscalCode.Text = assoc.FiscalCode;
            txtAssociationBanckAccount.Text = assoc.BanckAccont;
            drpAssociationEqualIndiviza.SelectedValue = (assoc.CotaIndivizaAparments.HasValue) ? "1" : "0";
            if (assoc.CotaIndivizaAparments.HasValue)
            {
                txtAssociationCotaIndivizaApartments.Text = assoc.CotaIndivizaAparments.Value.ToString(CultureInfo.InvariantCulture);
                btnAssociationEqualIndiviza.Visible = true;
                txtAssociationCotaIndivizaApartments.Visible = true;
            }

            InitializeStairs();
        }

        private void InitializeCounters()
        {
            var associationCounters = Association.AssociationCounters.OrderBy(ac => ac.Id_Expense);
            var formatedAssCounters = PrepareToBind(associationCounters);
            gvCounters.DataSource = formatedAssCounters;

            gvCounters.DataBind();
        }

        private static object PrepareToBind(IOrderedEnumerable<AssociationCounters> associationCounters)
        {
            var result = new List<FormatedAssociationCounter>();

            foreach (var associationCounter in associationCounters)
            {
                var formatedAssociationCounter = new FormatedAssociationCounter(associationCounter);
                result.Add(formatedAssociationCounter);
            }

            return result;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitializeStairs();
            InitializeCounters();
            if (!Page.IsPostBack)
            {
                associationStairs.SelectedIndex = Association.HasStaircase ? 1 : 0;
                rbHasRoundup.SelectedIndex = Association.HasRoundUpColumn.HasValue && Association.HasRoundUpColumn.Value ? 1 : 0;
            }
        }

        private void InitializeStairs()
        {
            bool associationHasStairCase = Association.HasStaircase;

            gvStaircasesMessage.Visible = associationHasStairCase;
            gvStaircases.Visible = associationHasStairCase;
            btnAssociationStairCasesNew.Visible = associationHasStairCase;
            if (associationHasStairCase)
            {
                gvStaircases.DataSource = Association.StairCases;
                gvStaircases.DataBind();
            }
        }

        private void stairsRemove_Click(object sender, EventArgs e)
        {
            LinkButton btn = (LinkButton)sender;

            int stairCaseId;
            if (int.TryParse(btn.CommandName, out stairCaseId))
            {
                StairCasesManager.Remove(stairCaseId, Association.Id);
                var es = AssociationsManager.GetById(Association.Id);
                Session[SessionConstants.SelectedAssociation] = es;
                InitializeStairs();
            }
        }

        protected void associationStairs_SelectedIndexChanged(object sender, EventArgs e)
        {
            AssociationsManager.UpdateStairs(Association, associationStairs.SelectedIndex == 1);

            Association.HasStaircase = associationStairs.SelectedIndex == 1;
            Session[SessionConstants.SelectedAssociation] = Association;
            Response.Redirect(Request.RawUrl);
        }

        protected void btnAssociationCountersNew_Click(object sender, EventArgs e)
        {
            if (newCounter.Visible)
            {
                if (!string.IsNullOrEmpty(txtAssociationCounterValueNew.Text))
                {
                    List<AssociationCountersStairCase> associationCounterStariCases = GetStairCases(chbAssociationStairs);
                    AssociationCounters associationCounters = new AssociationCounters
                    {
                        Id_Estate = Association.Id,
                        Value = txtAssociationCounterValueNew.Text,
                        Id_Expense = drpAssociationCounterTypeNew.SelectedValue.ToNullableInt().Value,
                        AssociationCountersStairCase = associationCounterStariCases
                    };

                    AssociationCountersManager.Add(associationCounters);
                    var newAssociation = AssociationsManager.GetById(Association.Id);
                    Session[SessionConstants.SelectedAssociation] = newAssociation;
                    Response.Redirect(Request.RawUrl);
                }
                else
                {
                    txtAssociationCounterValueNew.Attributes.Add("style", "border-color:red");
                }
            }
            else
            {
                newCounter.Visible = true;

                IEnumerable<Expenses> expenses = ExpensesManager.GetAllExpenses();

                foreach (Expenses expense in expenses)
                {
                    drpAssociationCounterTypeNew.Items.Add(new ListItem
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
                chbAssociationStairs.Items.Add(defaultExpense);
                if (Association.HasStaircase)
                {
                    foreach (var stairCase in Association.StairCases)
                    {
                        chbAssociationStairs.Items.Add(new ListItem
                        {
                            Text = stairCase.Nume,
                            Value = stairCase.Id.ToString()
                        });
                    }
                }
            }
        }

        private static List<AssociationCountersStairCase> GetStairCases(CheckBoxList chbAssociationStairs)
        {

            var result = new List<AssociationCountersStairCase>();

            foreach (ListItem item in chbAssociationStairs.Items)
            {
                if (item.Selected)
                {
                    var newCounte = new AssociationCountersStairCase
                    {
                        Id_StairCase = item.Value.ToNullableInt()
                    };

                    result.Add(newCounte);
                }
            }

            return result;
        }

        protected void gvStaircases_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvStaircases.EditIndex = e.NewEditIndex;
            gvStaircases.DataBind();
            btnAssociationStairCasesNew.Visible = false;
        }

        protected void gvStaircases_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            var row = gvStaircases.Rows[e.RowIndex];
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

                    var addedAssociation = AssociationsManager.GetById(Association.Id);

                    Session[SessionConstants.SelectedAssociation] = addedAssociation;
                    var associations = AssociationsManager.GetAllAssociationsByPartner(Association.Id_Partner);
                    Session[SessionConstants.AllAssociations] = associations;
                    Response.Redirect(Request.RawUrl);
                }
            }
        }

        protected void gvStaircases_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            btnAssociationCountersNew.Visible = true;
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

                //Select the Expense in CheckBoxList
                string expense = (e.Row.FindControl("lblExpense") as Label).Text;
                ddlCounters.Items.FindByValue(expense).Selected = true;

                CheckBoxList chbStairCase = (e.Row.FindControl("chbStairCase") as CheckBoxList);
                chbStairCase.Items.Add(new ListItem
                {
                    Value = "",
                    Text = "Contor pe bloc"
                });
                foreach (var stairCase in Association.StairCases)
                {
                    chbStairCase.Items.Add(new ListItem
                    {
                        Value = stairCase.Id.ToString(),
                        Text = stairCase.Nume
                    });
                }
                chbStairCase.DataBind();


                //Select the Expense in DropDownList
                string stairs = (e.Row.FindControl("lblStairCaseId") as Label).Text;

                foreach (var stair in stairs.Split(','))
                {
                    if (stair == string.Empty)
                    {
                        continue;
                    }

                    if (stair == "-1")
                    {
                        chbStairCase.Items.FindByValue(string.Empty).Selected = true;
                    }

                    if (chbStairCase.Items.FindByValue(stair) != null)
                    {
                        chbStairCase.Items.FindByValue(stair).Selected = true;
                    }
                }
            }
        }

        protected void gvCounters_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            btnAssociationCountersNew.Visible = true;
            gvCounters.EditIndex = -1;
            gvCounters.DataBind();
        }

        protected void gvCounters_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            var row = gvCounters.Rows[e.RowIndex];
            if (row.Cells.Count > 4 && row.Cells[2].Controls[0] is TextBox &&
                row.Cells[4].Controls[3] is CheckBoxList)
            {
                var counterValue = (TextBox)row.Cells[2].Controls[0];
                var stairCases = (CheckBoxList)row.Cells[4].Controls[3];
                var counterIdValue = row.Cells[6];

                int counterId;
                if (string.IsNullOrEmpty(counterValue.Text) || !int.TryParse(counterIdValue.Text, out counterId))
                {
                    counterValue.Attributes.Add("style", "background-color:red");
                }
                else
                {
                    AssociationCounters associationCounters = AssociationCountersManager.GetById(counterId);
                    List<AssociationCountersStairCase> associationCounterStariCases = GetStairCases(stairCases);
                    if (associationCounters != null)
                    {
                        var newCounter = new AssociationCounters
                        {
                            Value = counterValue.Text,
                            AssociationCountersStairCase = associationCounterStariCases,
                            Id = counterId
                        };
                        AssociationCountersManager.Update(newCounter);
                    }

                    gvStaircases.EditIndex = -1;
                    gvStaircases.DataBind();

                    var addedAssociation = AssociationsManager.GetById(Association.Id);

                    Session[SessionConstants.SelectedAssociation] = addedAssociation;
                    var Associations = AssociationsManager.GetAllAssociationsByPartner(Association.Id_Partner);
                    Session[SessionConstants.AllAssociations] = Associations;
                    Response.Redirect(Request.RawUrl);
                }
            }
        }

        protected void gvCounters_RowEditing(object sender, GridViewEditEventArgs e)
        {
            btnAssociationCountersNew.Visible = false;
            gvCounters.EditIndex = e.NewEditIndex;
            gvCounters.DataBind();
        }

        protected void btnAssociationstairCasesNew_Click(object sender, EventArgs e)
        {
            if (newStairCasePanel.Visible)
            {
                decimal indiviza;
                if (!string.IsNullOrEmpty(txtAssociationStairCaseName.Text) && decimal.TryParse(txtAssociationStairCaseIndiviza.Text
                    , out indiviza))
                {
                    StairCasesManager.AddNew(Association, txtAssociationStairCaseName.Text, indiviza);
                    var newAssociation = AssociationsManager.GetById(Association.Id);
                    Session[SessionConstants.SelectedAssociation] = newAssociation;
                    Response.Redirect(Request.RawUrl);
                }
                else
                {
                    txtAssociationStairCaseIndiviza.Attributes.CssStyle.Add("border-color", "red");
                    txtAssociationStairCaseName.Attributes.Add("style", "border-color:red");
                }
            }
            else
            {
                newStairCasePanel.Visible = true;
            }
        }

        protected void bntAssociationNameChange_Click(object sender, EventArgs e)
        {
            txtAssociationName.Attributes.CssStyle.Add("border-color", "");
            if (txtAssociationName.Enabled)
            {
                if (string.IsNullOrEmpty(txtAssociationName.Text))
                {
                    txtAssociationName.Attributes.CssStyle.Add("border-color", "red");
                }
                else
                {
                    txtAssociationName.Enabled = false;
                    var es = AssociationsManager.GetById(Association.Id);
                    es.Name = txtAssociationName.Text;
                    AssociationsManager.Update(es);
                    Session[SessionConstants.SelectedAssociation] = es;
                    var associations = AssociationsManager.GetAllAssociationsByPartner(Association.Id_Partner);
                    Session[SessionConstants.AllAssociations] = associations;
                    Response.Redirect(Request.RawUrl);
                }
            }
            else
            {
                txtAssociationName.Enabled = true;
            }
        }

        protected void btnAssociationAddress_Click(object sender, EventArgs e)
        {
            txtAssociationAddress.Attributes.CssStyle.Add("border-color", "");
            if (txtAssociationAddress.Enabled)
            {
                if (string.IsNullOrEmpty(txtAssociationAddress.Text))
                {
                    txtAssociationAddress.Attributes.CssStyle.Add("border-color", "red");
                }
                else
                {
                    txtAssociationAddress.Enabled = false;
                    var es = AssociationsManager.GetById(Association.Id);
                    es.Address = txtAssociationAddress.Text;
                    AssociationsManager.Update(es);
                    Session[SessionConstants.SelectedAssociation] = es;
                    var associations = AssociationsManager.GetAllAssociationsByPartner(Association.Id_Partner);
                    Session[SessionConstants.AllAssociations] = associations;
                    Response.Redirect(Request.RawUrl);
                }
            }
            else
            {
                txtAssociationAddress.Enabled = true;
            }
        }

        protected void btnAssociationFiscalCode_Click(object sender, EventArgs e)
        {
            txtAssociationFiscalCode.Attributes.CssStyle.Add("border-color", "");
            if (txtAssociationFiscalCode.Enabled)
            {
                if (string.IsNullOrEmpty(txtAssociationFiscalCode.Text))
                {
                    txtAssociationFiscalCode.Attributes.CssStyle.Add("border-color", "red");
                }
                else
                {
                    txtAssociationFiscalCode.Enabled = false;
                    var es = AssociationsManager.GetById(Association.Id);
                    es.FiscalCode = txtAssociationFiscalCode.Text;
                    AssociationsManager.Update(es);
                    Session[SessionConstants.SelectedAssociation] = es;
                    var associations = AssociationsManager.GetAllAssociationsByPartner(Association.Id_Partner);
                    Session[SessionConstants.AllAssociations] = associations;
                    Response.Redirect(Request.RawUrl);
                }
            }
            else
            {
                txtAssociationFiscalCode.Enabled = true;
            }
        }

        protected void btnAssociationBanckAccount_Click(object sender, EventArgs e)
        {
            txtAssociationBanckAccount.Attributes.CssStyle.Add("border-color", "");
            if (txtAssociationBanckAccount.Enabled)
            {
                if (string.IsNullOrEmpty(txtAssociationBanckAccount.Text))
                {
                    txtAssociationBanckAccount.Attributes.CssStyle.Add("border-color", "red");
                }
                else
                {
                    txtAssociationBanckAccount.Enabled = false;
                    var es = AssociationsManager.GetById(Association.Id);
                    es.BanckAccont = txtAssociationBanckAccount.Text;
                    AssociationsManager.Update(es);
                    Session[SessionConstants.SelectedAssociation] = es;
                    var associations = AssociationsManager.GetAllAssociationsByPartner(Association.Id_Partner);
                    Session[SessionConstants.AllAssociations] = associations;
                    Response.Redirect(Request.RawUrl);
                }
            }
            else
            {
                txtAssociationBanckAccount.Enabled = true;
            }
        }

        protected void associationEqualIndiviza_SelectedIndexChanged(object sender, EventArgs e)
        {
            var es = AssociationsManager.GetById(Association.Id);
            if (es.CotaIndivizaAparments.HasValue && drpAssociationEqualIndiviza.SelectedIndex == 0)
            {
                es.CotaIndivizaAparments = null;
                AssociationsManager.Update(es);
                Session[SessionConstants.SelectedAssociation] = es;
            }

            if (drpAssociationEqualIndiviza.SelectedIndex == 0)
            {
                txtAssociationCotaIndivizaApartments.Visible = false;
                btnAssociationEqualIndiviza.Visible = false;
            }
            else
            {
                txtAssociationCotaIndivizaApartments.Visible = true;
                btnAssociationEqualIndiviza.Visible = true;
            }
        }

        protected void btnAssociationEqualIndiviza_Click(object sender, EventArgs e)
        {
            txtAssociationCotaIndivizaApartments.Attributes.CssStyle.Add("border-color", "");
            if (txtAssociationCotaIndivizaApartments.Enabled)
            {
                if (string.IsNullOrEmpty(txtAssociationCotaIndivizaApartments.Text))
                {
                    txtAssociationCotaIndivizaApartments.Attributes.CssStyle.Add("border-color", "red");
                }
                else
                {
                    txtAssociationCotaIndivizaApartments.Enabled = false;
                    var es = AssociationsManager.GetById(Association.Id);

                    decimal? cotaIndivizaAparmentsResult;
                    decimal cotaIndivizaAparments;
                    if (decimal.TryParse(txtAssociationCotaIndivizaApartments.Text, out cotaIndivizaAparments))
                    {
                        cotaIndivizaAparmentsResult = cotaIndivizaAparments;
                    }
                    else
                    {
                        cotaIndivizaAparmentsResult = null;
                    }
                    es.CotaIndivizaAparments = cotaIndivizaAparments;
                    AssociationsManager.Update(es);
                    Session[SessionConstants.SelectedAssociation] = es;
                    var Associations = AssociationsManager.GetAllAssociationsByPartner(Association.Id_Partner);
                    Session[SessionConstants.AllAssociations] = Associations;
                    Response.Redirect(Request.RawUrl);
                }
            }
            else
            {
                txtAssociationCotaIndivizaApartments.Enabled = true;
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