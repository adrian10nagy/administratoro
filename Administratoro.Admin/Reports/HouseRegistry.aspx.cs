using Administratoro.BL.Managers;
using System;
using System.Globalization;

namespace Admin.Reports
{
    public partial class HouseRegistry : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                txtDate.Text = DateTime.Now.ToString("dd/MM/yyy", CultureInfo.InvariantCulture);
               
            }
        }

        protected void btnGenerate_OnClick(object sender, EventArgs e)
        {
            DateTime date;
            if (DateTime.TryParse(txtDate.Text, out date))
            {
                var registryRaport = ReportingManager.GenerateDailyReport(Association.Id, date);
                var myName = Server.UrlEncode("JurnalDeCasa" + "_" + date.ToShortDateString() + ".pdf");
                Response.Clear();
                Response.Buffer = true;
                Response.AddHeader("content-disposition", "attachment; filename=" + myName);
                Response.ContentType = "application/vnd.ms-excel";
                Response.BinaryWrite(registryRaport);
                Response.Flush();
                Response.SuppressContent = true;
            }
        }
    }
}