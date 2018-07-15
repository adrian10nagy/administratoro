
using System;
using Administratoro.BL.Constants;
using Administratoro.BL.Managers;

namespace Admin.Apartments
{
    public class ApartmentsBase : System.Web.UI.Page
    {
        protected override void OnInit(EventArgs e)
        {
            var apartment = Session[SessionConstants.LoginUser] as Administratoro.DAL.Apartments;
            if (apartment == null || !PartnerRightsManager.CanAccesRaportModule(apartment.Id))
            {
               // Response.Redirect("~/");
            }
        }
    }
}