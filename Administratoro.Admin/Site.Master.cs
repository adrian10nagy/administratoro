
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
            var association = Session[SessionConstants.SelectedAssociation] as Administratoro.DAL.Associations;
            var associations = Session[SessionConstants.AllAssociations] as List<Administratoro.DAL.Associations>;

            if (association == null || associations == null)
            {
                associations = AssociationsManager.GetAllAssociationsByPartner(partner.Id);
                if (associations != null && associations.Count > 0)
                {
                    association = associations.First();
                    Session[SessionConstants.SelectedAssociation] = association;
                    Session[SessionConstants.AllAssociations] = associations;
                }
            }

            drpMainEstate.Items.Clear();
            foreach (var itemEstate in associations)
            {
                drpMainEstate.Items.Add(new ListItem
                {
                    Text = itemEstate.Name,
                    Value = itemEstate.Id.ToString(),
                    Selected = (itemEstate.Id == association.Id)
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
            int? selectedAssociation = drpMainEstate.SelectedValue.ToNullableInt();
            var partner = Session[SessionConstants.LoggedPartner] as Partners;

            if (selectedAssociation.HasValue && selectedAssociation.Value != -1)
            {
                var associations = AssociationsManager.GetAllAssociationsByPartner(partner.Id);
                var existingAssociation = associations.FirstOrDefault(es => es.Id == selectedAssociation.Value);
                if (existingAssociation != null)
                {
                    Session[SessionConstants.SelectedAssociation] = existingAssociation;
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