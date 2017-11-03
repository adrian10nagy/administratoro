
namespace Admin.Tenants
{
    using Administratoro.BL.Constants;
    using System;
    using Administratoro.DAL;
    using Administratoro.BL.Managers;

    public class TenantsBase : System.Web.UI.Page
    {
        protected override void OnInit(EventArgs e)
        {
            var tenant = Session[SessionConstants.LoginUser] as Tenants;
            if (tenant == null || !PartnerRightsManager.CanAccesRaportModule(tenant.Id))
            {
               // Response.Redirect("~/");
            }
        }
    }
}