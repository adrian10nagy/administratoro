
namespace Admin.Account
{
    using System;
    using System.Web.UI;

    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                if (Request["Message"] == "Logout")
                {
                    lblMessageLogin.Text = "Delogare reușită";
                    lblMessageLogin.Style.Add("color", "green");
                    lblMessageLogin.Style.Add("font-size", "18px");
                    lblMessageLogin.Visible = true;
                }
            }
        }

        protected void btnAccountLogin_Click(object sender, EventArgs e)
        {
            if (this.ValidateLogin())
            {
                //User user = UsersManager.GetUserForLogin(txtUserName.Value, txtUserPassword.Value);
                //if (user == null || !UserRightsManager.CanLogin(user.Id))
                //{
                //    txtUserName.Style.Add("border", "1px solid red");
                //    txtUserPassword.Style.Add("border", "1px solid red");
                //    lblMessageLogin.Visible = true;
                //    lblMessageLogin.Text = "Logare nereușită<br> Combinație utilizator - parolă incorectă, acces restricționat";
                //    lblMessageLogin.Style.Add("color", "red");
                //}
                //else
                //{
                //    Session[SessionConstants.LoginUser] = user;
                //    //add to cookie
                //    Response.Redirect("~/");
                //}
            }
        }

        private bool ValidateLogin()
        {
            var result = true;

            if (string.IsNullOrEmpty(txtUserName.Value) || string.IsNullOrEmpty(txtUserPassword.Value))
            {
                txtUserName.Style.Add("border", "1px solid red");
                txtUserPassword.Style.Add("border", "1px solid red");
                lblMessageLogin.Visible = true;
                lblMessageLogin.Text = "Introduceți utilizator și parolă";
                lblMessageLogin.Style.Add("color", "red");
                result = false;
            }

            return result;
        }
    }
}