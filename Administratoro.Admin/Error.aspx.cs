
namespace Admin
{
    using Helpers.Constants;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public partial class Error : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if(!Page.IsPostBack)
            {
                var requestId = Request["errorId"];
                if (requestId == ErrorMessages.UserInvalid.ToString())
                {
                }
                else if (requestId == ErrorMessages.BookInvalid.ToString())
                {

                }
                lblMoreInfo.Text = "Dacă considerați acest mesaj ca fiind eronat vă rugăm să ne contactați având în vedere pașii urmați. <br> Aceasta ne ajută să îmbunătățim platforma pe viitor. Mulțumim!";
            }
        }
    }
}