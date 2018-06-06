
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
        public Administratoro.DAL.Associations Association
        {
            get
            {
                Administratoro.DAL.Associations result = null;
                if (Session[SessionConstants.SelectedAssociation] != null)
                {
                    result = (Administratoro.DAL.Associations)Session[SessionConstants.SelectedAssociation];
                }

                return result;
            }
        }

        public List<Administratoro.DAL.Associations> Associations
        {
            get
            {
                List<Administratoro.DAL.Associations> result = null;
                if (Session[SessionConstants.AllAssociations] != null)
                {
                    result = (List<Administratoro.DAL.Associations>)Session[SessionConstants.AllAssociations];
                }

                return result;
            }
        }

        public static List<Administratoro.DAL.Expenses> Expenses
        {
            get
            {
                return ExpensesManager.GetAllExpenses().ToList();
            }
        }

        public void RefreshAssociation()
        {
            Session[SessionConstants.SelectedAssociation] = AssociationsManager.GetById(Association.Id);
        }

    }
}