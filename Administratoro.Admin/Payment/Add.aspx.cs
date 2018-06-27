using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Administratoro.BL.Extensions;
using Administratoro.BL.Managers;

namespace Admin.Payment
{
    public partial class Add : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                InitializeYearMonth();
            }
        }

        protected void drpApartament_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnSave.Enabled = true;
            InitializeWhatCanPay();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/");
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {

        }

        private void InitializeWhatCanPay()
        {
            
        }

        private void InitializeYearMonth()
        {
            drpApartaments.Items.Clear();

            drpApartaments.Items.Add(new ListItem
            {
                Value = string.Empty,
                Text = string.Empty
            });

            var apartments = ApartmentsManager.Get(Association.Id);
            foreach (var apartment in apartments)
            {
                drpApartaments.Items.Add(new ListItem
                {
                    Value = apartment.Id.ToString(),
                    Text = "Ap. " + apartment.Number + " : " + apartment.Name
                });
            }
        }
    }
}