using Administratoro.BL.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Admin.Reports
{
    public partial class ApartmentRegistry : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                InitializeRegistryTypes();
                InitializeApartments();

            }
        }

        private void InitializeApartments()
        {
            var ap = ApartmentsManager.Get(Association.Id);

            drpApartments.Items.Add(new ListItem
            {
                Text = string.Empty,
                Value = "-1"
            });

            foreach (var apartment in ap)
            {
                drpApartments.Items.Add(new ListItem
                {
                    Value = apartment.Id.ToString(),
                    Text = apartment.Number + ": " + apartment.Name
                });
            }
        }

        private void InitializeRegistryTypes()
        {

            drpRegType.Items.Add(new ListItem
            {
                Text = string.Empty,
                Value = "-1"
            });

            drpRegType.Items.Add(new ListItem
            {
                Text = "Fond de rulment",
                Value = "0"
            });

            drpRegType.Items.Add(new ListItem
            {
                Text = "Fond de reparații",
                Value = "1"
            });
        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            int apartmentId;
            int reportType;

            if(!int.TryParse(drpApartments.SelectedValue, out apartmentId) ||
                !int.TryParse(drpRegType.SelectedValue, out reportType)){return;}

            byte[] registryRaport = { };
            if(reportType == 0)
            {
                registryRaport = ReportingManager.GenerateFondsReport(apartmentId, Administratoro.BL.Constants.DebtType.Repairfond);
            }
            else if (reportType == 1)
            {
                registryRaport = ReportingManager.GenerateFondsReport(apartmentId, Administratoro.BL.Constants.DebtType.RulmentFond);
            }
            else
            {
                return;
            }


            var fileName = Server.UrlEncode("Raport_" + drpRegType.SelectedValue + "_apartament" + 
                drpApartments.SelectedValue + "_" + DateTime.Now.ToShortDateString() + ".pdf");
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
            Response.ContentType = "application/vnd.ms-excel";
            Response.BinaryWrite(registryRaport);
            Response.Flush();
            Response.SuppressContent = true;
        }
    }
}