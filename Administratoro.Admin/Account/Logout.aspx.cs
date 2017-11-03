
namespace Admin.Account
{
    using System;

    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Session.Remove(SessionConstants.LoginUser);
            //CacheManager.ClearAllCache();

            Response.Redirect("~/Account/Login.aspx?Message=Logout");
        }
    }
}