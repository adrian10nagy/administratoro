using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Administratoro.BL.Managers;
using Administratoro.DAL;
using Administratoro.BL.Constants;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Drawing;

namespace Admin.Expenses
{
    public partial class AddEditExpense : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var id_exes = Request.QueryString["id_exes"];
            int idExpenseEstate;
            if (int.TryParse(id_exes, out idExpenseEstate))
            {
                EstateExpenses ee = EstateExpensesManager.GetById(idExpenseEstate);
                if (ee != null)
                {
                    btnRedirect.PostBackUrl = "Invoices.aspx?year=" + ee.Year + "&month=" + ee.Month;
                    btnRedirect.Visible = true;

                    lblExpenseMeessage.Text = "Modifică <b>" + ee.Expenses.Name + "</b> pe luna <b>"
                        + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(ee.Month) + "</b> (cheltuială " + ee.ExpenseTypes.Name + ")";

                    if (ee.ExpenseTypes.Id == (int)ExpenseType.PerIndex)
                    {
                        DataTable dt = new DataTable();
                        if (!Page.IsPostBack)
                        {
                            txtExpensesPerIndexValue.Text = ee.PricePerExpenseUnit.ToString();
                        }
                        if (ViewState["dtPerIndex"] == null)
                        {
                            this.InitializeGridViewExpensesPerIndex(dt, ee.Id);
                        }
                        else
                        {
                            dt = (DataTable)ViewState["dtPerIndex"];
                            ViewState["dtPerIndex"] = dt;
                            gvExpensesPerIndex.DataSource = dt;
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Expense type not per index");
                    }
                }
                else
                {
                    throw new ArgumentException("AddEditExpenseRequest parameter does not exist");
                }
            }
            else
            {
                throw new ArgumentException("AddEditExpenseRequest parameter not correct");
            }
        }

        private void InitializeGridViewExpensesPerIndex(DataTable dt, int esexId)
        {
            var estate = Session[SessionConstants.SelectedAssociation] as Estates;
            var tenants = ApartmentsManager.GetAllThatAreRegisteredWithSpecificCounters(estate.Id, esexId);
            EstateExpenses ee = EstateExpensesManager.GetById(esexId);
            foreach (var tenant in tenants)
            {
                TenantExpensesManager.ConfigurePerIndex(ee, tenant);

                string query = @"
                    Select 
                    TE.Id as Id,
                    T.Number as Apartament,
                    TE.IndexOld as 'Index vechi',
                    TE.IndexNew as 'Index nou',
                    TE.Value as 'Valoare'
                    from TenantExpenses TE
                    Inner join Tenants T
                    ON TE.Id_Tenant = T.Id
                    where Id_EstateExpense = " + esexId + " and Id_Tenant = " + tenant.Id +
                                               " and T.Id_Estate = " + estate.Id;

                SqlConnection cnn = new SqlConnection("data source=HOME\\SQLEXPRESS;initial catalog=Administratoro;integrated security=True;MultipleActiveResultSets=True;");
                SqlCommand cmd = new SqlCommand(query, cnn);
                SqlDataAdapter adp = new SqlDataAdapter(cmd);
                adp.Fill(dt);
            }

            ViewState["dtPerIndex"] = dt;
            gvExpensesPerIndex.DataSource = dt;
            gvExpensesPerIndex.DataBind();
        }

        protected void gvExpensesPerIndex_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvExpensesPerIndex.EditIndex = e.NewEditIndex;
            gvExpensesPerIndex.DataBind();
        }

        protected void gvExpensesPerIndex_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            var id_exes = Request.QueryString["id_exes"];
            int idExpenseEstate;
            if (int.TryParse(id_exes, out idExpenseEstate))
            {
                var row = gvExpensesPerIndex.Rows[e.RowIndex];
                int tenantExpenseId;
                if (int.TryParse(gvExpensesPerIndex.DataKeys[e.RowIndex].Value.ToString(), out tenantExpenseId))
                {
                    if (row.Cells.Count > 5 &&
                        row.Cells[4].Controls.Count > 0 && row.Cells[4].Controls[0] is TextBox &&
                        row.Cells[3].Controls.Count > 0 && row.Cells[3].Controls[0] is TextBox)
                    {
                        var cellOld = row.Cells[3].Controls[0] as TextBox;
                        var cellNew = row.Cells[4].Controls[0] as TextBox;

                        decimal newIndexValue;
                        decimal oldIndexValue;

                        if (!string.IsNullOrEmpty(cellNew.Text) && decimal.TryParse(cellNew.Text, out newIndexValue) &&
                            !string.IsNullOrEmpty(cellOld.Text) && decimal.TryParse(cellOld.Text, out oldIndexValue))
                        {
                            decimal? newValue = newIndexValue;
                            decimal? oldValue = oldIndexValue;
                            TenantExpensesManager.UpdateNewIndexAndValue(tenantExpenseId, idExpenseEstate, newValue, true, oldValue);
                        }
                        else
                        {
                            TenantExpensesManager.UpdateNewIndexAndValue(tenantExpenseId, idExpenseEstate, null, true, null);
                        }
                    }
                    else if (row.Cells.Count > 5 && row.Cells[5].Controls.Count > 0 && row.Cells[5].Controls[0] is TextBox)
                    {
                        var cellNew = row.Cells[5].Controls[0] as TextBox;

                        decimal newIndexValue;

                        if (string.IsNullOrEmpty(cellNew.Text) || !decimal.TryParse(cellNew.Text, out newIndexValue))
                        {
                            TenantExpensesManager.UpdateNewIndexAndValue(tenantExpenseId, idExpenseEstate, null, false);
                        }
                        else
                        {
                            TenantExpensesManager.UpdateNewIndexAndValue(tenantExpenseId, idExpenseEstate, newIndexValue, false);
                        }
                    }
                }
            }

            this.InitializeGridViewExpensesPerIndex(new DataTable(), idExpenseEstate);
            gvExpensesPerIndex.EditIndex = -1;
            gvExpensesPerIndex.DataBind();
        }

        protected void gvExpensesPerIndex_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvExpensesPerIndex.EditIndex = -1;
            gvExpensesPerIndex.DataBind();
        }

        protected void gvExpensesPerIndex_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (true)
            {
                gvExpensesPerIndex.DataKeyNames = new string[] { "Id", "Apartament", "Valoare" };
            }
            else
            {
                gvExpensesPerIndex.DataKeyNames = new string[] { "Id", "Apartament", "Valoare", "Index vechi" };
            }

            e.Row.Cells[1].Visible = false;
        }

        protected void btnExpensesPerIndexValue_Click(object sender, EventArgs e)
        {
            if (txtExpensesPerIndexValue.Enabled)
            {
                decimal newPricePerUnit;
                txtExpensesPerIndexValue.Attributes.CssStyle.Add("color", "");
                var id_exes = Request.QueryString["id_exes"];
                int idExpenseEstate;
                if (decimal.TryParse(txtExpensesPerIndexValue.Text, out newPricePerUnit) && int.TryParse(id_exes, out idExpenseEstate))
                {
                    txtExpensesPerIndexValue.Enabled = false;
                    EstateExpensesManager.UpdatePricePerUnit(idExpenseEstate, newPricePerUnit);
                    Response.Redirect(Request.RawUrl);
                }
                else
                {
                    txtExpensesPerIndexValue.Attributes.CssStyle.Add("color", "red");
                }
            }
            else
            {
                txtExpensesPerIndexValue.Enabled = true;
            }

        }
    }
}