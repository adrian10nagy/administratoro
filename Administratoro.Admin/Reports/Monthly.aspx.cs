
namespace Admin.Expenses
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Managers;
    using Administratoro.DAL;
    using ClosedXML.Excel;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Web.UI.WebControls;

    public partial class CurrentMonth : System.Web.UI.Page
    {
        private int _month
        {
            get
            {
                var monthId = Request.QueryString["month"];
                int month;
                if (!int.TryParse(monthId, out month) || month > 13 || month < 0)
                {
                    Response.Redirect("..\\Expenses\\Dashboard.aspx", true);
                }

                return month;
            }
        }

        private int _year
        {
            get
            {
                var yearId = Request.QueryString["year"];
                int year;
                if (!int.TryParse(yearId, out year) || year < 0)
                {
                    Response.Redirect("..\\Expenses\\Dashboard.aspx", true);
                }

                return year;
            }
        }

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
            var year = _year;
            var month = _month;

            var estate = Session[SessionConstants.SelectedEstate] as Estates;

            var tenants = ApartmentsManager.GetAllByEstateId(estate.Id);

            foreach (var tenant in tenants)
            {
                var expenses = ExpensesManager.GetAllExpensesByTenantAndMonth(tenant.Id, year, month);
                string query = string.Empty;
                if (expenses.Count == 0)
                {
                    ExpensesManager.AddDefaultTenantExpense(tenant, year, month);
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
		                        WHERE EE.Id_Estate = " + estate.Id + @" and EE.isDefault = 0
                                AND EE.WasDisabled = 0 and EE.Month = " + month + @" and EE.Year = " + year + @"
		                        ) as Expense

                                set @DynamicPivotQuery = 
                                'select 
                                *
                                FROM(
                                select 
                                T.Id as Id, 	
                                T.Number as Apartament,
                                T.Name,
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
		                         AND EE.year = " + year + @"
                                AND EE.month = " + month + @"
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
            var year = _year;
            var month = _month;

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
            int apartmentid;
            int j = 0;
            if (int.TryParse(GridView1.DataKeys[e.RowIndex].Value.ToString(), out apartmentid))
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
                                TenantExpensesManager.RemoveTenantExpense(apartmentid, year, month, expenses[j]);
                            }
                            else
                            {
                                decimal expenseValue;
                                if (decimal.TryParse(theCell.Text, NumberStyles.Any, new CultureInfo("ro-RO"), out expenseValue))
                                {
                                    var theExpense = expenses[j];
                                    var te = ExpensesManager.GetExpenseByTenantMonth(apartmentid, year, month, theExpense);

                                    if (te != null)
                                    {
                                        te.Value = expenseValue;
                                        ExpensesManager.UpdateTenantExpense(apartmentid, year, month, te);
                                    }
                                    else
                                    {
                                        ExpensesManager.AddTenantExpense(apartmentid, year, month, theExpense, expenseValue);
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
            //to do
            var dt = ExpensesManager.GetAllEpensesAsList(1);

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt, "Carti");
                string myName = Server.UrlEncode("Cheltuieli" + "_" + DateTime.Now.ToShortDateString() + ".xlsx");
                MemoryStream stream = GetStream(wb);
                Response.Clear();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + myName);
                Response.ContentType = "application/vnd.ms-excel";
                Response.BinaryWrite(stream.ToArray());
                Response.Flush();
                Response.SuppressContent = true;
            }
        }

        public MemoryStream GetStream(XLWorkbook excelWorkbook)
        {
            MemoryStream fs = new MemoryStream();
            excelWorkbook.SaveAs(fs);
            fs.Position = 0;
            return fs;
        }
    }
}