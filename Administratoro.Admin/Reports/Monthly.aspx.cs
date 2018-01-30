
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

    public partial class CurrentMonth : BasePage
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

            if (!Page.IsPostBack)
            {
                InitializeStairCases();
            }

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

        private void InitializeStairCases()
        {
            drpDisplayMode.Items.Add(new ListItem 
            {
                Text = "Tot blocul",
                Value = ""
            });

            foreach (StairCases stairCase in Association.StairCases)
            {
                drpDisplayMode.Items.Add(new ListItem
                {
                    Text = "Scara "+stairCase.Nume,
                    Value = stairCase.Id.ToString()
                }); 
            }
        }

        private void InitializeGridView(DataTable dt, int? stairCase = null)
        {
            var year = _year;
            var month = _month;

            var estate = Session[SessionConstants.SelectedAssociation] as Estates;

            dt = TenantExpensesManager.GetMonthlyRaportAsDataTable(Association.Id, _year, _month, stairCase);

            ViewState["dt"] = dt;
            GridView1.DataSource = dt;
            GridView1.DataBind();
        }

        protected void lblExpenseMeessageDownload_Click(object sender, EventArgs e)
        {
            DataTable dt;
            int stairCase;
            if(int.TryParse(drpDisplayMode.SelectedValue, out stairCase))
            {
                dt = TenantExpensesManager.GetMonthlyRaportAsDataTable(Association.Id, _year, _month, stairCase);
            }
            else
            {
                dt = TenantExpensesManager.GetMonthlyRaportAsDataTable(Association.Id, _year, _month, null);
            }


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

        protected void drpDisplayMode_SelectedIndexChanged(object sender, EventArgs e)
        {
             DataTable dt = new DataTable();

            if(string.IsNullOrEmpty(drpDisplayMode.SelectedValue))
            {
                this.InitializeGridView(dt);
            }
            else
            {
                int result;
                if(int.TryParse(drpDisplayMode.SelectedValue, out result))
                {
                    this.InitializeGridView(dt, result);
                }
            }
        }
    }
}