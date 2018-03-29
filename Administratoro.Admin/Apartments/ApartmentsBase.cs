
namespace Admin.Tenants
{
    using Administratoro.BL.Constants;
    using System;
    using Administratoro.DAL;
    using Administratoro.BL.Managers;

    public class ApartmentsBase : System.Web.UI.Page
    {
        protected override void OnInit(EventArgs e)
        {
            var apartment = Session[SessionConstants.LoginUser] as Apartments;
            if (apartment == null || !PartnerRightsManager.CanAccesRaportModule(apartment.Id))
            {
               // Response.Redirect("~/");
            }
        }
    }
}