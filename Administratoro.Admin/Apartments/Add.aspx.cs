
namespace Admin.Tenants
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Extensions;
    using Administratoro.BL.Managers;
    using Administratoro.DAL;
    using Helpers.Constants;
    using System;
    using System.Linq;
    using System.Web.UI.WebControls;

    public partial class Add : TenantsBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                PopulateStairCase();
                if (!string.IsNullOrEmpty(Request["tenantid"]))
                {
                    var tenantid = Request["tenantid"].ToNullableInt();
                    if (tenantid != null && tenantid != 0)
                    {
                        var user = TenantsManager.GetById(tenantid.Value);
                        if (user != null)
                        {
                            userName.Value = user.Name;
                            userExtraInfo.Value = user.ExtraInfo;
                            userPhone.Value = user.Telephone;
                            userEmail.Value = user.Email;
                            userDependents.Value = user.Dependents.ToString();
                            usercota.Value = user.CotaIndiviza.ToString();
                            userNr.Value = user.Number.ToString();
                            btnSave.Text = "Actualizează datele proprietatății";
                            lblUserId.Text = Request["tenantid"];
                            userStairCase.SelectedValue = (user.Id_StairCase != null) ? user.Id_StairCase.ToString() : null;
                        }
                        else
                        {
                            Response.Redirect("~/Error.aspx?errorId=" + ErrorMessages.UserInvalid);
                        }
                    }
                    else
                    {
                        Response.Redirect("~/Error.aspx?errorId=" + ErrorMessages.UserInvalid);
                    }
                }
            }
        }

        private void PopulateStairCase()
        {
            var estate = (Estates)Session[SessionConstants.SelectedEstate];
            var staircases = StairCasesManager.GetAllByEstate(estate.Id);
            if (estate.HasStaircase)
            {
                ListItem defaultNull = new ListItem
                {
                    Value = null
                };
                userStairCase.Items.Add(defaultNull);
                divStaircase.Visible = true;

                foreach (var item in staircases)
                {
                    ListItem li = new ListItem
                    {
                        Text = item.Value,
                        Value = item.Id.ToString()
                    };
                    userStairCase.Items.Add(li);
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!this.ValidateInputs())
            {
                return;
            }

            var estate = (Estates)Session[SessionConstants.SelectedEstate];
            var cota = usercota.Value.ToNullableDecimal();

            var tenant = new Tenants
            {
                Name = userName.Value,
                Dependents = userDependents.Value.ToNullableInt().Value,
                ExtraInfo = userExtraInfo.Value,
                CotaIndiviza = (cota.HasValue) ? cota.Value : 0,
                Number = userNr.Value.ToNullableInt().Value,
                Telephone = userPhone.Value,
                Email = userEmail.Value,
                CreatedDate = DateTime.Now,
                id_Estate = estate.Id,
                Password = "dasd",
                Id_StairCase = userStairCase.SelectedValue.ToNullableInt()
            };

            if (!string.IsNullOrEmpty(lblUserId.Text) && lblUserId.Text.ToNullableInt() != 0)
            {
                tenant.Id = lblUserId.Text.ToNullableInt().Value;
                TenantsManager.Update(tenant);
            }
            else
            {
                TenantsManager.Add(tenant);
                lblStatus.Text = FlowMessages.TenantAddSuccess;
                lblStatus.CssClass = "SuccessBox";
                CleanFields();
            }

            Response.Redirect("~/Apartments/Manage.aspx?Message=UserUpdatedSuccess");
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Apartments/Manage.aspx");
        }

        private bool ValidateInputs()
        {
            bool isValid = true;

            userDependents.Attributes.CssStyle.Add("color", "");

            if (!userDependents.Value.ToNullableInt().HasValue)
            {
                userDependents.Attributes.CssStyle.Add("color", "red");
                isValid = false;
            }

            userName.Attributes.CssStyle.Add("color", "");

            if (string.IsNullOrEmpty(userName.Value))
            {
                userName.Attributes.CssStyle.Add("color", "red");
                isValid = false;
            }

            usercota.Attributes.CssStyle.Add("color", "");
            decimal userCotaValue;
            if (string.IsNullOrEmpty(usercota.Value) || !decimal.TryParse(usercota.Value, out userCotaValue))
            {
                usercota.Attributes.CssStyle.Add("color", "red");
                isValid = false;
            }

            userNr.Attributes.CssStyle.Add("color", "");
            int userNrValue;
            if (string.IsNullOrEmpty(userNr.Value) || !int.TryParse(userNr.Value, out userNrValue))
            {
                userNr.Attributes.CssStyle.Add("color", "red");
                isValid = false;
            }

            return isValid;
        }

        private void CleanFields()
        {
            userName.Value = string.Empty;
            userDependents.Value = string.Empty;
            userPhone.Value = string.Empty;
            userEmail.Value = string.Empty;
        }
    }
}