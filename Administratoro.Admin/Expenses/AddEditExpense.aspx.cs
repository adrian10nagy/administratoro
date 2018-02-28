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

                    if (ee.ExpenseTypes.Id == (int)ExpenseType.PerCotaIndiviza)
                    {
                        txtExpensePerCotaIndiviza.Visible = true;
                    }
                    else if (ee.ExpenseTypes.Id == (int)ExpenseType.PerTenants)
                    {
                        txtExpensePerTenants.Visible = true;
                    }
                    else if (ee.ExpenseTypes.Id == (int)ExpenseType.PerIndex)
                    {
                        DataTable dt = new DataTable();
                        divExpensesPerIndex.Visible = true;
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

        private void InitializeGridViewPerConsumption(DataTable dt, int esexId)
        {
            var estateExpense = EstateExpensesManager.GetById(esexId);

            var tenants = ApartmentsManager.GetAllByEstateId(estateExpense.Estates.Id);

            foreach (var tenant in tenants)
            {
                var expenses = TenantExpensesManager.GetAllExpensesByTenantAndMonth(tenant.Id, estateExpense.Year, estateExpense.Month);
                string query = string.Empty;
                if (expenses.Count == 0)
                {
                    TenantExpensesManager.AddDefaultTenantExpense(tenant, estateExpense.Year, estateExpense.Month);
                }
                query = @"USE administratoro DECLARE @DynamicPivotQuery AS NVARCHAR(MAX)
                                DECLARE @ColumnName AS NVARCHAR(MAX)
                                DECLARE @query  AS NVARCHAR(MAX)

                                SELECT @ColumnName= ISNULL(@ColumnName + ',','') 
		                                + QUOTENAME(Name)
                                FROM (SELECT DISTINCT Name FROM Expenses AS Expense
		                        INNER JOIN EstateExpenses as EE 
		                        on  EE.Id_Expense = Expense.Id 
		                        WHERE EE.Id_Estate = 1 and EE.isDefault = 0
                                AND EE.WasDisabled = 0 and EE.Month = " + estateExpense.Month + @" and EE.Year = " + estateExpense.Year + @"
                                AND EE.Id  = " + esexId +
                                @"
		                        ) as Expense

                                set @DynamicPivotQuery = 
                                'select 
                                *
                                FROM(
                                select 
                                T.Id as Id, 	
                                T.Number as Apartament,
                                E.Name as ename,
                                TE.Value as tevalue
                                from EstateExpenses EE
                                LEFT JOIN TenantExpenses TE
                                ON TE.Id_EstateExpense = EE.Id
                                LEFT Join Tenants T
                                ON T.Id = TE.Id_tenant
                                LEFT Join Expenses E
                                ON E.Id = EE.Id_Expense
                                WHERE T.ID = 
		                        " + tenant.Id + @"
		                         AND EE.year = " + estateExpense.Year + @"
                                AND EE.month = " + estateExpense.Month + @"
                                AND EE.WasDisabled = 0
                                )
                                AS P
                                pivot
                                (
	                                sum(P.tevalue)
	                                for P.ename in (' + @ColumnName + ')

                                )as PIV
                                '

                                EXEC sp_executesql @DynamicPivotQuery";

                SqlConnection cnn = new SqlConnection("data source=HOME\\SQLEXPRESS;initial catalog=Administratoro;integrated security=True;MultipleActiveResultSets=True;");
                SqlCommand cmd = new SqlCommand(query, cnn);
                SqlDataAdapter adp = new SqlDataAdapter(cmd);
                adp.Fill(dt);
            }

            ViewState["dt"] = dt;
            gvExpenses.DataSource = dt;
            gvExpenses.DataBind();
        }

        protected void btnExpensePerCotaIndivizaAll_Click(object sender, EventArgs e)
        {
            decimal allvalue;
            int idExpenseEstate;
            var id_exes = Request.QueryString["id_exes"];

            if (decimal.TryParse(txtExpensePerCotaIndivizaAll.Text, out allvalue) && int.TryParse(id_exes, out idExpenseEstate))
            {
                TenantExpensesManager.AddCotaIndivizaTenantExpenses(idExpenseEstate, allvalue);
                lblExpenseMeessageInfo.Text = "Salvat cu succes";
                lblExpenseMeessageInfo.CssClass = "colorGreen";
                txtExpensePerCotaIndiviza.Visible = false;
            }
            else
            {
                txtExpensePerCotaIndivizaAll.BorderColor = Color.Red;
            }
        }

        protected void btnExpensePertenantAll_Click(object sender, EventArgs e)
        {
            string messageFormat = "{0} locatari luna asta, [<b>{1}</b> X <b>{0}</b> = <b>{2}</b>]";
            decimal allvalue;
            if (decimal.TryParse(txtExpensePerTenantAll.Text, out allvalue))
            {
                int idExpenseEstate;
                var id_exes = Request.QueryString["id_exes"];
                if (int.TryParse(id_exes, out idExpenseEstate))
                {
                    var ee = EstateExpensesManager.GetById(idExpenseEstate);
                    if (ee != null)
                    {
                        var allTenantDependents = ApartmentsManager.GetDependentsNr(ee.Id_Estate);
                        var valuePerTenant = Math.Round(allvalue / allTenantDependents, 2).ToString();
                        txtExpensePerTenantEach.Text = valuePerTenant;
                        txtExpensePerTenantEachInfo.Text = string.Format(messageFormat, allTenantDependents, valuePerTenant, allvalue);
                    }
                }
            }
            else
            {
                txtExpensePerTenantAll.BorderColor = Color.Red;
            }
        }

        protected void btnExpensePerTenantEach_Click(object sender, EventArgs e)
        {
            decimal valuePerTenant;
            int idExpenseEstate;
            var id_exes = Request.QueryString["id_exes"];

            if (decimal.TryParse(txtExpensePerTenantEach.Text, out valuePerTenant) && int.TryParse(id_exes, out idExpenseEstate))
            {
                TenantExpensesManager.AddPerTenantExpenses(idExpenseEstate, valuePerTenant);
                lblExpenseMeessageInfo.Text = "Salvat cu succes";
                lblExpenseMeessageInfo.CssClass = "colorGreen";
                txtExpensePerTenants.Visible = false;
            }
            else
            {
                txtExpensePerTenantEach.BorderColor = Color.Red;
            }
        }

        #region Expenses Per Index

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
                            decimal? newValue=newIndexValue;
                            decimal? oldValue=oldIndexValue;
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

        #endregion

        #region Expenses Per consumption


        protected void gvExpenses_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvExpenses.EditIndex = e.NewEditIndex;
            gvExpenses.DataBind();
        }

        protected void gvExpenses_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            var id_exes = Request.QueryString["id_exes"];
            int idExpenseEstate;
            if (int.TryParse(id_exes, out idExpenseEstate))
            {
                var row = gvExpenses.Rows[e.RowIndex];
                int apartmentid;
                if (int.TryParse(gvExpenses.DataKeys[e.RowIndex].Value.ToString(), out apartmentid))
                {
                    foreach (TableCell cell in row.Cells)
                    {
                        if (cell.Controls.Count > 0 && cell.Controls[0] is TextBox)
                        {
                            var theCell = (TextBox)cell.Controls[0];
                            if (string.IsNullOrEmpty(theCell.Text))
                            {
                                TenantExpensesManager.RemoveTenantExpense(apartmentid, idExpenseEstate);
                            }
                            else
                            {
                                decimal expenseValue;
                                if (decimal.TryParse(theCell.Text, NumberStyles.Any, new CultureInfo("ro-RO"), out expenseValue))
                                {
                                    var te = TenantExpensesManager.GetByExpenseEstateIdAndapartmentid(idExpenseEstate, apartmentid);

                                    if (te != null)
                                    {
                                        TenantExpensesManager.UpdateTenantExpense(idExpenseEstate, apartmentid, expenseValue);
                                    }
                                    else
                                    {
                                        TenantExpensesManager.AddTenantExpense(apartmentid, idExpenseEstate, expenseValue);
                                    }
                                }
                                else
                                {
                                    theCell.Attributes.Add("color", "red");
                                }
                            }

                        }
                    }
                }

                this.InitializeGridViewPerConsumption(new DataTable(), idExpenseEstate);
                gvExpenses.EditIndex = -1;
                gvExpenses.DataBind();
            }
        }

        protected void gvExpenses_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvExpenses.EditIndex = -1;
            gvExpenses.DataBind();
        }

        protected void gvExpenses_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            e.Row.Cells[1].Visible = false;
        }

        #endregion

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