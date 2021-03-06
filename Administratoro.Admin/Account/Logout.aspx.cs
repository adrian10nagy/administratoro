﻿
namespace Admin.Account
{
    using Administratoro.BL.Constants;
    using System;

    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session.Remove(SessionConstants.LoggedPartner);
            Session.Remove(SessionConstants.AllAssociations);
            Session.Remove(SessionConstants.SelectedAssociation);

            Response.Redirect("~/Account/Login.aspx?Message=Logout");
        }
    }
}