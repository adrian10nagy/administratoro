
namespace Admin
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Managers;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.UI;

    public class BasePage : Page
    {
        public Estates Association
        {
            get
            {
                Estates result = null;
                if (Session[SessionConstants.SelectedAssociation] != null)
                {
                    result = (Estates)Session[SessionConstants.SelectedAssociation];
                }
                return result;
            }
        }

        public List<Estates> Associations
        {
            get
            {
                List<Estates> result = null;
                if (Session[SessionConstants.AllAssociations] != null)
                {
                    result = (List<Estates>)Session[SessionConstants.AllAssociations];
                }

                return result;
            }
        }

        public List<Administratoro.DAL.Expenses> Expenses
        {
            get
            {
                return ExpensesManager.GetAllExpenses();
            }
        }

        public void RefreshEstate()
        {
            Session[SessionConstants.SelectedAssociation] = AssociationsManager.GetById(Association.Id);
        }

    }
}