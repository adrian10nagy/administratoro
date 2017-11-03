using Administratoro.BL.Constants;
using Administratoro.BL.Managers;
using Administratoro.DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;

namespace Admin.Expenses
{
    public partial class CurrentMonth : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            lblExpenseMeessage.Text = "Cheltuielile pe luna Septembrie";

            DataTable dt = new DataTable();

            if (ViewState["dt"] == null)
            {
                this.InitializeGridView(dt);
            }
            else
            {
                dt = (DataTable)ViewState["dt"];
                ViewState["dt"] = dt;
                GridView1.DataSource = dt;
            }
        }

        private void InitializeGridView(DataTable dt)
        {
            var tenants = TenantsManager.GetAllByEstateId(1);

            foreach (var tenant in tenants)
            {
                var expenses = ExpensesManager.GetAllExpensesByTenantAndMonth(tenant.Id, 2017, 9);
                string query = string.Empty;
                if (expenses.Count == 0)
                {
                    ExpensesManager.AddDefaultTenantExpense(tenant, 2017, 9);
                }
                query = @"USE administratoro DECLARE @DynamicPivotQuery AS NVARCHAR(MAX)
                                DECLARE @ColumnName AS NVARCHAR(MAX)
                                DECLARE @query  AS NVARCHAR(MAX)

                                SELECT @ColumnName= ISNULL(@ColumnName + ',','') 
		                                + QUOTENAME(Name)
                                FROM (SELECT DISTINCT Name 
                                FROM Expenses AS Expense
		                        INNER JOIN EstateExpenses as EE 
		                        on  EE.Id_Expense = Expense.Id 
		                        WHERE EE.Id_Estate = 1 and EE.isDefault = 0
                                AND EE.WasDisabled = 0 and EE.Month = 9 and EE.Year = 2017
		                        ) as Expense

                                set @DynamicPivotQuery = 
                                'select 
                                *
                                FROM(
                                select 
                                T.Id as Id, 	
                                T.Number as Apartament,



                                T.Dependents as NrPers, 


   	
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
		                         AND EE.year = 2017
                                AND EE.month = 9
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
            GridView1.DataSource = dt;
            GridView1.DataBind();
        }

        protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
        {
            GridView1.EditIndex = e.NewEditIndex;
            GridView1.DataBind();
        }

        protected void GridView1_RowUpdating1(object sender, GridViewUpdateEventArgs e)
        {
            Dictionary<int, object> expenses = new Dictionary<int, object>();
            int i = 0;

            foreach (TableCell tc in GridView1.HeaderRow.Cells)
            {
                if (tc != null && !string.IsNullOrEmpty(tc.Text))
                {
                    var expense = ExpensesManager.GetExpenseByName(tc.Text);
                    if (expense != null)
                    {
                        expenses.Add(i, expense);
                    }
                }

                i++;
            }

            var row = GridView1.Rows[e.RowIndex];
            int tenantId;
            int j = 0;
            if (int.TryParse(GridView1.DataKeys[e.RowIndex].Value.ToString(), out tenantId))
            {
                foreach (TableCell cell in row.Cells)
                {
                    if (cell.Controls.Count > 0 && cell.Controls[0] is TextBox)
                    {
                        if (expenses.ContainsKey(j))
                        {
                            var theCell = (TextBox)cell.Controls[0];
                            if (string.IsNullOrEmpty(theCell.Text))
                            {
                                TenantExpensesManager.RemoveTenantExpense(tenantId, 2017, 9, expenses[j]);
                            }
                            else
                            {
                                decimal expenseValue;
                                if (decimal.TryParse(theCell.Text, NumberStyles.Any, new CultureInfo("ro-RO"), out expenseValue))
                                {
                                    var theExpense = expenses[j];
                                    var te = ExpensesManager.GetExpenseByTenantMonth(tenantId, 2017, 9, theExpense);

                                    if (te != null)
                                    {
                                        te.Value = expenseValue;
                                        ExpensesManager.UpdateTenantExpense(tenantId, 2017, 9, te);
                                    }
                                    else
                                    {
                                        ExpensesManager.AddTenantExpense(tenantId, 2017, 9, theExpense, expenseValue);
                                    }
                                }
                                else
                                {
                                    theCell.Attributes.Add("color", "red");
                                }
                            }
                        }
                    }

                    j++;
                }

                InitializeGridView(new DataTable());
                GridView1.EditIndex = -1;
                GridView1.DataBind();
            }
        }

        protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridView1.EditIndex = -1;
            GridView1.DataBind();
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            e.Row.Cells[1].Visible = false;
        }

        protected void lblExpenseMeessageDownload_Click(object sender, EventArgs e)
        {
            // todo
        }
    }
}