
namespace Admin.Account
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Managers;
    using Administratoro.DAL;
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
                Partners partner = PartnersManager.Login(txtUserName.Value, txtUserPassword.Value);
                if (partner == null)
                {
                    txtUserName.Style.Add("border", "1px solid red");
                    txtUserPassword.Style.Add("border", "1px solid red");
                    lblMessageLogin.Visible = true;
                    lblMessageLogin.Text = "Logare nereușită<br> Combinație email - parolă incorectă";
                    lblMessageLogin.Style.Add("color", "red");
                }
                else
                {
                    Session[SessionConstants.LoggedPartner] = partner;
                    //add to cookie
                    Response.Redirect("~/");
                }
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