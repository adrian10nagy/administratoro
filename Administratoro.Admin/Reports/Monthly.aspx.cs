
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
    using System.Web.UI;
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
            lblExpenseMeessage.Text = "Cheltuielile pe luna <b>" + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(_month) + " " + _year + "</b>";

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
            RecalculationManager.RecalculateMonthlyExpenses(Association.Id, _year, _month);

            dt = ApartmentExpensesManager.GetMonthlyRaportAsDataTable(Association.Id, _year, _month, stairCase);
           
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
                dt = ApartmentExpensesManager.GetMonthlyRaportAsDataTable(Association.Id, _year, _month, stairCase);
                dt.Rows.Add(new TableCell());
            }
            else
            {
                dt = ApartmentExpensesManager.GetMonthlyRaportAsDataTable(Association.Id, _year, _month, null);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add(dt, "Cheltuieli");
                ws.Row(1).InsertRowsAbove(1);

                ws.Cells("A1").Value = ws.Cell(2, 1).Value; 
                ws.Range("A1:A2").Merge();
                
                ws.Cells("B1").Value = ws.Cell(2, 2).Value;
                ws.Range("B1:B2").Merge();

                ws.Cells("C1").Value = ws.Cell(3, 2).Value;
                ws.Range("C1:C2").Merge();

                ws.Cells("D1").Value = ws.Cell(4, 2).Value;
                ws.Range("D1:D2").Merge();

                var expenses = AssociationExpensesManager.GetByMonthAndYearNotDisabled(Association.Id, _year, _month).GroupBy(ee => ee.Id_ExpenseType).OrderBy(er => er.Key);
                char position = 'E';
                foreach (var expense in expenses)
                {
                    var tempPosition = position;
                    position = (char)(((int)position) + expense.ToList().Count-1);
                    var range = string.Format("{0}1:{1}1",tempPosition.ToString(), position.ToString());
                    ws.Cells(tempPosition + "1").Value = expense.FirstOrDefault().ExpenseTypes.Name;
                    ws.Range(range).Merge();
                    position = (char)(((int)position+1)); 
                }

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

        public static MemoryStream GetStream(XLWorkbook excelWorkbook)
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

        protected void GridView1_RowCreated(object sender, GridViewRowEventArgs e)
        {
        }

        protected void GridView1_DataBinding(object sender, EventArgs e)
        {

            GridViewRow HeaderGridRow = new GridViewRow(0, 0, DataControlRowType.Header,
                                                        DataControlRowState.Insert);  //creating new Header Type 
            TableCell HeaderCell = new TableCell(); //creating HeaderCell
            HeaderCell.ColumnSpan = 4;
            HeaderGridRow.Cells.Add(HeaderCell);//Adding HeaderCell to header.

            var expenses = AssociationExpensesManager.GetByMonthAndYearNotDisabled(Association.Id, _year, _month).GroupBy(ee => ee.Id_ExpenseType).OrderBy(er => er.Key).ToList();
            foreach (var expense in expenses)
            {
                var HeaderCell2 = new TableCell();
                HeaderCell2.Text = expense.FirstOrDefault().ExpenseTypes.Name;
                HeaderCell2.ColumnSpan = expense.ToList().Count;
                HeaderGridRow.Cells.Add(HeaderCell2);//Adding HeaderCell to header.
            }

            TableCell HeaderCell3 = new TableCell(); //creating HeaderCell
            HeaderCell3.ColumnSpan = GridView1.Rows[0].Cells.Count - 4 - expenses.Select(c => c.ToList()).Count();
            HeaderGridRow.Cells.Add(HeaderCell3);//Adding HeaderCell to header.

            GridView1.Controls[0].Controls.AddAt(0, HeaderGridRow);
        }
        public override void VerifyRenderingInServerForm(Control control)
        {
            /* Confirms that an HtmlForm control is rendered for the specified ASP.NET
               server control at run time. */
        }
        protected void lblExpenseMeessageDownload_Click1(object sender, EventArgs e)
        {
            Response.ClearContent();
            Response.AddHeader("content-disposition", "attachment; filename=gvtoexcel.xls");
            Response.ContentType = "application/excel";
            System.IO.StringWriter sw = new System.IO.StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            GridView1.RenderControl(htw);
            Response.Write(sw.ToString());
            Response.End();
        }
    }
}