using System;
using System.Web.UI.WebControls;
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
            pnlWhatToPay.Visible = true;
            chbWhatToPay.Items.Clear();
            chbWhatToPay.Items.Add(new ListItem
            {
                Value = string.Empty,
                Text = "Luna Mai: " + "242 RON"
            });
            chbWhatToPay.Items.Add(new ListItem
            {
                Value = string.Empty,
                Text = "Luna iunie: " + "112 RON"
            }); chbWhatToPay.Items.Add(new ListItem
            {
                Value = string.Empty,
                Text = "Luna iulie: " + "242 RON"
            });
            chbWhatToPay.Items.Add(new ListItem
            {
                Value = string.Empty,
                Text = "Luna august: " + "112 RON"
            });
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