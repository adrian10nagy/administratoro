
namespace Admin
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Managers;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using Administratoro.BL.Extensions;

    public partial class SiteMaster : MasterPage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            var partner = Session[SessionConstants.LoggedPartner] as Partners;
            if (partner != null)
            {
                InitializeEsates(partner);
                txtWelcomeUserName.Text = partner.Name;
            }
            else
            {
                Response.Redirect("~/Account/Login.aspx");
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        private void InitializeEsates(Partners partner)
        {
            var estate = Session[SessionConstants.SelectedAssociation] as Estates;
            var estates = Session[SessionConstants.AllAssociations] as List<Estates>;

            if (estate == null || estates == null)
            {
                estates = AssociationsManager.GetAllAssociationsByPartner(partner.Id);
                if (estates != null && estates.Count > 0)
                {
                    estate = estates.First();
                    Session[SessionConstants.SelectedAssociation] = estate;
                    Session[SessionConstants.AllAssociations] = estates;
                }
            }

            drpMainEstate.Items.Clear();
            foreach (var itemEstate in estates)
            {
                drpMainEstate.Items.Add(new ListItem
                {
                    Text = itemEstate.Name,
                    Value = itemEstate.Id.ToString(),
                    Selected = (itemEstate.Id == estate.Id)
                });
            }

            
            drpMainEstate.Items.Add(new ListItem
            {
                Text = "Adaugă o nouă asociație",
                Value = "-1"
            });
        }

        protected void drpMainEstate_SelectedIndexChanged(object sender, EventArgs e)
        {
            int? selectedEstate = drpMainEstate.SelectedValue.ToNullableInt();
            var partner = Session[SessionConstants.LoggedPartner] as Partners;

            if (selectedEstate.HasValue && selectedEstate.Value != -1)
            {
                var estates = AssociationsManager.GetAllAssociationsByPartner(partner.Id);
                var existingEstate = estates.FirstOrDefault(es => es.Id == selectedEstate.Value);
                if (existingEstate != null)
                {
                    Session[SessionConstants.SelectedAssociation] = existingEstate;
                    Response.Redirect("~/");
                }
                else
                {
                    // to do- redirect
                }
            }
            else
            {
                //Response.Redirect("~/Account/Login.aspx");
                Response.Redirect("~/Associations/New.aspx");
            }

            Response.Redirect(Request.RawUrl);
        }
    }
}