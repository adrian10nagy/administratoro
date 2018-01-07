
namespace Admin.Tenants
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using Helpers.Constants;
    using Administratoro.BL.Constants;
    using Administratoro.DAL;
    using Administratoro.BL.Managers;


    public partial class Manage : TenantsBase
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Request["message"]) && Request["message"] == "UserUpdatedSuccess")
            {
                lblMessage.Text = FlowMessages.TenantUpdatedSuccess;
                lblMessage.CssClass = "SuccessBox";
            }
            
            if (!Page.IsPostBack)
            {
                InitializeUsersTable();
            }
        }

        
        private void InitializeUsersTable()
        {
            var sessionUser = Session[SessionConstants.LoginUser] as Tenants;
            var estate = Session[SessionConstants.SelectedEstate] as Estates;

            var tentants = ApartmentsManager.GetAllByEstateId(estate.Id);

            foreach (Tenants user in tentants)
            {
                TableRow row = new TableRow();

                HyperLink linkEditUser = new HyperLink
                {
                    NavigateUrl = "~/Apartments/Add.aspx?apartmentid=" + user.Id,
                    CssClass = "toClickOn",
                    Text = "Editează"
                };
                TableCell userEditCell = new TableCell();
                userEditCell.Controls.Add(linkEditUser);
                row.Cells.Add(userEditCell);

                TableCell tentantNr = new TableCell
                {
                    Text = user.Number.ToString()
                };
                row.Cells.Add(tentantNr);

                TableCell tentantName = new TableCell
                {
                    Text = user.Name
                };
                row.Cells.Add(tentantName);

                TableCell userPhone = new TableCell
                {
                    Text = user.Telephone
                };
                row.Cells.Add(userPhone);

                TableCell userEmail = new TableCell
                {
                    Text = user.Email
                };
                row.Cells.Add(userEmail);

                TableCell userDependents = new TableCell
                {
                    Text = user.Dependents.ToString()
                };
                row.Cells.Add(userDependents);

                TableCell userSurface = new TableCell
                {
                    Text = user.CotaIndiviza.ToString()
                };
                row.Cells.Add(userSurface);

                datatableResponsive.Rows.Add(row);
            }
        }
    }
}