using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Linq;
using Administratoro.BL.Constants;
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
            CleanFields();
            btnSave.Enabled = true;
            InitializeWhatCanPay();
            upMain.Visible = true;
            pnlSendEmail.Visible = true;
            lblValidationMessage.Text = string.Empty;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/");
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            decimal sumOfChecked;
            decimal totalToPay;

            if (decimal.TryParse(tbTotalToPay.Text, out totalToPay) && decimal.TryParse(tbSumOfChecked.Text, out sumOfChecked))
            {
                tbTotalToPay.Attributes.Add("style", "");

                if (sumOfChecked < totalToPay)
                {
                    lblValidationMessage.Text =
                        "Suma de pe chitanță este <b> mai mare</b> decât cea selectată. Suma de <b>" +
                        (totalToPay - sumOfChecked) + "</b> va fi calcuilată ca și plată în avans";
                    btnSave.Visible = false;
                    btnConfirm.Visible = true;
                    pnlSendEmail.Visible = true;
                }
                else if (sumOfChecked > totalToPay)
                {
                    lblValidationMessage.Text =
                        "Suma de pe chitanță este <b>mai mică</b> decât cea selectată, alege cheltuiala care va fi plătită parțial ramânând de plată suma de <b>" + (sumOfChecked - totalToPay) + "</b>";
                    InitializeWhatCanPayPartially();
                    btnSave.Visible = false;
                    btnConfirm.Visible = true;
                    pnlSendEmail.Visible = true;
                }
                else
                {
                    SavePayment();
                    CleanFields();
                    drpApartaments.SelectedValue = string.Empty;
                    upMain.Visible = false;
                    btnConfirm.Visible = false;
                    tbSumOfChecked.Text = string.Empty;
                    tbTotalToPay.Text = string.Empty;
                }
            }
            else
            {
                tbTotalToPay.Attributes.Add("style", "border-color:red");
            }
        }

        private void SavePayment()
        {
            decimal sumOfChecked;
            decimal totalToPay;
            int apartmentId;

            if (!decimal.TryParse(tbTotalToPay.Text, out totalToPay) ||
                !decimal.TryParse(tbSumOfChecked.Text, out sumOfChecked) ||
                !int.TryParse(drpApartaments.SelectedValue, out apartmentId))
            {
                return;
            }

            List<string> whatToPay = new List<string>();
            foreach (ListItem item in chbWhatToPay.Items)
            {
                if (!item.Selected) { continue; }
                whatToPay.Add(item.Value);
            }

            DateTime date;
            if (DateTime.TryParse(txtAddedDate.Text, out date))
            {
                RegistriesHomeManager.RegisterPay(sumOfChecked, totalToPay, apartmentId, whatToPay,
                    tbNr.Text, tbExplanations.Text, drpWhatTpPayPartially.SelectedValue, Association.Id, date);

                lblValidationMessage.Text = "Plată salvată cu succes!";
            }

        }

        private void InitializeWhatCanPay()
        {
            int apartmentId;
            if (!int.TryParse(drpApartaments.SelectedValue, out apartmentId))
            {
                return;
            }

            var apartmentDebts = ApartmentDebtsManager.GetUnpayed(apartmentId, false);

            pnlWhatToPay.Visible = true;
            chbWhatToPay.Visible = true;
            chbWhatToPay.Items.Clear();
            foreach (var apartmentsDebt in apartmentDebts)
            {
                var valueToPay = apartmentsDebt.RemainingToPay.HasValue ? apartmentsDebt.RemainingToPay.Value : apartmentsDebt.Value;
                chbWhatToPay.Items.Add(new ListItem
                {
                    Value = apartmentsDebt.Year + "!" + apartmentsDebt.Month + "!" + apartmentsDebt.Id_debtType + "!" + valueToPay,
                    Text = "Luna " + apartmentsDebt.Month + ", " + " cheltuiala: " + apartmentsDebt.Id_debtType + " , suma: " + valueToPay + " RON"
                });
            }
        }

        private void InitializeWhatCanPayPartially()
        {
            drpWhatTpPayPartially.Visible = true;
            drpWhatTpPayPartially.Items.Clear();

            foreach (ListItem item in chbWhatToPay.Items)
            {
                if (item.Selected)
                {
                    drpWhatTpPayPartially.Items.Add(new ListItem
                    {
                        Value = item.Value,
                        Text = item.Text
                    });
                }
            }
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

        protected void chbWhatToPay_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            tbExplanations.Text = string.Empty;
            decimal sum = 0;
            foreach (ListItem item in chbWhatToPay.Items)
            {
                if (item.Selected)
                {
                    tbExplanations.Text = tbExplanations.Text + item.Value;
                    var items = item.Value.Split('!');
                    decimal value;
                    if (items.Length == 4 && decimal.TryParse(items[3], out value))
                    {
                        sum = sum + value;
                    }
                }
            }

            tbSumOfChecked.Text = sum.ToString();

        }

        protected void bnConfirm_OnClick(object sender, EventArgs e)
        {
            SavePayment();
            CleanFields();
            drpApartaments.SelectedValue = string.Empty;
            upMain.Visible = false;
        }

        private void CleanFields()
        {
            tbSumOfChecked.Text = string.Empty;
            tbTotalToPay.Text = string.Empty;

            drpWhatTpPayPartially.Visible = false;
            drpWhatTpPayPartially.Items.Clear();

            chbWhatToPay.Visible = false;
            chbWhatToPay.Items.Clear();

            btnSave.Visible = true;
            btnConfirm.Visible = false;
            pnlSendEmail.Visible = false;
        }
    }
}