
namespace Admin
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Managers;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public partial class SiteMaster : MasterPage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            //var user = Session["LoginUser"] as User;
            //if (user == null || user.Id == 0)
            //{
            //    Response.Redirect("~/Account/Login.aspx");
            //}
            //else
            //{
            //    txtWelcomeUserName.Text = user.LastName + " " + user.FirstName;
            //    txtMenuTopUserName.Text = user.LastName + " " + user.FirstName;
            //    txtWelcomeLibrary.Text = "Biblioteca " + user.Library.Name;
            //    txtWelcomeLibraryFooter.Text = "Biblioteca " + user.Library.Name;
            //}
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            InitializeEsates();
        }

        private void InitializeEsates()
        {
            int partnerId = 4;
            var estates = EstatesManager.GetAllEstatesByPartner(partnerId);
            if(estates != null && estates.Count > 0)
            {
                Session[SessionConstants.SelectedEstate] = estates.First();
            }

            foreach (var item in estates)
            {
                drpMainEstate.Items.Add(new ListItem
                {
                    Text = item.Name,
                    Value = item.Id.ToString()
                });
            }
        }
    }
}