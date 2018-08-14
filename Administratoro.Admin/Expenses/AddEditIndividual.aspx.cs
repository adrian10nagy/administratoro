using Administratoro.BL.Constants;
using Administratoro.BL.Managers;
using Administratoro.DAL;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Admin.Expenses
{
    public partial class AddEditHeatHelp : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var idExes = Request.QueryString["id_exes"];
            int idExpenseEstate;
            if (int.TryParse(idExes, out idExpenseEstate))
            {
                AssociationExpenses ee = AssociationExpensesManager.GetById(idExpenseEstate);
                if (ee != null)
                {
                    btnRedirect.PostBackUrl = "Invoices.aspx?year=" + ee.Year + "&month=" + ee.Month;
                    btnRedirect.Visible = true;

                    lblExpenseMeessage.Text = "Modifică <b>" + ee.Expenses.Name + "</b> pe luna <b>"
                        + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(ee.Month) + "</b> (cheltuială " + ee.ExpenseTypes.Name + ")";

                    if (ee.ExpenseTypes.Id == (int)ExpenseType.Individual)
                    {
                        DataTable dt = new DataTable();
                        if (ViewState["dtIndividual"] == null)
                        {
                            InitializeGridViewExpensesPerIndex(dt, ee.Id);
                        }
                        else
                        {
                            dt = (DataTable)ViewState["dtIndividual"];
                            ViewState["dtIndividual"] = dt;
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
            var apartments = ApartmentsManager.GetForIndividual(Association.Id, esexId);

            AssociationExpenses ee = AssociationExpensesManager.GetById(esexId);
            foreach (var apartment in apartments)
            {
                ApartmentExpensesManager.ConfigureIndividual(ee, apartment);

                string query = @"
                    Select 
                    TE.Id as Id,
                    A.Number as Apartament,
                    TE.Value as 'Valoare'
                    from ApartmentExpenses TE
                    Inner join Apartments A
                    ON TE.Id_Tenant = A.Id
                    where Id_EstateExpense = " + esexId + " and Id_Tenant = " + apartment.Id +
                                               " and A.Id_Estate = " + Association.Id;

                SqlConnection cnn = new SqlConnection("data source=HOME\\SQLEXPRESS;initial catalog=Administratoro;integrated security=True;MultipleActiveResultSets=True;");
                SqlCommand cmd = new SqlCommand(query, cnn);
                SqlDataAdapter adp = new SqlDataAdapter(cmd);
                adp.Fill(dt);
            }

            ViewState["dtIndividual"] = dt;
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
                int apartmentExpenseId;
                if (int.TryParse(gvExpensesPerIndex.DataKeys[e.RowIndex].Value.ToString(), out apartmentExpenseId))
                {
                    if (row.Cells.Count > 3 && row.Cells[3].Controls.Count > 0 && row.Cells[3].Controls[0] is TextBox)
                    {
                        var cellOld = row.Cells[3].Controls[0] as TextBox;

                        decimal newValue;

                        if (!string.IsNullOrEmpty(cellOld.Text) && decimal.TryParse(cellOld.Text, out newValue))
                        {
                            ApartmentExpensesManager.UpdateApartmentExpense(apartmentExpenseId, newValue);
                        }
                        else
                        {
                            ApartmentExpensesManager.UpdateApartmentExpense(apartmentExpenseId, null);
                        }
                    }
                }
            }

            InitializeGridViewExpensesPerIndex(new DataTable(), idExpenseEstate);
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
            gvExpensesPerIndex.DataKeyNames = new[] { "Id", "Apartament"};
            e.Row.Cells[1].Visible = false;
        }
    }
}