namespace Admin.Account
{
    using System;

    public partial class Settings : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            //txtErrorMessage.Text = "";
            //txtErrorMessage.Attributes.CssStyle.Add("color", "");
            //txtErrorMessage.Attributes.CssStyle.Add("font-weight", "");

            if (ValidatePassword())
            {
            //    var user = Session[SessionConstants.LoginUser] as User;

            //    if (UsersManager.UpdateUserPassword(user.Id, txtUserPasswordOld.Value, txtUserPasswordNew.Value, user.Library.Id))
            //    {
            //        txtErrorMessage.Text = "Parolă schimbată cu succes!";
            //        txtErrorMessage.Attributes.CssStyle.Add("color", "green");
            //    }
            //    else
            //    {
            //        txtErrorMessage.Text = "Parola gresita";
            //        txtErrorMessage.Attributes.CssStyle.Add("color", "red");
            //    }

            //    txtErrorMessage.Attributes.CssStyle.Add("font-weight", "bold");
            }
        }

        private bool ValidatePassword()
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(txtUserPasswordNew.Value) || txtUserPasswordNew.Value != txtUserPasswordNewConfirm.Value)
            {
                lblUserPasswordNew.Attributes.CssStyle.Add("color", "red");
                lblUserPasswordNewConfirm.Attributes.CssStyle.Add("color", "red");
                txtErrorMessage.Text = "Parolele nu corespund";
                isValid = false;
            }
            else
            {
                lblUserPasswordNew.Attributes.CssStyle.Add("color", "");
                lblUserPasswordNewConfirm.Attributes.CssStyle.Add("color", "");
            }

            return isValid;
        }
    }
}